// PRSServerProgram.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 
// 
// Extended by Andy Horn
// October  2019
// CST 415 - Assignment 1
// 

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using PRSLib;

namespace PRSServer
{
    class PRSServerProgram
    {
        class PRS
        {
            // represents a PRS Server, keeps all state and processes messages accordingly

            class PortReservation
            {
                private ushort port;
                private bool available;
                private string serviceName;
                private DateTime lastAlive;

                public PortReservation(ushort port)
                {
                    this.port = port;
                    available = true;
                }

                public string ServiceName { get { return serviceName; } }
                public ushort Port { get { return port; } }
                public bool Available { get { return available; } }

                public bool Expired(int timeout)
                {
                    // return true if timeout seconds have elapsed since lastAlive
                    return (DateTime.Now - lastAlive).TotalSeconds >= timeout;
                }

                public void Reserve(string serviceName)
                {
                    // reserve this port for serviceName
                    available = false;
                    this.serviceName = serviceName;
                    lastAlive = DateTime.Now;
                }

                public void KeepAlive()
                {
                    // save current time in lastAlive
                    lastAlive = DateTime.Now;
                }

                public void Close()
                {
                    // make this reservation available
                    available = true;
                    serviceName = null;
                }
            }

            // server attribues
            private ushort startingClientPort;
            private ushort endingClientPort;
            private int keepAliveTimeout;
            private int numPorts;
            private PortReservation[] ports;
            private bool stopped;

            public PRS(ushort startingClientPort, ushort endingClientPort, int keepAliveTimeout)
            {

                // save parameters
                this.startingClientPort = startingClientPort;
                this.endingClientPort = endingClientPort;
                this.keepAliveTimeout = keepAliveTimeout;
                this.numPorts = endingClientPort - startingClientPort + 1;
                // initialize to not stopped
                stopped = false;

                // initialize port reservations
                // get the total amount of numbers between start and end (inclusively)
                ports = new PortReservation[numPorts];
                for (ushort i = 0; i < numPorts; i++)
                {
                    ports[i] = new PortReservation((ushort)(startingClientPort + i));
                }
            }

            public bool Stopped { get { return stopped; } }

            private void CheckForExpiredPorts()
            {
                foreach (PortReservation port in ports.Where(p => p.Expired(keepAliveTimeout) && !p.Available))
                {
                    port.Close();
                }
            }

            private PRSMessage RequestPort(string serviceName)
            {
                PRSMessage response = null;

                // client has requested the lowest available port, so find it!

                PortReservation reservation = ports.FirstOrDefault(port => port.Available);

                if (reservation != null)
                {
                    // if found an available port, reserve it and send SUCCESS
                    reservation.Reserve(serviceName);
                    response = new PRSMessage(PRSMessage.MESSAGE_TYPE.RESPONSE, serviceName, reservation.Port, PRSMessage.STATUS.SUCCESS);
                }
                else
                {
                    // else, none available, send ALL_PORTS_BUSY
                    response = new PRSMessage(PRSMessage.MESSAGE_TYPE.RESPONSE, serviceName, 0, PRSMessage.STATUS.ALL_PORTS_BUSY);
                }

                return response;
            }

            public PRSMessage HandleMessage(PRSMessage msg)
            {
                // handle one message and return a response

                PRSMessage response = null;

                switch (msg.MsgType)
                {
                    case PRSMessage.MESSAGE_TYPE.REQUEST_PORT:
                    {
                        // check for expired ports and send requested report
                        CheckForExpiredPorts();
                        response = RequestPort(msg.ServiceName);
                    }
                    break;

                    case PRSMessage.MESSAGE_TYPE.KEEP_ALIVE:
                    {
                        // client has requested that we keep their port alive
                        // find the port
                        PortReservation reservation = ports.FirstOrDefault(
                            port => (port.ServiceName == msg.ServiceName)
                            && (port.Port == msg.Port));

                        if (reservation != null)
                        {
                            // if found, keep it alive and send SUCCESS
                            reservation.KeepAlive();
                            response = new PRSMessage(
                                PRSMessage.MESSAGE_TYPE.RESPONSE
                                , reservation.ServiceName
                                , reservation.Port
                                , PRSMessage.STATUS.SUCCESS);
                        }
                        else
                        {
                            // else, SERVICE_NOT_FOUND
                            response = new PRSMessage(
                                PRSMessage.MESSAGE_TYPE.RESPONSE
                                , msg.ServiceName
                                , msg.Port
                                , PRSMessage.STATUS.SERVICE_NOT_FOUND);
                        }
                    }
                    break;

                    case PRSMessage.MESSAGE_TYPE.CLOSE_PORT:
                    {
                        // client has requested that we close their port, and make it available for others!
                        // find the port
                        PortReservation reservation = ports.FirstOrDefault(
                            port => (port.ServiceName == msg.ServiceName)
                            && (port.Port == msg.Port));

                        if (reservation != null)
                        {
                            // if found, close it and send SUCCESS
                            reservation.Close();
                            response = new PRSMessage(
                                PRSMessage.MESSAGE_TYPE.RESPONSE
                                , msg.ServiceName
                                , msg.Port
                                , PRSMessage.STATUS.SUCCESS);
                        }
                        else
                        {
                            // else, SERVICE_NOT_FOUND
                            response = new PRSMessage(
                                PRSMessage.MESSAGE_TYPE.RESPONSE
                                , msg.ServiceName
                                , msg.Port
                                , PRSMessage.STATUS.SERVICE_NOT_FOUND);
                        }
                    }
                    break;

                    case PRSMessage.MESSAGE_TYPE.LOOKUP_PORT:
                    {
                        // client wants to know the reserved port number for a named service
                        // find the port
                        PortReservation reservation = ports.FirstOrDefault(
                            port => (port.ServiceName == msg.ServiceName));

                        if (reservation != null)
                        {
                            // if found, send port number back
                            response = new PRSMessage(
                                PRSMessage.MESSAGE_TYPE.RESPONSE
                                , reservation.ServiceName
                                , reservation.Port
                                , PRSMessage.STATUS.SUCCESS);
                        }
                        else
                        {
                            // else, SERVICE_NOT_FOUND
                            response = new PRSMessage(
                                PRSMessage.MESSAGE_TYPE.RESPONSE
                                , msg.ServiceName
                                , msg.Port
                                , PRSMessage.STATUS.SERVICE_NOT_FOUND);
                        }
                    }
                    break;

                    case PRSMessage.MESSAGE_TYPE.STOP:
                    {
                        // client is telling us to close the appliation down
                        // stop the PRS and return SUCCESS
                        stopped = true;
                        response = new PRSMessage(PRSMessage.MESSAGE_TYPE.RESPONSE, "", 0, PRSMessage.STATUS.SUCCESS);
                    }
                    break;
                }

                return response;
            }

        }

        static void Usage()
        {
            Console.WriteLine("usage: PRSServer [options]");
            Console.WriteLine("\t-p < service port >");
            Console.WriteLine("\t-s < starting client port number >");
            Console.WriteLine("\t-e < ending client port number >");
            Console.WriteLine("\t-t < keep alive time in seconds >");
        }

        static void Main(string[] args)
        {

            // defaults
            ushort SERVER_PORT = 30000;
            ushort STARTING_CLIENT_PORT = 40000;
            ushort ENDING_CLIENT_PORT = 40099;
            int KEEP_ALIVE_TIMEOUT = 300;

            // process command options
            // -p < service port >
            // -s < starting client port number >
            // -e < ending client port number >
            // -t < keep alive time in seconds >

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-p":
                    {
                        SERVER_PORT = ushort.Parse(args[++i]);
                    }
                    break;
                    case "-s":
                    {
                        STARTING_CLIENT_PORT = ushort.Parse(args[++i]);
                    }
                    break;
                    case "-e":
                    {
                        ENDING_CLIENT_PORT = ushort.Parse(args[++i]);
                    }
                    break;
                    case "-t":
                    {
                        KEEP_ALIVE_TIMEOUT = int.Parse(args[++i]);
                    }
                    break;
                    default:
                    {
                        Console.WriteLine($"Error: Invalid argument - {args[i]}");
                        return;
                    }
                }
            }

            // check for valid STARTING_CLIENT_PORT and ENDING_CLIENT_PORT
            if (STARTING_CLIENT_PORT >= ENDING_CLIENT_PORT
                || STARTING_CLIENT_PORT == 0
                || ENDING_CLIENT_PORT == 0
                || STARTING_CLIENT_PORT == SERVER_PORT
                || ENDING_CLIENT_PORT == SERVER_PORT)
            {
                Console.WriteLine("Error: Invalid starting and/or ending port(s)");
                return;
            }

            Console.WriteLine("Server starting...");
            Console.WriteLine($"\tServer port: {SERVER_PORT}");
            Console.WriteLine($"\tStarting port: {STARTING_CLIENT_PORT}");
            Console.WriteLine($"\tEnding port: {ENDING_CLIENT_PORT}");
            Console.WriteLine($"\tTimeout: {KEEP_ALIVE_TIMEOUT}");

            // initialize the PRS server
            PRS prs = new PRS(STARTING_CLIENT_PORT, ENDING_CLIENT_PORT, KEEP_ALIVE_TIMEOUT);

            // create the socket for receiving messages at the server
            Socket socket = new Socket(SocketType.Dgram, ProtocolType.Udp);

            // bind the listening socket to the PRS server port
            socket.Bind(new IPEndPoint(IPAddress.Any, SERVER_PORT));

            //
            // Process client messages
            //

            while (!prs.Stopped)
            {
                try
                {
                    // receive a message from a client
                    EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    PRSMessage messageReceived = PRSMessage.ReceiveMessage(socket, ref remoteEndPoint);

                    // let the PRS handle the message
                    PRSMessage messageResponse = prs.HandleMessage(messageReceived);

                    // send response message back to client
                    messageResponse.SendMessage(socket, remoteEndPoint);

                }
                catch (Exception ex)
                {
                    // attempt to send a UNDEFINED_ERROR response to the client, if we know who that was
                    Console.WriteLine(ex.Message);
                }
            }

            // close the listening socket
            socket.Close();

            // wait for a keypress from the user before closing the console window
            Console.WriteLine("Press Enter to exit");
            Console.ReadKey();
        }
    }
}
