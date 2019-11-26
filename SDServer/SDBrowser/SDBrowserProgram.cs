// SDBrowserProgram.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 

using System;
using System.Windows.Forms;

namespace SDBrowser
{
    static class SDBrowserProgram
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
