// FTConnectedClient.cs
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

namespace FTServer
{
    class FTConnectedClient
    {
        // represents a single connected ft client that wants directory contents from the server
        // each client will have its own socket and thread
        // client is given it's socket from the FTServer when the server accepts the connection
        // the client class creates it's own thread
        // the client's thread will process messages on the client's socket

        private Socket clientSocket;
        private NetworkStream stream;
        private StreamReader reader;
        private StreamWriter writer;
        private Thread clientThread;

        public FTConnectedClient(Socket clientSocket)
        {
            // save the client's socket
            this.clientSocket = clientSocket;

            // at this time, there is no stream, reader, write or thread
            stream = null;
            reader = null;
            writer = null;
            clientThread = null;            
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

            // the param is a FTConnectedClient instance
            // start processing messages with the Run() method
            var client = (FTConnectedClient)param;
            client.Run();
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

                Console.WriteLine($"[{clientThread.ManagedThreadId.ToString()}] Stream, reader, and writer initialized");
                
                // process client requests
                bool done = false;
                while (!done)
                {
                    // receive a message from the client
                    var message = reader.ReadLine();
                    Console.WriteLine($"[{clientThread.ManagedThreadId.ToString()}] Received message: {message}");

                    switch (message)
                    {
                        case "exit":
                        {
                            // client is done, close its socket and quit the thread
                            done = true;
                        }
                        break;

                        case "get":
                        {
                            // get directoryName
                            var directoryName = reader.ReadLine();

                            if (directoryName == null)
                            {
                                SendError("Empty directory");
                                break;
                            }

                            Console.WriteLine($"[{clientThread.ManagedThreadId.ToString()}] Client asked for directory: {directoryName}");

                            // retrieve directory contents and send all the files
                            var directory = new DirectoryInfo(directoryName);

                            // if directory does not exist! send an error!
                            if (!directory.Exists)
                            {
                                SendError($"Directory {directoryName} does not exist");
                                break;
                            }

                            // if directory exists, send each file to the client
                            // for each file...
                            foreach (var file in directory.GetFiles())
                            {
                                // make sure it's a .txt file
                                if (file.Extension != ".txt")
                                {
                                    Console.WriteLine($"[{clientThread.ManagedThreadId.ToString()}] File {file.Name} is not a .txt file, skipping...");
                                    continue;
                                }

                                // get the file's name
                                var fileName = file.Name;
                                SendFileName(fileName, file.Length);

                                // get the file contents
                                var contents = file.OpenText().ReadToEnd();
                                SendFileContents(contents);
                            }

                            // send done after last file
                            SendDone();
                        }
                        break;

                        default: // invalid message
                        {
                            // error handling for an invalid message
                            SendError($"Invalid request: {message}");

                            // this client is too broken to waste our time on!
                            // quit processing messages and disconnect
                            Console.WriteLine($"[{clientThread.ManagedThreadId.ToString()}] Invalid request received, closing connection to client");
                            done = true;
                        }
                        break;
                    }
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine("[" + clientThread.ManagedThreadId.ToString() + "] " + "Error on client socket, closing connection: " + se.Message);
            }

            // close the client's writer, reader, network stream and socket
            reader.Close();
            writer.Close();
            stream.Close();
            clientSocket.Close();

            Console.WriteLine($"[{clientThread.ManagedThreadId.ToString()}] Reader, writer, stream, and socket closed");
        }

        private void SendFileName(string fileName, long fileLength)
        {
            // send file name and file length message
            writer.WriteLine(fileName);
            writer.WriteLine(fileLength.ToString());
            writer.Flush();
            Console.WriteLine($"[{clientThread.ManagedThreadId.ToString()}] Sent file name and length to client: {fileName}, {fileLength.ToString()}");
        }

        private void SendFileContents(string fileContents)
        {
            // send file contents only
            // NOTE: no \n at end of contents
            writer.Write(fileContents);
            writer.Flush();
            Console.WriteLine($"[{clientThread.ManagedThreadId.ToString()}] Sent {fileContents.Length} bytes to client");
        }

        private void SendDone()
        {
            // send done message
            writer.WriteLine("done");
            writer.Flush();
            Console.WriteLine($"[{clientThread.ManagedThreadId.ToString()}] Sent 'done' to client");
        }

        private void SendError(string errorMessage)
        {
            // send error message
            writer.WriteLine("error");
            writer.WriteLine(errorMessage);
            writer.Flush();
            Console.WriteLine($"[{clientThread.ManagedThreadId.ToString()}] Sent 'error' to client");
        }
    }
}
