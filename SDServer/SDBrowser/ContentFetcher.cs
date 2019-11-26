// ContentFetcher.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 

using System;
using System.Collections.Generic;


namespace SDBrowser
{
    class ContentFetcher
    {
        private Dictionary<string, IProtocolClient> protocols;  // protocol name --> protocol client instance

        public ContentFetcher()
        {
            // initially empty protocols dictionary
            protocols = new Dictionary<string, IProtocolClient>();
        }

        public void Close()
        {
            // close each protocol client
            foreach (var protocol in protocols.Values)
            {
                protocol.Close();
            }
        }

        public void AddProtocol(string name, IProtocolClient client)
        {
            // save the protocol client under the given name
            protocols.Add(name, client);
        }

        public string Fetch(string address)
        {
            // parse the address
            // Address format:
            //    < type >:< server IP >:< resource >
            //    Where…
            //      < type > is one of “SD” and “FT”
            //      < server IP > is the IP address of the server to contact
            //      < resource > is the name of the resource to request from the server
            var parts = address.Split(':');

            if (parts.Length != 3)
            {
                throw new Exception("Invalid address");
            }

            var type = parts[0];
            var ip = parts[1];
            var resource = parts[2];


            // retrieve the correct protocol client for the requested protocol
            // watch out for invalid type
            if (!protocols.ContainsKey(type))
            {
                throw new Exception("Unrecognized protocol type");
            }

            var client = protocols[type];

            // get the content from the protocol client, using the given IP address and resource name
            if (string.IsNullOrWhiteSpace(ip))
            {
                throw new Exception("IP address cannot be empty");
            }
            if (string.IsNullOrWhiteSpace(resource))
            {
                throw new Exception("Resource name cannot be empty");
            }

            var content = client.GetDocument(ip, resource);
            
            // return the content
            return content;
        }
    }
}
