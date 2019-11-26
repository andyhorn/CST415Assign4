// SDServerProgram.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 

using System;
using PRSLib;

namespace SDServer
{
    class SDServerProgram
    {
        private static void Usage()
        {
            Console.WriteLine("Usage: SDServer -prs <PRS IP address>:<PRS port>");
        }

        static void Main(string[] args)
        {
            // defaults
            ushort SDSERVER_PORT = 40000;
            int CLIENT_BACKLOG = 5;
            string PRS_ADDRESS = "127.0.0.1";
            ushort PRS_PORT = 30000;
            string SERVICE_NAME = "SD Server";

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                switch (arg)
                {
                    case "-prs":
                    {
                        var parameters = args[++i].Split(':');
                        var ipAddress = parameters[0];
                        var port = parameters[1];

                        PRS_ADDRESS = ipAddress;
                        PRS_PORT = ushort.Parse(port);
                    }
                    break;
                    default:
                    {
                        Console.WriteLine($"Invalid argument: {arg}");
                        Usage();
                    }
                    break;
                }
            }

            Console.WriteLine("PRS Address: " + PRS_ADDRESS);
            Console.WriteLine("PRS Port: " + PRS_PORT);

            try
            {
                // contact the PRS, request a port for "FT Server" and start keeping it alive
                var prs = new PRSClient(PRS_ADDRESS, PRS_PORT, SERVICE_NAME);
                SDSERVER_PORT = prs.RequestPort();
                prs.KeepPortAlive();

                // instantiate SD server and start it running
                var server = new SDServer(SDSERVER_PORT, CLIENT_BACKLOG);
                server.Start();

                // tell the PRS that it can have it's port back, we don't need it anymore
                prs.ClosePort();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            // wait for a keypress from the user before closing the console window
            Console.WriteLine("Press Enter to exit");
            Console.ReadKey();
        }
    }
}
