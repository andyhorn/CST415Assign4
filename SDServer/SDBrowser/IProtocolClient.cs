// IProtocolClient.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 

namespace SDBrowser
{
    // interface of a client for retrieving "documents" from a server
    // classes that implement this interface...
    // implement the protcol used to connect, retrieve and disconnect

    interface IProtocolClient
    {
        string GetDocument(string serverIP, string documentName);
        void Close();
    }
}
