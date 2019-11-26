// FTServer.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 

using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace FTServer
{
    class FTServer
    {
        // represents the server and it's logic
        // the server uses the main program thread to listen and accept connections from client
        // when the server accepts a client connection, it will create the client's socket and thread

        private ushort listeningPort;
        private int clientBacklog;

        public FTServer(ushort listeningPort, int clientBacklog)
        {
            this.listeningPort = listeningPort;
            this.clientBacklog = clientBacklog;
        }

        public void Start()
        {
            // create a listening socket for clients to connect
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            // bind to the FT Server port
            Console.WriteLine($"Binding FT server to port {listeningPort}");
            socket.Bind(new IPEndPoint(IPAddress.Any, listeningPort));

            // set the socket to listen
            socket.Listen(clientBacklog);
            Console.WriteLine("FT server listening...");

            bool done = false;
            while (!done)
            {
                try
                {
                    // accept a client connection
                    Console.WriteLine("Waiting for client connection...");
                    var connectedClientSocket = socket.Accept();
                    Console.WriteLine("Client connection accepted");

                    // instantiate connected client to process messages
                    var connectedClient = new FTConnectedClient(connectedClientSocket);
                    Console.WriteLine("Starting FT client");
                    connectedClient.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while accepting and starting client: " + ex.Message);
                    Console.WriteLine("Waiting for 5 seconds and trying again...");
                    Thread.Sleep(5000);
                }
            }

            // close socket and quit
            socket.Close();
            Console.WriteLine("FT server socket closed");
        }
    }
}
