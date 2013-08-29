﻿using System;
using System.Net;
using System.Windows.Forms;
using CommandLine;

namespace TA.SharpBooru.Client.GUI
{
    public class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            Options options = new Options();
            try
            {
                if (Parser.Default.ParseArguments(args, options))
                {
                    (new Program()).Run(options);
                    return 0;
                }
                else OutputOnGUIandCLI(options.GetUsage(), "Usage", MessageBoxIcon.Information);
            }
            catch (BooruProtocol.BooruException bEx)
            {
                if (bEx.ErrorCode == BooruProtocol.ErrorCode.ProtocolVersionMismatch)
                    OutputOnGUIandCLI("Connection to the server was successfull, but the server is using a newer version of the protocol. Please redownload the client and try again.", "Protocol version mismatch", MessageBoxIcon.Error);
                else if (bEx.ErrorCode == BooruProtocol.ErrorCode.LoginFailed)
                    OutputOnGUIandCLI("Username and/or password incorrect. Please try again.", "Login failed", MessageBoxIcon.Error);
                else throw;
            }
            catch (Exception ex) { TryHandleException(ex); }
            return 1;
        }

        public static void TryHandleException(Exception ex)
        {
            string exName = ex.GetType().Name;
            string exMsg = string.Format("{0}: {1}", exName, ex.Message);
            OutputOnGUIandCLI(exMsg, exName, MessageBoxIcon.Error);
        }

        public static void OutputOnGUIandCLI(string Text, string Title, MessageBoxIcon Icon)
        {
            try { MessageBox.Show(Text, Title, MessageBoxButtons.OK, Icon); }
            catch { Console.WriteLine(Text); }
        }

        public void Run(Options options)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            IPEndPoint endPoint = Helper.GetIPEndPointFromString(options.Server);
            bool usernameSpecified = !string.IsNullOrWhiteSpace(options.Username);
            bool passwordSpecified = !string.IsNullOrEmpty(options.Password);

            Booru booru = null;
            if (!(usernameSpecified && passwordSpecified))
            {
                string username = options.Username;
                string password = string.Empty; //Don't fill in password automatically
                if (LoginDialog.ShowDialog(ref username, ref password))
                    booru = new Booru(endPoint, username, password);
                else return;
            }
            else booru = new Booru(endPoint, options.Username, options.Password);

            try
            {
                using (MainForm mForm = new MainForm(booru))
                    Application.Run(mForm);
            }
            finally
            {
                booru.Dispose();
                Helper.CleanTempFolder();
            }
        }
    }
}