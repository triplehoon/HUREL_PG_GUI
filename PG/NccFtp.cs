using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSCP;
using System.Threading;

namespace HUREL.PG.NccHelper
{
    public static class NccFtp
    {
        public static bool IsFtpStart
        {
            get;
            private set;
        }
        public delegate void SyncAndDownloadLogHandler(string fileName);
        public static event SyncAndDownloadLogHandler? NewLogFileReceived;


        private static WinSCP.Session? ftpSession = null;
        private static bool IsSessionOpen = false;
        private static SessionOptions? sessionOptions;
        public static (bool, string) OpenFtpSession(
            SessionOptions? _sessionOptions = null)
        {
            if (_sessionOptions == null)
            {
                _sessionOptions = new SessionOptions
                {
                    GiveUpSecurityAndAcceptAnySshHostKey = true,
                    HostName = "10.1.30.80",
                    UserName = "clinical",
                    Password = "Madne55",
                    PortNumber = 22,
                    Protocol = Protocol.Sftp
                };
            }
            sessionOptions = _sessionOptions;
            try
            {
                ftpSession = new WinSCP.Session();
                ftpSession.Open(_sessionOptions);
                IsSessionOpen = true;
                return (true, "Success");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                IsSessionOpen = false;
                return (false, "Fail. " + ex.ToString());
            }

        }

        public static void CloseFtpSession()
        {
            if (IsSessionOpen)
            {
                IsFtpStart = false;
                Thread.Sleep(1000);
                try
                {
                    ftpSession?.Close();
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.ToString());
                }
                IsSessionOpen = false;
            }

        }

        public static async Task SyncAndDownloadLogFile(
            string localDirectory,
            string remoteDirectory)
        {


            DateTime syncStartTime = DateTime.Now - TimeSpan.FromSeconds(10);

            if (!IsSessionOpen)
            {
                Trace.WriteLine("SyncAndDownloadLogFile Started");
                IsFtpStart = false;
                return;
            }

            IsFtpStart = true;
            // Connect to Server
            string remotePath = remoteDirectory;

            //string remotePath = "";
            List<RemoteFileInfo> checkedRemoteFiles = new List<RemoteFileInfo>();
            Trace.WriteLine("SyncAndDownloadLogFile Started");
            await Task.Run(() =>
            {
                while (IsFtpStart)
                {
                    try
                    {
                        if (ftpSession == null)
                        {
                            IsFtpStart = false;
                            return;
                        }
                        RemoteDirectoryInfo directory = ftpSession.ListDirectory(remotePath);

                        foreach (RemoteFileInfo file in directory.Files)
                        {
                            if (file.FileType == '-' && file.LastWriteTime > syncStartTime && file.Length > 2 * 1024)
                            {
                                bool isExistAndUnchanged = false;
                                for (int i = 0; i < checkedRemoteFiles.Count; ++i)
                                {
                                    RemoteFileInfo checkFile = checkedRemoteFiles[i];
                                    if (checkFile.Name == file.Name)
                                    {
                                        if (checkFile.Length == file.Length)
                                        {
                                            isExistAndUnchanged = true;
                                            break;
                                        }
                                        else
                                        {
                                            checkedRemoteFiles[i] = file;
                                            break;
                                        }
                                    }
                                }
                                if (!isExistAndUnchanged)
                                {
                                    ftpSession.GetFileToDirectory(file.FullName, localDirectory);
                                    NewLogFileReceived?.Invoke(file.Name);
                                    checkedRemoteFiles.Add(file);
                                }
                            }
                        }
                        Thread.Sleep(1);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.ToString());
                        Thread.Sleep(1);
                        if (ftpSession != null)
                        {
                            ftpSession.Close();
                        }
                        // check scp session
                        if (ftpSession != null && !ftpSession.Opened)
                        {
                            ftpSession.Open(sessionOptions);
                        }
                    }
                }
            });
            Trace.WriteLine($"");
            Trace.WriteLine($"FTP Finished");
            Trace.WriteLine($"");

            return;
        }
        public static void StopSyncAndDownloadLogFile()
        {
            IsFtpStart = false;
        }
    }
}
