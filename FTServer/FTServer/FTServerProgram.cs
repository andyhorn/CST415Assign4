// FTServerProgram.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 

using System;
using PRSLib;


namespace FTServer
{
    class FTServerProgram
    {
        private static void Usage()
        {
            Console.WriteLine("Usage: FTServer -prs <PRS IP address>:<PRS port>");
        }

        static void Main(string[] args)
        {
            // defaults
            ushort FTSERVER_PORT = 40000;
            int CLIENT_BACKLOG = 5;
            string PRS_ADDRESS = "127.0.0.1";
            ushort PRS_PORT = 30000;
            string SERVICE_NAME = "FT Server";

            // process the command line arguments to get the PRS ip address and PRS port number
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                switch (arg)
                {
                    case "-prs":
                    {
                        var prs = args[++i].Split(':');
                        PRS_ADDRESS = prs[0];
                        PRS_PORT = ushort.Parse(prs[1]);
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
            Console.WriteLine("PRS Address: " + PRS_ADDRESS);
            Console.WriteLine("PRS Port: " + PRS_PORT);
            
            try
            {
                // contact the PRS, request a port for "FT Server" and start keeping it alive
                var prsClient = new PRSClient(PRS_ADDRESS, PRS_PORT, SERVICE_NAME);
                FTSERVER_PORT = prsClient.RequestPort();
                prsClient.KeepPortAlive();
                Console.WriteLine($"Received port {FTSERVER_PORT} from PRS Server");

                // instantiate FT server and start it running
                Console.WriteLine("Starting FT server");
                var ftServer = new FTServer(FTSERVER_PORT, CLIENT_BACKLOG);
                ftServer.Start();
                Console.WriteLine($"FT server stopped");

                // tell the PRS that it can have it's port back, we don't need it anymore
                prsClient.ClosePort();
                Console.WriteLine("FT server port closed");
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
