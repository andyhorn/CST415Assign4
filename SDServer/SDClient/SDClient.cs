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
            // save server address/port
            this.sdServerAddress = sdServerAddress;
            this.sdServerPort = sdServerPort;

            // initialize to not connected to server
            clientSocket = null;
            stream = null;
            reader = null;
            writer = null;
            connected = false;

            // no session open at this time
            sessionID = 0;
        }

        public ulong SessionID { get { return sessionID; } set { sessionID = value; } }

        public void Connect()
        {
            ValidateDisconnected();

            // create a client socket and connect to the FT Server's IP address and port
            clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(sdServerAddress, sdServerPort);

            // establish the network stream, reader and writer
            stream = new NetworkStream(clientSocket);
            reader = new StreamReader(stream, UTF8Encoding.ASCII);
            writer = new StreamWriter(stream, UTF8Encoding.ASCII);

            // now connected
            connected = true;
        }

        public void Disconnect()
        {
            ValidateConnected();

            // close writer, reader and stream
            writer.Close();
            reader.Close();
            stream.Close();

            // disconnect and close socket
            clientSocket.Disconnect(false);
            clientSocket.Close();

            // now disconnected
            connected = false;
        }

        public void OpenSession()
        {
            ValidateConnected();

            // send open command to server
            SendOpen();

            // receive server's response, hopefully with a new session id
            sessionID = ReceiveSessionResponse();
        }

        public void ResumeSession()
        {
            ValidateConnected();

            // send resume session to the server
            SendResume();

            // receive server's response, hopefully confirming our sessionId
            var resumedSessionId = ReceiveSessionResponse();

            // verify that we received the same session ID that we requested
            if (resumedSessionId != sessionID)
            {
                throw new Exception($"Requested resuming session {sessionID}, server resumed session {resumedSessionId}");
            }
        }

        public void CloseSession()
        {
            ValidateConnected();

            // send close request to the server
            SendClose();

            // receive closed response from the server
            var closedSessionId = ReceiveSessionResponse();
            if (closedSessionId != sessionID)
            {
                throw new Exception($"Server closed wrong session. Requested {sessionID}, received {closedSessionId}");
            }

            // no session open
            sessionID = 0;
        }

        public string GetDocument(string documentName)
        {
            ValidateConnected();

            // send get to the server
            SendGet(documentName);

            // get the server's response
            var response = ReceiveGetResponse();
            return response;
        }

        public void PostDocument(string documentName, string documentContents)
        {
            ValidateConnected();

            // send the document to the server
            SendPost(documentName, documentContents);

            // get the server's response
            ReceivePostResponse();
        }

        private void ValidateConnected()
        {
            if (!connected)
                throw new Exception("Cannot perform action. Not connected to server!");
        }

        private void ValidateDisconnected()
        {
            if (connected)
                throw new Exception("Cannot perform action. Already connected to server!");
        }

        private void SendOpen()
        {
            // send open message to SD server
            writer.WriteLine("open");
            writer.Flush();
            Console.WriteLine("Sent 'open' to server");
        }

        private void SendClose()
        {
            // send close message to SD server
            writer.WriteLine("close");
            writer.WriteLine(sessionID);
            writer.Flush();
            Console.WriteLine($"Sent 'close' to server for session id {sessionID}");
        }

        private void SendResume()
        {
            // send resume message to SD server
            writer.WriteLine("resume");
            writer.WriteLine(sessionID);
            writer.Flush();
            Console.WriteLine($"Sent 'resume' to server for session {sessionID}");
        }

        private ulong ReceiveSessionResponse()
        {
            // get SD server's response to our last session request (open or resume or close)
            string line = reader.ReadLine();
            if (line == "accepted")
            {
                // yay, server accepted our session!
                // get the sessionID
                return ulong.Parse(reader.ReadLine());
            }
            else if (line == "closed")
            {
                var sessionId = reader.ReadLine();
                Console.WriteLine($"Server closed session {sessionId}");
                return ulong.Parse(sessionId);
            }
            else if (line == "rejected")
            {
                // boo, server rejected us!
                var reason = reader.ReadLine();
                Console.WriteLine($"Server rejected session request: {reason}");
                throw new Exception(reason);
            }
            else if (line == "error")
            {
                // boo, server sent us an error!
                var error = reader.ReadLine();
                Console.WriteLine($"Server sent an error: {error}");
                throw new Exception(error);
            }
            else
            {
                throw new Exception("Expected to receive a valid session response, instead got: " + line);
            }
        }

        private void SendPost(string documentName, string documentContents)
        {
            // send post message to SD server, including document name, length and contents
            writer.WriteLine("post");
            writer.WriteLine(documentName);
            writer.WriteLine(documentContents.Length);
            writer.Write(documentContents);
            writer.Flush();

            Console.WriteLine($"Sent 'post' to server for '{documentName}' of {documentContents.Length} bytes");
        }

        private void SendGet(string documentName)
        {
            // send get message to SD server
            writer.WriteLine("get");
            writer.WriteLine(documentName);
            writer.Flush();
            Console.WriteLine($"Sent 'get' to server for document {documentName}");
        }

        private void ReceivePostResponse()
        {
            // get server's response to our last post request
            string line = reader.ReadLine();
            if (line == "success")
            {
                // yay, server accepted our request!
                Console.WriteLine("Received 'success' from server");
                return;
            }
            else if (line == "error")
            {
                // boo, server sent us an error!
                throw new Exception($"Error, failed to post document: {reader.ReadLine()}");
            }
            else
            {
                throw new Exception("Expected to receive a valid post response, instead got... " + line);
            }
        }

        private string ReceiveGetResponse()
        {
            // get server's response to our last get request and return the content received
            string line = reader.ReadLine();
            if (line == "success")
            {
                // yay, server accepted our request!

                // read the document name, content length and content
                var name = reader.ReadLine();
                var length = int.Parse(reader.ReadLine());
                var content = ReceiveDocumentContent(length);
                
                // return the content
                return content;
            }
            else if (line == "error")
            {
                // boo, server sent us an error!
                throw new Exception($"{reader.ReadLine()}");
            }
            else
            {
                throw new Exception("Expected to receive a valid 'get' response, instead got... " + line);
            }
        }

        private string ReceiveDocumentContent(int length)
        {
            // read from the reader until we've received the expected number of characters
            // accumulate the characters into a string and return those when we received enough
            int bytesLeftToRead = length;
            string contents = "";

            while (bytesLeftToRead > 0)
            {
                char[] buffer = new char[bytesLeftToRead];
                int bytesRead = reader.Read(buffer, 0, bytesLeftToRead);
                string s = new string(buffer);

                contents += s;
                bytesLeftToRead -= bytesRead;
            }

            Console.WriteLine($"Received {contents.Length} bytes of data");

            return contents;
        }
    }
}
