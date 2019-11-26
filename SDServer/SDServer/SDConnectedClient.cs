// SDConnectedClient.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 

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
            // TODO: SDConnectedClient.SDConnectedClient()

            // save the client's socket
            
            // at this time, there is no stream, reader, write or thread
            
            // save the server's session table
            
            // at this time, ther eis no session open
            
        }

        public void Start()
        {
            // TODO: SDConnectedClient.Start()

            // called by the main thread to start the clientThread and process messages for the client

            // create and start the clientThread, pass in a reference to this class instance as a parameter

        }

        private static void ThreadProc(Object param)
        {
            // TODO: SDConnectedClient.ThreadProc()

            // the procedure for the clientThread
            // when this method returns, the clientThread will exit

            // the param is a SDConnectedClient instance
            // start processing messages with the Run() method

        }

        private void Run()
        {
            // TODO: SDConnectedClient.Run()

            // this method is executed on the clientThread

            try
            {
                // create network stream, reader and writer over the socket
                
                // process client requests
                bool done = false;
                while (!done)
                {
                    // receive a message from the client
                    string msg = reader.ReadLine();
                    if (msg == null)
                    {
                        // no message means the client disconnected
                        // remember that the client will connect and disconnect as desired
                        
                    }
                    else
                    {
                        // handle the message
                        switch (msg)
                        {
                            case "open":
                                break;

                            case "resume":
                                break;

                            case "close":
                                break;

                            case "get":
                                break;

                            case "post":
                                break;

                            default:
                                {
                                    // error handling for an invalid message
                                    
                                    // this client is too broken to waste our time on!
                                    
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
            
        }

        private void HandleOpen()
        {
            // TODO: SDConnectedClient.HandleOpen()

            // handle an "open" request from the client

            // if no session currently open, then...
            if (sessionId == 0)
            {
                try
                {
                    // ask the SessionTable to open a new session and save the session ID
                    
                    // send accepted message, with the new session's ID, to the client
                    
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
            // TODO: SDConnectedClient.HandleResume()

            // handle a "resume" request from the client

            // get the sessionId that the client just asked us to resume
            
            try
            {
                // if we don't have a session open currently for this client...
                if (sessionId == 0)
                {
                    // try to resume the session in the session table
                    // if success, remember the session that we're now using and send accepted to client
                    
                    // if failed to resume session, send rejectetd to client

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
            // TODO: SDConnectedClient.HandleClose()

            // handle a "close" request from the client

            // get the sessionId that the client just asked us to close
            
            try
            {
                // close the session in the session table
                // send closed message back to client
                // record that this client no longer has an open session

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
            // TODO: SDConnectedClient.HandleGet()

            // handle a "get" request from the client

            // if the client has a session open
            if (sessionId != 0)
            {
                try
                {
                    // get the document name from the client
                    
                    // get the document content from the session table
                    
                    // send success and document to the client
                    
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
                
            }
        }

        private void HandlePost()
        {
            // TODO: SDConnectedClient.HandlePost()

            // handle a "post" request from the client

            // if the client has a session open
            if (sessionId != 0)
            {
                try
                {
                    // get the document name, content length and contents from the client
                    
                    // put the document into the session
                    
                    // send success to the client
                    
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
                
            }
        }

        private void SendAccepted(ulong sessionId)
        {
            // TODO: SDConnectedClient.SendAccepted()

            // send accepted message to SD client, including session id of now open session
            
        }

        private void SendRejected(string reason)
        {
            // TODO: SDConnectedClient.SendRejected()

            // send rejected message to SD client, including reason for rejection
            
        }

        private void SendClosed(ulong sessionId)
        {
            // TODO: SDConnectedClient.SendClosed()

            // send closed message to SD client, including session id that was just closed
            
        }

        private void SendSuccess()
        {
            // TODO: SDConnectedClient.SendSuccess()

            // send sucess message to SD client, with no further info
            // NOTE: in response to a post request
            
        }

        private void SendSuccess(string documentName, string documentContent)
        {
            // TODO: SDConnectedClient.SendSuccess(documentName, documentContent)

            // send success message to SD client, including retrieved document name, length and content
            // NOTE: in response to a get request
            
        }

        private void SendError(string errorString)
        {
            // TODO: SDConnectedClient.SendError()

            // send error message to SD client, including error string
            
        }

        private string ReceiveDocument(int length)
        {
            // TODO: SDConnectedClient.ReceiveDocument()

            // receive a document from the SD client, of expected length
            // NOTE: as part of processing a post request

            // read from the reader until we've received the expected number of characters
            // accumulate the characters into a string and return those when we got enough
            
            return "TODO";
        }
    }
}
