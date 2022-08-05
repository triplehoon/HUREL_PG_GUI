using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSCP;
using System.Threading;

namespace HUREL.PG.Ncc
{
    public static class LogFileSync
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

        public static (bool, string) OpenFtpSession(string hostName = "10.1.30.80", string userName = "clinical", string passWord = "Madne55")
        {
            SessionOptions sessionOptions = new SessionOptions
            {
                GiveUpSecurityAndAcceptAnySshHostKey = true,

                Protocol = Protocol.Sftp,
                HostName = hostName,
                UserName = userName,
                Password = passWord,
                PortNumber = 22,
            };

            try
            {
                ftpSession = new WinSCP.Session();
                ftpSession.Open(sessionOptions);
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
        
        public static async Task SyncAndDownloadLogFile(string localPath, bool testMode = false)
        {


            DateTime syncStartTime = DateTime.Now - TimeSpan.FromSeconds(10);

            if (!IsSessionOpen)
            {
                Trace.WriteLine("SyncAndDownloadLogFile Started");
                IsFtpStart = false; 
                return ;
            }

            IsFtpStart = true;
            // Connect to Server
            string remotePath = "/PBSdata/test/clinical/tr3/planId/beamId/fractionId";
            if (testMode)
            {
                remotePath = "/home/csh/PBSdata/test/clinical/tr3/planId/beamId/fractionId";
            }
            //
            //string remotePath = "";
            List<RemoteFileInfo> checkedRemoteFiles = new List<RemoteFileInfo>();
            Trace.WriteLine("SyncAndDownloadLogFile Started");
            await Task.Run(() =>
            {
                while (IsFtpStart)
                {
                    try
                    {   if (ftpSession == null)
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
                                    ftpSession.GetFileToDirectory(file.FullName, localPath);
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
                    }
                }
            });
            Trace.WriteLine($"");
            Trace.WriteLine($"FTP Finished");
            Trace.WriteLine($"");

            return ;
        }
        public static void StopSyncAndDownloadLogFile()
        {
            IsFtpStart = false;
        }
    }
}
