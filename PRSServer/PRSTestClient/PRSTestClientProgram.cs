// PRSTestClientProgram.cs
//
// Pete Myers
// CST 415
// Fall 2019
//
// Connects to a PRSServer and runs TC1 through TC6
// Assumes the PRSServer is run with the following command line arguments:
//     PRSServer.exe -p 30000 -s 40000 -e 40100 -t 10
// 
// 
// Extended by Andy Horn
// October 2019
// CST 415 - Assignment 1
//

using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using PRSLib;

namespace PRSTestClient
{
    class PRSTestClientProgram
    {
        static void Usage()
        {
            Console.WriteLine("usage: PRSTestClient [options]");
            Console.WriteLine("\t-prs <serverIP>:<serverPort>");
        }

        static void Main(string[] args)
        {
            // defaults
            string SERVER_IP = "127.0.0.1";
            int SERVER_PORT = 30000;

            // process command options
            

            // tell user what we're doing
            Console.WriteLine("Test Client started...");
            Console.WriteLine("  ServerIP = " + SERVER_IP.ToString());
            Console.WriteLine("  ServerPort = " + SERVER_PORT.ToString());

            // recommend proper PRSServer command line arguments
            Console.WriteLine();
            Console.WriteLine("Assumes the PRSServer is running with the following command line arguments:");
            Console.WriteLine("    PRSServer.exe -p 30000 -s 40000 -e 40100 -t 10");
            Console.WriteLine();

            // create the socket for sending messages to the server
            Socket socket = new Socket(SocketType.Dgram, ProtocolType.Udp);

            // construct the server's address and port
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(SERVER_IP), SERVER_PORT);
            
            //
            // Implement test cases
            //

            try
            {
                // call each test case method
                TestCase1(socket, endPoint);
                TestCase2(socket, endPoint);
                TestCase3(socket, endPoint);
                TestCase4(socket, endPoint);
                TestCase5(socket, endPoint);
                TestCase6(socket, endPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            // close the client socket and quit
            socket.Close();
            
            // wait for a keypress from the user before closing the console window
            Console.WriteLine("Press Enter to exit");
            Console.ReadKey();
        }

        private static void SendMessage(Socket clientSocket, IPEndPoint endPt, PRSMessage msg)
        {
            msg.SendMessage(clientSocket, endPt);
        }

        private static PRSMessage ExpectMessage(Socket clientSocket, string expectedMessage)
        {
            // receive message and validate that expected PRSMessage was received
            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            PRSMessage msg = PRSMessage.ReceiveMessage(clientSocket, ref remoteEP);
            if (msg.ToString() != expectedMessage)
                throw new Exception("Test failed! Expected " + expectedMessage);

            return msg;
        }

        private static void TestCase1(Socket clientSocket, IPEndPoint endPt)
        {
            // Simulates a PRS client, SVC1, that requests a port, keeps it alive and then closes it.

            Console.WriteLine("TestCase 1 Started...");

            // See test cases doc
            SendMessage(clientSocket, endPt, new PRSMessage(PRSMessage.MESSAGE_TYPE.REQUEST_PORT, "SVC1", 0, 0));
            ExpectMessage(clientSocket, "{RESPONSE, SVC1, 40000, SUCCESS}");

            SendMessage(clientSocket, endPt, new PRSMessage(PRSMessage.MESSAGE_TYPE.KEEP_ALIVE, "SVC1", 40000, 0));
            ExpectMessage(clientSocket, "{RESPONSE, SVC1, 40000, SUCCESS}");

            SendMessage(clientSocket, endPt, new PRSMessage(PRSMessage.MESSAGE_TYPE.CLOSE_PORT, "SVC1", 40000, 0));
            ExpectMessage(clientSocket, "{RESPONSE, SVC1, 40000, SUCCESS}");

            Console.WriteLine("TestCase 1 Passed!");
            Console.WriteLine();
        }

        private static void TestCase2(Socket clientSocket, IPEndPoint endPt)
        {
            // See test cases doc
            Console.WriteLine("TestCase 2 Started...");

            // Simulates two PRS clients, SVC1 and C1, where SVC1 requests a port, and C1 looks up the port.
            SendMessage(clientSocket, endPt, new PRSMessage(PRSMessage.MESSAGE_TYPE.REQUEST_PORT, "SVC1", 0, 0));
            ExpectMessage(clientSocket, "{RESPONSE, SVC1, 40000, SUCCESS}");

            SendMessage(clientSocket, endPt, new PRSMessage(PRSMessage.MESSAGE_TYPE.LOOKUP_PORT, "SVC1", 0, 0));
            ExpectMessage(clientSocket, "{RESPONSE, SVC1, 40000, SUCCESS}");

            SendMessage(clientSocket, endPt, new PRSMessage(PRSMessage.MESSAGE_TYPE.CLOSE_PORT, "SVC1", 40000, 0));
            ExpectMessage(clientSocket, "{RESPONSE, SVC1, 40000, SUCCESS}");

            Console.WriteLine("TestCase 2 Passed!");
            Console.WriteLine();
        }

        private static void TestCase3(Socket clientSocket, IPEndPoint endPt)
        {
            Console.WriteLine("TestCase 3 Started...");

            // Simulates two PRS clients, SVC1 and SVC2, where SVC1 requests a port, then SVC2 requests a port and receives its own port.
            SendMessage(clientSocket, endPt, new PRSMessage(PRSMessage.MESSAGE_TYPE.REQUEST_PORT, "SVC1", 0, 0));
            ExpectMessage(clientSocket, "{RESPONSE, SVC1, 40000, SUCCESS}");

            SendMessage(clientSocket, endPt, new PRSMessage(PRSMessage.MESSAGE_TYPE.REQUEST_PORT, "SVC2", 0, 0));
            ExpectMessage(clientSocket, "{RESPONSE, SVC2, 40001, SUCCESS}");

            SendMessage(clientSocket, endPt, new PRSMessage(PRSMessage.MESSAGE_TYPE.CLOSE_PORT, "SVC1", 40000, 0));
            ExpectMessage(clientSocket, "{RESPONSE, SVC1, 40000, SUCCESS}");

            SendMessage(clientSocket, endPt, new PRSMessage(PRSMessage.MESSAGE_TYPE.CLOSE_PORT, "SVC2", 40001, 0));
            ExpectMessage(clientSocket, "{RESPONSE, SVC2, 40001, SUCCESS}");

            // See test cases doc

            Console.WriteLine("TestCase 3 Passed!");
            Console.WriteLine();
        }

        private static void TestCase4(Socket clientSocket, IPEndPoint endPt)
        {
            // Simulates two PRS clients, SVC1 and SVC2, where SVC1 requests a port, SVC1 fails to keep the port alive, then SVC2 requests a port and receives SVC1’s expired port.

            Console.WriteLine("TestCase 4 Started...");

            // See test cases doc
            // use Thread.Sleep();
            SendMessage(clientSocket, endPt, new PRSMessage(PRSMessage.MESSAGE_TYPE.REQUEST_PORT, "SVC1", 0, 0));
            ExpectMessage(clientSocket, "{RESPONSE, SVC1, 40000, SUCCESS}");

            // Let SCV1 - 40000 expire
            Console.WriteLine("Sleeping (and expiring)...");
            Thread.Sleep(15 * 1000);

            // Request a port again, should get 40000 as it has expired
            SendMessage(clientSocket, endPt, new PRSMessage(PRSMessage.MESSAGE_TYPE.REQUEST_PORT, "SVC2", 0, 0));
            ExpectMessage(clientSocket, "{RESPONSE, SVC2, 40000, SUCCESS}");

            SendMessage(clientSocket, endPt, new PRSMessage(PRSMessage.MESSAGE_TYPE.CLOSE_PORT, "SVC2", 40000, 0));
            ExpectMessage(clientSocket, "{RESPONSE, SVC2, 40000, SUCCESS}");

            Console.WriteLine("TestCase 4 Passed!");
            Console.WriteLine();
        }

        private static void TestCase5(Socket clientSocket, IPEndPoint endPt)
        {
            // Simulates two PRS clients, SVC1 and SVC2, where SVC1 requests a port, SVC1 keeps the port alive, then SVC2 requests a port and receives its own port.

            Console.WriteLine("TestCase 5 Started...");

            // See test cases doc
            // use Thread.Sleep();
            SendMessage(clientSocket, endPt, new PRSMessage(PRSMessage.MESSAGE_TYPE.REQUEST_PORT, "SVC1", 0, 0));
            ExpectMessage(clientSocket, "{RESPONSE, SVC1, 40000, SUCCESS}");

            Console.WriteLine("Sleeping...");
            // Sleep, then send a KeepAlive
            Thread.Sleep(8 * 1000);

            SendMessage(clientSocket, endPt, new PRSMessage(PRSMessage.MESSAGE_TYPE.KEEP_ALIVE, "SVC1", 40000, 0));
            ExpectMessage(clientSocket, "{RESPONSE, SVC1, 40000, SUCCESS}");

            // Sleep again for another 8 seconds
            // If the KeepAlive doesn't work, this will cause the reservation to timeout
            Console.WriteLine("Sleeping...");
            Thread.Sleep(8 * 1000);

            // Request a new port, should get 40001
            SendMessage(clientSocket, endPt, new PRSMessage(PRSMessage.MESSAGE_TYPE.REQUEST_PORT, "SVC2", 0, 0));
            ExpectMessage(clientSocket, "{RESPONSE, SVC2, 40001, SUCCESS}");

            SendMessage(clientSocket, endPt, new PRSMessage(PRSMessage.MESSAGE_TYPE.CLOSE_PORT, "SVC1", 40000, 0));
            ExpectMessage(clientSocket, "{RESPONSE, SVC1, 40000, SUCCESS}");

            SendMessage(clientSocket, endPt, new PRSMessage(PRSMessage.MESSAGE_TYPE.CLOSE_PORT, "SVC2", 40001, 0));
            ExpectMessage(clientSocket, "{RESPONSE, SVC2, 40001, SUCCESS}");

            Console.WriteLine("TestCase 5 Passed!");
            Console.WriteLine();
        }

        private static void TestCase6(Socket clientSocket, IPEndPoint endPt)
        {
            // Simulates a PRS client, M, that tells the PRS to stop

            Console.WriteLine("TestCase 6 Started...");

            // See test cases doc
            SendMessage(clientSocket, endPt, new PRSMessage(PRSMessage.MESSAGE_TYPE.STOP, "", 0, 0));
            ExpectMessage(clientSocket, "{RESPONSE, , 0, SUCCESS}");

            Console.WriteLine("TestCase 6 Passed!");
            Console.WriteLine();
        }
    }

}
