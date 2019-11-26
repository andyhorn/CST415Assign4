// FTClientProgram.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 

using System;
using PRSLib;

namespace FTClient
{
    class FTClientProgram
    {
        private static void Usage()
        {
            /*
                -prs <PRS IP address>:<PRS port>
                -s <file transfer server IP address>
                -d <directory requested>
            */
            Console.WriteLine("Usage: FTClient -d <directory> [-prs <PRS IP>:<PRS port>] [-s <FT Server IP>]");
        }

        static void Main(string[] args)
        {
            // defaults
            string PRSSERVER_IPADDRESS = "127.0.0.1";
            ushort PRSSERVER_PORT = 30000;
            string FTSERVICE_NAME = "FT Server";
            string FTSERVER_IPADDRESS = "127.0.0.1";
            ushort FTSERVER_PORT = 40000;
            string DIRECTORY_NAME = null;

            // process the command line arguments
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                switch (arg)
                {
                    case "-d":
                    {
                        DIRECTORY_NAME = args[++i];
                    }
                    break;

                    case "-prs":
                    {
                        var prs = args[++i];
                        PRSSERVER_IPADDRESS = prs.Split(':')[0];
                        PRSSERVER_PORT = ushort.Parse(prs.Split(':')[1]);
                    }
                    break;

                    case "-s":
                    {
                        FTSERVER_IPADDRESS = args[++i];
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
            Console.WriteLine("FT Server Address: " + FTSERVER_IPADDRESS);
            Console.WriteLine("Directory: " + DIRECTORY_NAME);
            
            try
            {
                // contact the PRS and lookup port for "FT Server"
                var prs = new PRSClient(PRSSERVER_IPADDRESS, PRSSERVER_PORT, FTSERVICE_NAME);
                FTSERVER_PORT = prs.LookupPort();
                Console.WriteLine($"Received port {FTSERVER_PORT} from PRS server");

                // create an FTClient and connect it to the server
                var ftClient = new FTClient(FTSERVER_IPADDRESS, FTSERVER_PORT);
                ftClient.Connect();
                Console.WriteLine("FT client connected");

                // get the contents of the specified directory
                ftClient.GetDirectory(DIRECTORY_NAME);
                Console.WriteLine("FT client retrieved directory");

                // disconnect from the server
                ftClient.Disconnect();
                Console.WriteLine("FT client disconnected");
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
