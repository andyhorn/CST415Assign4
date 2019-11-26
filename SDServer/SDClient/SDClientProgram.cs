using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using PRSLib;

namespace SDClient
{
    class SDClientProgram
    {
        private static void Usage()
        {
            /*
                -prs <PRS IP address>:<PRS port>
                -s <SD server IP address>
		        -o | -r <session id> | -c <session id>
                [-get <document> | -post <document>]
            */
            Console.WriteLine("Usage: SDClient [-prs <PRS IP>:<PRS port>] [-s <SD Server IP>]");
            Console.WriteLine("\t-o | -r <session id> | -c <session id>");
            Console.WriteLine("\t[-get <document> | -post <document>]");
        }

        static void Main(string[] args)
        {
            // defaults
            string PRSSERVER_IPADDRESS = "127.0.0.1";
            ushort PRSSERVER_PORT = 30000;
            string SDSERVICE_NAME = "SD Server";
            string SDSERVER_IPADDRESS = "127.0.0.1";
            ushort SDSERVER_PORT = 40000;
            string SESSION_CMD = null;
            ulong SESSION_ID = 0;
            string DOCUMENT_CMD = null;
            string DOCUMENT_NAME = null;

            // process the command line arguments
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                switch(arg)
                {
                    case "-r":
                    case "-c":
                    {
                        SESSION_CMD = arg;
                        SESSION_ID = ulong.Parse(args[++i]);
                    }
                    break;
                    case "-o":
                    {
                        SESSION_CMD = arg;
                    }
                    break;
                    case "-get":
                    case "-post":
                    {
                        DOCUMENT_CMD = arg;
                        DOCUMENT_NAME = args[++i];
                    }
                    break;
                    default:
                    {
                        Console.WriteLine($"Invalid argument: {arg}");
                        Usage();
                        return;
                    }
                }
            }

            Console.WriteLine("PRS Address: " + PRSSERVER_IPADDRESS);
            Console.WriteLine("PRS Port: " + PRSSERVER_PORT);
            Console.WriteLine("SD Server Address: " + SDSERVER_IPADDRESS);
            Console.WriteLine("Session Command: " + SESSION_CMD);
            Console.WriteLine("Session Id: " + SESSION_ID);
            Console.WriteLine("Document Command: " + DOCUMENT_CMD);
            Console.WriteLine("Document Name: " + DOCUMENT_NAME);

            try
            {
                // contact the PRS and lookup port for "SD Server"
                var prs = new PRSClient(PRSSERVER_IPADDRESS, PRSSERVER_PORT, SDSERVICE_NAME);

                // create an SDClient to use in talking to the server
                var client = new SDClient(SDSERVER_IPADDRESS, SDSERVER_PORT);
                client.Connect();
                
                // send session command to server
                if (SESSION_CMD == "-o")
                {
                    // open new session
                    client.OpenSession();
                    Console.WriteLine($"Opened session {client.SessionID}");
                }
                else if (SESSION_CMD == "-r")
                {
                    // resume existing session
                    Console.WriteLine($"Resuming session id {SESSION_ID}");
                    client.SessionID = SESSION_ID;
                    client.ResumeSession();
                    Console.WriteLine($"Accepted session id {SESSION_ID}");
                }
                else if (SESSION_CMD == "-c")
                {
                    Console.WriteLine($"Closing session id {SESSION_ID}");
                    // close existing session
                    client.SessionID = SESSION_ID;
                    client.CloseSession();
                    Console.WriteLine($"Closed session id {SESSION_ID}");
                }
                
                // send document request to server
                if (DOCUMENT_CMD == "-post")
                {
                    // read the document contents from stdin
                    var contents = Console.In.ReadToEnd();

                    // send the document to the server
                    Console.WriteLine($"Posting {contents.Length} bytes of {DOCUMENT_NAME}");
                    client.PostDocument(DOCUMENT_NAME, contents);

                    Console.WriteLine("Success");
                }
                else if (DOCUMENT_CMD == "-get")
                {
                    Console.WriteLine($"Getting {DOCUMENT_NAME}");
                    // get document from the server
                    var contents = client.GetDocument(DOCUMENT_NAME);

                    // print out the received document
                    Console.WriteLine($"Success, received {contents.Length} bytes of {DOCUMENT_NAME}");
                    Console.WriteLine(contents);
                }

                // disconnect from the server
                client.Disconnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            // wait for a keypress from the user before closing the console window
            // NOTE: the following commented out as they cannot be used when redirecting input to post a file
            //Console.WriteLine("Press Enter to exit");
            //Console.ReadKey();
        }
    }
}
