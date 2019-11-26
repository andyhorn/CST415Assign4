// FTClient.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace FTClient
{
    class FTClient
    {
        private string ftServerAddress;
        private ushort ftServerPort;
        bool connected;
        Socket clientSocket;
        NetworkStream stream;
        StreamReader reader;
        StreamWriter writer;

        public FTClient(string ftServerAddress, ushort ftServerPort)
        {
            // save server address/port
            this.ftServerAddress = ftServerAddress;
            this.ftServerPort = ftServerPort;

            // initialize to not connected to server
            connected = false;

            clientSocket = null;
            stream = null;
            reader = null;
            writer = null;
        }

        public void Connect()
        {
            if (!connected)
            {
                // create a client socket and connect to the FT Server's IP address and port
                clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(new IPEndPoint(IPAddress.Parse(ftServerAddress), ftServerPort));

                // establish the network stream, reader and writer
                stream = new NetworkStream(clientSocket);
                reader = new StreamReader(stream, UTF8Encoding.ASCII);
                writer = new StreamWriter(stream, UTF8Encoding.ASCII);

                // now connected
                connected = true;
            }
        }

        public void Disconnect()
        {
            if (connected)
            {
                // send exit to FT server
                SendExit();

                // close writer, reader and stream
                writer.Close();
                reader.Close();
                stream.Close();

                // disconnect and close socket
                clientSocket.Disconnect(false);
                clientSocket.Close();

                // now disconnected
                connected = false;
            }
        }

        public void GetDirectory(string directoryName)
        {
            // send get to the server for the specified directory and receive files
            if (connected)
            {
                // send get command for the directory
                SendGet(directoryName);

                // receive and process files
                while (ReceiveFile(directoryName));
            }
        }

        #region implementation

        private void SendGet(string directoryName)
        {
            // send get message for the directory
            writer.WriteLine("get");
            writer.WriteLine(directoryName);
            writer.Flush();
            Console.WriteLine($"Sent 'get' to FT server for directory {directoryName}");
        }

        private void SendExit()
        {
            // send exit message
            writer.WriteLine("exit");
            writer.Flush();
            Console.WriteLine("Sent 'exit' to FT server");
        }

        private void SendInvalidMessage()
        {
            // allows for testing of server's error handling code
            writer.WriteLine("invalid");
            writer.Flush();
            Console.WriteLine("Sent an invalid test message to the server");
        }

        private bool ReceiveFile(string directoryName)
        {
            // receive a single file from the server and save it locally in the specified directory

            // expect file name from server
            var message = reader.ReadLine();

            switch (message)
            {
                case null:
                {
                    Console.WriteLine("Received a null string from the server");
                    return false;
                }

                case "done":
                {
                    // when the server sends "done", then there are no more files!
                    Console.WriteLine("Received 'done' from server");
                    return false;
                }

                case "error":
                {
                    // handle error messages from the server
                    var errorMessage = reader.ReadLine();
                    Console.WriteLine($"Received error from server: {errorMessage}");
                    return false;
                }

                default:
                {
                    // received a file name
                    var fileName = message;
                    Console.WriteLine($"Received file name: {fileName}");

                    // receive file length from server
                    var fileLength = int.Parse(reader.ReadLine());
                    Console.WriteLine($"Received file length: {fileLength.ToString()}");

                    // receive file contents

                    // loop until all of the file contents are received
                    var numBytesLeftToRead = fileLength;
                    string contents = "";

                    while (numBytesLeftToRead > 0)
                    {
                        // receive as many characters from the server as available
                        char[] buffer = new char[numBytesLeftToRead];
                        var numBytesReceived = reader.Read(buffer, 0, numBytesLeftToRead);
                        var contentReceived = new string(buffer);

                        // accumulate bytes read into the contents
                        contents += contentReceived;
                        numBytesLeftToRead -= numBytesReceived;
                    }
                    Console.WriteLine($"Received {contents.Length.ToString()} bytes from the server");

                    // create the local directory if needed
                    var directory = new DirectoryInfo(directoryName);

                    if (!directory.Exists)
                    {
                        directory.Create();
                        Console.WriteLine($"Directory {directoryName} created");
                    }

                    // save the file locally on the disk
                    using (var file = File.CreateText(Path.Combine(directoryName, fileName)))
                    {
                        file.Write(contents);
                        file.Flush();
                    }
                    Console.WriteLine($"File written to disk");

                    return true;
                }
            }
        }

        #endregion
    }
}
