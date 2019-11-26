// SDProtocolClient.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 

using System;
using System.Collections.Generic;
using System.Text;
using PRSLib;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace SDBrowser
{
    // implements IProtocolClient
    // uses the SD protcol
    // keeps track of sessions for each SD Server
    //  appropriately opens or resumes a session
    //  closes the session when the protocol client is closed
    // retrieves a single requested "document" by name
    // TODO: consider how this class could be implemented in terms of the SDClient class in SDClient project

    class SDProtocolClient : IProtocolClient
    {
        // represents an open session for a single SD Server
        private class SDSession
        {
            public string ipAddr;
            public ushort port;
            public ulong sessionId;

            public SDSession(string ipAddr, ushort port, ulong sessionId)
            {
                this.ipAddr = ipAddr;
                this.port = port;
                this.sessionId = sessionId;
            }
        }

        private string prsIP;
        private ushort prsPort;
        private Dictionary<string, SDSession> sessions;     // server IP address --> session  info on the SD server


        public SDProtocolClient(string prsIP, ushort prsPort)
        {
            // TODO: SDProtocolClient.SDProtocolClient()

            // save the PRS server's IP address and port
            // will be used later to lookup the port for the SD Server when needed
            

            // initially empty dictionary of sessions
            
        }

        public string GetDocument(string serverIP, string documentName)
        {
            // TODO: SDProtocolClient.GetDocument()

            // retrieve requested document from the specified server
            // manage the session with the SD Server
            //  opening or resuming as needed
            // connect to and disconnect from the server w/in this method

            // make sure we have valid parameters
            // serverIP is the SD Server's IP address
            // documentName is the name of a docoument on the SD Server
            // both should not be empty
            

            // contact the PRS and lookup port for "SD Server"
            

            // connect to SD server by ipAddr and port
            // use OpenOrResumeSession() to ensure session is handled correctly
            

            // create network stream, reader and writer
            

            // send get message to server for requested document
            

            // get the server's response
            

            // close writer, reader and network stream
            

            // disconnect from server and close the socket
            

            // return the content
            return "TODO";
        }

        public void Close()
        {
            // TODO: SDProtocolClient.Close()

            // close each open session with the various servers

            // for each session...
            // connect to the SD Server's IP address and port
            // create network stream, reader and writer
            // send the close for this sessionId
            // close writer, reader and network stream
            // disconnect from server and close the socket

        }

        private Socket OpenOrResumeSession(string ipAddr, ushort port)
        {
            // TODO: SDProtocolClient.OpenOrResumeSession()

            // create and connect a socket to the given SD Server
            // open or resume a session
            // leave the socket open and return it for communication to the server

            // connect to the SD Server's IP address and port
            
            // create network stream, reader and writer
            
            // do we already have a session for this server?
            // yes, session already open
            // retrieve the sessionId and send resume message to server
            // receive response and verify sessionId received
                
            // no, session not open for this server
            // open a new session and save the sessionId
            // receive response and verify sessionId received
            // save this open session in the sessions dictionary for later
            
            // keep the socket open and return it
            return null;
        }

        private static void SendOpen(StreamWriter writer)
        {
            // TODO: SDProtocolClient.SendOpen()

            // send open message to SD Server

        }

        private static void SendResume(StreamWriter writer, ulong sessionId)
        {
            // TODO: SDProtocolClient.SendResume()

            // send resume message to SD Server

        }

        private static ulong ReceiveSessionResponse(StreamReader reader)
        {
            // TODO: SDProtocolClient.ReceiveSessionResponse()

            // get server's response to our session request

            // if accepted...
            // yay, server accepted our session!
            // get and return the sessionID


            // if rejected
            // boo, server rejected us!


            // if error
            // boo, server sent us an error!

            // handle invalid response to our session request

            return 0;
        }

        private static void SendClose(StreamWriter writer, ulong sessionId)
        {
            // TODO: SDProtocolClient.SendClose()

            // send close message to SD Server

        }

        private static void SendGet(StreamWriter writer, string documentName)
        {
            // TODO: SDProtocolClient.SendGet()

            // send get message to SD Server

        }

        private static string ReceiveGetResponse(StreamReader reader)
        {
            // TODO: SDProtocolClient.ReceiveGetResponse()

            // get server's response to our get request and return the content received

            // if success...
            // yay, server accepted our request!
            // read the document name, content length and content
            // return the content

            // if error
            // boo, server sent us an error!

            // handle invalid response to our session request

            return "TODO";
        }

        private static string ReceiveDocument(StreamReader reader, int length)
        {
            // TODO: SDProtocolClient.ReceiveDocument()

            // read from the reader until we've received the expected number of characters
            // accumulate the characters into a string and return those when we got enough
            

            return "TODO";
        }
    }
}
