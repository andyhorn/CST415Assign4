// MainForm.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 

using System;
using System.Windows.Forms;

namespace SDBrowser
{
    public partial class MainForm : Form
    {
        private ContentFetcher fetcher;

        public MainForm()
        {
            // TODO: MainForm.MainForm()
            // default command line values
            string prsIP = "127.0.0.1";
            ushort prsPort = 30000;

            // parse the command line and get the PRS Server's IP Address and Port number
            // -prs < PRS IP address>:< PRS port >
            // NOTE: args[0] is the name of the program, first true argument is at args[1]
            //string[] args = Environment.GetCommandLineArgs();
            

            // instantiate the fetcher and add the support SD and FT protocols
            

            InitializeComponent();
        }

        private void buttonGo_Click(object sender, EventArgs e)
        {
            // TODO: MainForm.buttonGo_Click()
            // user clicked the Go! button

            // grab the address from the address bar
            
            // fetch the content
            
            // put the content in the content box
            
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // TODO: MainForm.MainForm_FormClosed()
            // close the fetcher so it can close it's sessions with the servers

        }
    }
}
