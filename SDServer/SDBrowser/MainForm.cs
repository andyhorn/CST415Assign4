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
            // default command line values
            string prsIP = "127.0.0.1";
            ushort prsPort = 30000;

            // TODO: parse the command line and get the PRS Server's IP Address and Port number
            // -prs < PRS IP address>:< PRS port >
            // NOTE: args[0] is the name of the program, first true argument is at args[1]
            //string[] args = Environment.GetCommandLineArgs();


            // instantiate the fetcher and add the support SD and FT protocols
            fetcher = new ContentFetcher();
            fetcher.AddProtocol("SD", new SDProtocolClient(prsIP, prsPort));
            fetcher.AddProtocol("FT", new FTProtocolClient(prsIP, prsPort));

            InitializeComponent();
        }

        private void buttonGo_Click(object sender, EventArgs e)
        {
            // user clicked the Go! button

            try
            {
                // grab the address from the address bar
                var address = textboxAddress.Text;

                if (string.IsNullOrWhiteSpace(address))
                {
                    throw new Exception("Address cannot be empty");
                }

                // fetch the content
                var content = fetcher.Fetch(address);

                if (content == null)
                {
                    throw new Exception("Received null content");
                }

                // put the content in the content box
                textboxContent.Text = content;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // close the fetcher so it can close it's sessions with the servers
            try
            {
                fetcher.Close();
            }
            catch (Exception ex)
            {
                // nothing to do here, just catch the exception and hide it from the user
            }
        }
    }
}
