﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace HexOnSteroids
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Add code to output the exception details to a message box/event log/log file,   etc.
            // Be sure to include details about any inner exceptions
            try
            {
                var f = new StreamWriter(HexOnSteroids.MainWindow.DocsPath + @"\errorlog_unh.txt");

                f.Write(e.Exception.ToString());
                f.WriteLine();
                f.WriteLine();
                f.Write(e.Exception.InnerException == null ? "None" : e.Exception.InnerException.Message);
                f.WriteLine();
                f.WriteLine();
                f.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't create errorlog!\n\n" + ex + "\n\n" + ex.InnerException);
            }

            MessageBox.Show("Hex on Steroids encountered a critical error and will be terminated.\n\nAn Error Log has been saved at " +
                            HexOnSteroids.MainWindow.DocsPath + @"\errorlog_unh.txt");
            try
            {
                Process.Start(HexOnSteroids.MainWindow.DocsPath + @"\errorlog_unh.txt");
            }
            catch (Exception)
            {

            }

            // Prevent default unhandled exception processing
            e.Handled = true;

            Environment.Exit(-1);
        }
    }
}