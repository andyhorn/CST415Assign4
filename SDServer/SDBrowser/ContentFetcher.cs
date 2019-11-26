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
            // TODO: ContentFetcher.ContentFetcher()
            // initially empty protocols dictionary
            
        }

        public void Close()
        {
            // TODO: ContentFetcher.Close()
            // close each protocol client
            
        }

        public void AddProtocol(string name, IProtocolClient client)
        {
            // TODO: ContentFetcher.AddProtocol()
            // save the protocol client under the given name
            
        }

        public string Fetch(string address)
        {
            // TODO: ContentFetcher.Fetch()
            // parse the address
            // Address format:
            //    < type >:< server IP >:< resource >
            //    Where…
            //      < type > is one of “SD” and “FT”
            //      < server IP > is the IP address of the server to contact
            //      < resource > is the name of the resource to request from the server
            

            // retrieve the correct protocol client for the requested protocol
            // watch out for invalid type
            

            // get the content from the protocol client, using the given IP address and resource name
            
            
            // return the content
            return "TODO";
        }
    }
}
