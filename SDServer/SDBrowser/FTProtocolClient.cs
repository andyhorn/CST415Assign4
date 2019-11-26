// FTProtocolClient.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 

using System;
using System.Text;
using PRSLib;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace SDBrowser
{
    // implements IProtocolClient
    // uses the FT protcol
    // retrieves an entire directory and represents it as a single text "document"
    // TODO: consider how this class could be implemented in terms of the FTClient class from Assign 2

    class FTProtocolClient : IProtocolClient
    {
        private string prsIP;
        private ushort prsPort;

        public FTProtocolClient(string prsIP, ushort prsPort)
        {
            // TODO: FTProtocolClient.FTProtocolClient()
            // save the PRS server's IP address and port
            // will be used later to lookup the port for the FT Server when needed
            
        }

        public string GetDocument(string serverIP, string documentName)
        {
            // TODO: FTProtocolClient.GetDocument()
            // make sure we have valid parameters
            // serverIP is the FT Server's IP address
            // documentName is the name of a directory on the FT Server
            // both should not be empty
            

            // contact the PRS and lookup port for "FT Server"
            

            // connect to FT server by ipAddr and port
            

            // create network stream, reader and writer
            

            // send get message to server for requested directory
            

            // receive files from server, and accumulate in result string
            
            
            // send exit
            

            // close writer, reader and network stream
            

            // disconnect from server and close the socket
            

            // return the content
            return "TODO";
        }

        public void Close()
        {
            // TODO: FTProtocolClient.Close()
            // nothing to do here!
            // the FT Protocol does not expect a client to close a session
            // everything is handled in the GetDocument() method
        }

        private static void SendGet(StreamWriter writer, string directoryName)
        {
            // TODO: FTProtocolClient.SendGet()
            // send the get message to the FT server
            
        }

        private static bool ReceiveFile(StreamReader clientReader, string directoryName, StringBuilder result)
        {
            // TODO: FTProtocolClient.ReceiveFile()
            // retrieve a single file from the FT Server
            // for reach file received...
            //  result += document name + \n
            //  result += document content

            // expect file name


            // when the server sends "done", then there are no more files!


            // handle error messages from the server


            // received a file name and add it to the result


            // retrieve file length


            // retrieve file contents


            // add the contents to the result string


            return true;
        }

        private static void SendExit(StreamWriter writer)
        {
            // TODO: FTProtocolClient.SendExit()
            // send exit message to FT server

        }
    }
}
