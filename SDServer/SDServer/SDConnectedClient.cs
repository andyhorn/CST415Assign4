// SDConnectedClient.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 
// Extended by Andy Horn
// October-November 2019

using System;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.IO;

namespace SDServer
{
    class SDConnectedClient
    {
        // represents a single connected sd client
        // each client will have its own socket and thread while its connected
        // client is given it's socket from the SDServer when the server accepts the connection
        // this class creates it's own thread
        // the client's thread will process messages on the client's socket until it disconnects
        // NOTE: an sd client can connect/send messages/disconnect many times over it's lifetime

        private Socket clientSocket;
        private NetworkStream stream;
        private StreamReader reader;
        private StreamWriter writer;
        private Thread clientThread;
        private SessionTable sessionTable;      // server's session table
        private ulong sessionId;                // session id for this session, once opened or resumed

        public SDConnectedClient(Socket clientSocket, SessionTable sessionTable)
        {
            // save the client's socket
            this.clientSocket = clientSocket;

            // at this time, there is no stream, reader, write or thread
            reader = null;
            writer = null;
            stream = null;
            clientThread = null;

            // save the server's session table
            this.sessionTable = sessionTable;

            // at this time, there is no session open
            sessionId = 0;            
        }

        public void Start()
        {
            // called by the main thread to start the clientThread and process messages for the client
            // create and start the clientThread, pass in a reference to this class instance as a parameter
            clientThread = new Thread(ThreadProc);
            clientThread.Start(this);
        }

        private static void ThreadProc(Object param)
        {
            // the procedure for the clientThread
            // when this method returns, the clientThread will exit

            // the param is a SDConnectedClient instance
            // start processing messages with the Run() method
            var connectedClient = param as SDConnectedClient;
            connectedClient.Run();
        }

        private void Run()
        {
            // this method is executed on the clientThread

            try
            {
                // create network stream, reader and writer over the socket
                stream = new NetworkStream(clientSocket);
                reader = new StreamReader(stream, UTF8Encoding.ASCII);
                writer = new StreamWriter(stream, UTF8Encoding.ASCII);
                
                // process client requests
                bool done = false;
                while (!done)
                {
                    // receive a message from the client
                    Console.WriteLine($"[{clientThread.ManagedThreadId.ToString()}] Waiting for message from client...");
                    string msg = reader.ReadLine();
                    if (msg == null)
                    {
                        // no message means the client disconnected
                        // remember that the client will connect and disconnect as desired
                        Console.WriteLine($"[{clientThread.ManagedThreadId.ToString()}] Client disconnected, exiting...");
                        done = true;
                    }
                    else
                    {
                        Console.WriteLine($"[{clientThread.ManagedThreadId.ToString()}] Message received: {msg}");
                        // handle the message
                        switch (msg)
                        {
                            case "open":
                            {
                                HandleOpen();
                            }
                            break;

                            case "resume":
                            {
                                HandleResume();
                            }
                            break;

                            case "close":
                            {
                                HandleClose();
                            }
                            break;

                            case "get":
                            {
                                HandleGet();
                            }
                            break;

                            case "post":
                            {
                                HandlePost();
                            }
                            break;

                            default:
                            {
                                // error handling for an invalid message
                                Console.WriteLine($"[{clientThread.ManagedThreadId.ToString()}] Invalid message received: {msg}");
                                // this client is too broken to waste our time on!
                                SendError("Invalid message");
                                done = true;
                            }
                            break;
                        }
                    }
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine("[" + clientThread.ManagedThreadId.ToString() + "] " + "Error on client socket, closing connection: " + se.Message);
            }
            catch (IOException ioe)
            {
                Console.WriteLine("[" + clientThread.ManagedThreadId.ToString() + "] " + "IO Error on client socket, closing connection: " + ioe.Message);
            }

            // close the client's writer, reader, network stream and socket
            writer.Close();
            reader.Close();
            stream.Close();
            clientSocket.Close();
        }

        private void HandleOpen()
        {
            // handle an "open" request from the client

            // if no session currently open, then...
            if (sessionId == 0)
            {
                try
                {
                    // ask the SessionTable to open a new session and save the session ID
                    sessionId = sessionTable.OpenSession();
                    Console.WriteLine($"[{clientThread.ManagedThreadId.ToString()}] Opening new session: {sessionId}");

                    // send accepted message, with the new session's ID, to the client
                    SendAccepted(sessionId);
                }
                catch (SessionException se)
                {
                    SendError(se.Message);
                }
                catch (Exception ex)
                {
                    SendError(ex.Message);
                }
            }
            else
            {
                // error!  the client already has a session open!
                SendError("Session already open!");
            }
        }

        private void HandleResume()
        {
            // handle a "resume" request from the client

            // get the sessionId that the client just asked us to resume
            var resumeSessionId = ulong.Parse(reader.ReadLine());
            Console.WriteLine($"[{clientThread.ManagedThreadId.ToString()}] Client asked to resume session {resumeSessionId}");
            
            try
            {
                // if we don't have a session open currently for this client...
                if (sessionId == 0)
                {
                    // try to resume the session in the session table
                    // if success, remember the session that we're now using and send accepted to client
                    if (sessionTable.ResumeSession(resumeSessionId))
                    {
                        sessionId = resumeSessionId;
                        SendAccepted(sessionId);
                    }
                    // if failed to resume session, send rejectetd to client
                    else
                    {
                        SendRejected("Invalid session id");
                    }

                }
                else
                {
                    // error! we already have a session open
                    SendError("Session already open, cannot resume!");
                }
            }
            catch (SessionException se)
            {
                SendError(se.Message);
            }
            catch (Exception ex)
            {
                SendError(ex.Message);
            }
        }

        private void HandleClose()
        {
            // handle a "close" request from the client

            // get the sessionId that the client just asked us to close
            var closedSessionId = ulong.Parse(reader.ReadLine());
            Console.WriteLine($"[{clientThread.ManagedThreadId.ToString()}] Client asked to close session id {closedSessionId}");
            
            try
            {
                // close the session in the session table
                sessionTable.CloseSession(closedSessionId);

                // send closed message back to client
                SendClosed(closedSessionId);

                // record that this client no longer has an open session
                if (sessionId == closedSessionId)
                {
                    Console.WriteLine($"[{clientThread.ManagedThreadId.ToString()}] Client no longer has an open session");
                    sessionId = 0;
                }
            }
            catch (SessionException se)
            {
                SendError(se.Message);
            }
            catch (Exception ex)
            {
                SendError(ex.Message);
            }
        }

        private void HandleGet()
        {
            // handle a "get" request from the client

            // if the client has a session open
            if (sessionId != 0)
            {
                try
                {
                    string documentContent = string.Empty;

                    // get the document name from the client
                    var documentName = reader.ReadLine();

                    if (string.IsNullOrEmpty(documentName) || documentName.Equals("/"))
                    {
                        throw new Exception("Document name cannot be empty");
                    }

                    // test if the client is requesting a file or a session variable
                    if (documentName.StartsWith("/"))
                    {
                        // find the file on disk
                        var filePath = $"{Environment.CurrentDirectory}{documentName.Replace('/', Path.DirectorySeparatorChar)}";

                        if (!File.Exists(filePath))
                        {
                            throw new Exception($"File {documentName} does not exist");
                        }

                        // read the contents
                        documentContent = File.ReadAllText(filePath);
                    }
                    else
                    {
                        // get the document content from the session table
                        documentContent = sessionTable.GetSessionValue(sessionId, documentName);
                    }

                    // send success and document to the client
                    SendSuccess(documentName, documentContent);
                }
                catch (SessionException se)
                {
                    SendError(se.Message);
                }
                catch (Exception ex)
                {
                    SendError(ex.Message);
                }
            }
            else
            {
                // error, cannot get without a session
                SendError($"Error, open session not found!");
            }
        }

        private void HandlePost()
        {
            // handle a "post" request from the client

            // if the client has a session open
            if (sessionId != 0)
            {
                try
                {
                    // get the document name, content length and contents from the client
                    var documentName = reader.ReadLine();
                    var documentLength = int.Parse(reader.ReadLine());
                    var documentContents = ReceiveDocument(documentLength);

                    // put the document into the session
                    sessionTable.PutSessionValue(sessionId, documentName, documentContents);

                    // send success to the client
                    SendSuccess();
                }
                catch (SessionException se)
                {
                    SendError(se.Message);
                }
                catch (Exception ex)
                {
                    SendError(ex.Message);
                }
            }
            else
            {
                // error, cannot post without a session
                SendError($"No session found with ID {sessionId}, cannot post document");
            }
        }

        private void SendAccepted(ulong sessionId)
        {
            // send accepted message to SD client, including session id of now open session
            writer.WriteLine("accepted");
            writer.WriteLine(sessionId.ToString());
            writer.Flush();
            Console.WriteLine($"[{clientThread.ManagedThreadId.ToString()}] Sent 'accepted' to client for session ID: {sessionId.ToString()}");
        }

        private void SendRejected(string reason)
        {
            // send rejected message to SD client, including reason for rejection
            writer.WriteLine("rejected");
            writer.WriteLine(reason);
            writer.Flush();
            Console.WriteLine($"[{clientThread.ManagedThreadId.ToString()}] Sent 'rejected' to client, reason: {reason}");
        }

        private void SendClosed(ulong sessionId)
        {
            // send closed message to SD client, including session id that was just closed
            writer.WriteLine("closed");
            writer.WriteLine(sessionId);
            writer.Flush();
            Console.WriteLine($"[{clientThread.ManagedThreadId.ToString()}] Sent 'closed' to client for session id: {sessionId.ToString()}");
        }

        private void SendSuccess()
        {
            // send sucess message to SD client, with no further info
            // NOTE: in response to a post request
            writer.WriteLine("success");
            writer.Flush();

            Console.WriteLine($"[{clientThread.ManagedThreadId}] Sent 'success' for session {sessionId}");
        }

        private void SendSuccess(string documentName, string documentContent)
        {
            // send success message to SD client, including retrieved document name, length and content
            // NOTE: in response to a get request
            writer.WriteLine("success");
            writer.WriteLine(documentName);
            writer.WriteLine(documentContent.Length);
            writer.Write(documentContent);
            writer.Flush();

            Console.WriteLine($"[{clientThread.ManagedThreadId}] Sent 'success' with {documentContent.Length} bytes of '{documentName}' for session: {sessionId}");
        }

        private void SendError(string errorString)
        {
            // send error message to SD client, including error string
            writer.WriteLine("error");
            writer.WriteLine(errorString);
            writer.Flush();
            Console.WriteLine($"[{clientThread.ManagedThreadId.ToString()}] Sent error to client: {errorString}");
        }

        private string ReceiveDocument(int length)
        {
            // receive a document from the SD client, of expected length
            // NOTE: as part of processing a post request

            // read from the reader until we've received the expected number of characters
            // accumulate the characters into a string and return those when we got enough

            int bytesLeftToRead = length;
            string contents = "";

            while (bytesLeftToRead > 0)
            {
                char[] buffer = new char[bytesLeftToRead];
                int bytesRead = reader.Read(buffer, 0, bytesLeftToRead);
                string s = new string(buffer);

                contents += s;
                bytesLeftToRead -= bytesRead;
            }

            Console.WriteLine($"Received {contents.Length} bytes of data");

            return contents;
        }
    }
}
