// SDClient.cs
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

namespace SDClient
{
    class SDClient
    {
        private string sdServerAddress;
        private ushort sdServerPort;
        private bool connected;
        private ulong sessionID;
        Socket clientSocket;
        NetworkStream stream;
        StreamReader reader;
        StreamWriter writer;

        public SDClient(string sdServerAddress, ushort sdServerPort)
        {
            // TODO: SDClient.SDClient()

            // save server address/port
            
            // initialize to not connected to server
            
            // no session open at this time
            
        }

        public ulong SessionID { get { return sessionID; } set { sessionID = value; } }

        public void Connect()
        {
            // TODO: SDClient.Connect()

            ValidateDisconnected();

            // create a client socket and connect to the FT Server's IP address and port
            
            // establish the network stream, reader and writer
            
            // now connected
            
        }

        public void Disconnect()
        {
            // TODO: SDClient.Disconnect()

            ValidateConnected();

            // close writer, reader and stream
            
            // disconnect and close socket
            
            // now disconnected
            
        }

        public void OpenSession()
        {
            // TODO: SDClient.OpenSession()

            ValidateConnected();

            // send open command to server
            
            // receive server's response, hopefully with a new session id
            
        }

        public void ResumeSession(ulong trySessionID)
        {
            // TODO: SDClient.ResumeSession()

            ValidateConnected();

            // send resume session to the server
            
            // receive server's response, hopefully confirming our sessionId
            
            // verify that we received the same session ID that we requested
            
            // save opened session
            
        }

        public void CloseSession()
        {
            // TODO: SDClient.CloseSession()

            ValidateConnected();

            // send close session to the server
            
            // no session open
            
        }

        public string GetDocument(string documentName)
        {
            // TODO: SDClient.GetDocument()

            ValidateConnected();

            // send get to the server
            
            // get the server's response
            return "TODO";
        }

        public void PostDocument(string documentName, string documentContents)
        {
            // TODO: SDClient.PostDocument()

            ValidateConnected();

            // send the document to the server
            
            // get the server's response
            
        }

        private void ValidateConnected()
        {
            if (!connected)
                throw new Exception("Connot perform action. Not connected to server!");
        }

        private void ValidateDisconnected()
        {
            if (connected)
                throw new Exception("Connot perform action. Already connected to server!");
        }

        private void SendOpen()
        {
            // TODO: SDClient.SendOpen()

            // send open message to SD server
            
        }

        private void SendClose(ulong sessionId)
        {
            // TODO: SDClient.SendClose()

            // send close message to SD server
            
        }

        private void SendResume(ulong sessionId)
        {
            // TODO: SDClient.SendResume()

            // send resume message to SD server
            
        }

        private ulong ReceiveSessionResponse()
        {
            // TODO: SDClient.ReceiveSessionResponse()

            // get SD server's response to our last session request (open or resume)
            string line = reader.ReadLine();
            if (line == "accepted")
            {
                // yay, server accepted our session!
                // get the sessionID
                return 0;
            }
            else if (line == "rejected")
            {
                // boo, server rejected us!
                throw new Exception("TODO");
            }
            else if (line == "error")
            {
                // boo, server sent us an error!
                throw new Exception("TODO");
            }
            else
            {
                throw new Exception("Expected to receive a valid session response, instead got... " + line);
            }
        }

        private void SendPost(string documentName, string documentContents)
        {
            // TODO: SDClient.SendPost()

            // send post message to SD erer, including document name, length and contents

        }

        private void SendGet(string documentName)
        {
            // TODO: SDClient.SendGet()

            // send get message to SD server

        }

        private void ReceivePostResponse()
        {
            // TODO: SDClient.ReceivePostResponse()

            // get server's response to our last post request
            string line = reader.ReadLine();
            if (line == "success")
            {
                // yay, server accepted our request!
                
            }
            else if (line == "error")
            {
                // boo, server sent us an error!
                throw new Exception("TODO");
            }
            else
            {
                throw new Exception("Expected to receive a valid post response, instead got... " + line);
            }
        }

        private string ReceiveGetResponse()
        {
            // TODO: SDClient.ReceiveGetResponse()

            // get server's response to our last get request and return the content received
            string line = reader.ReadLine();
            if (line == "success")
            {
                // yay, server accepted our request!
                
                // read the document name, content length and content
                
                // return the content
                return "TODO";
            }
            else if (line == "error")
            {
                // boo, server sent us an error!
                throw new Exception("TODO");
            }
            else
            {
                throw new Exception("Expected to receive a valid get response, instead got... " + line);
            }
        }

        private string ReceiveDocumentContent(int length)
        {
            // TODO: SDClient.ReceiveDocumentContent()

            // read from the reader until we've received the expected number of characters
            // accumulate the characters into a string and return those when we received enough

            return "TODO";
        }
    }
}
