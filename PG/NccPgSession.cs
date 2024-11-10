using HUREL.PG;
using HUREL.PG.NccHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PG
{
    public class NccPgSession : PgSession
    {
        public NccPgSession(string patientId, 
                         string sessionDescription,
                         string calibrationFilePath,
                         string pldPath,
                         string pld3dPath) : base(eSessionType.NCC, patientId, sessionDescription)
        {
            // log session creation
            SessionMessage = "Session created";
            // calibration file path is valid and is *.mat 
            if (calibrationFilePath == null)
            {
                throw new Exception("Calibration file path is not set");
            }
            if (!System.IO.File.Exists(calibrationFilePath))
            {
                throw new Exception("Calibration file does not exist");
            }
            if (!calibrationFilePath.EndsWith(".mat"))
            {
                throw new Exception("Calibration file is not a .mat file");
            }
            // pld file path is valid and is *.pld
            if (pldPath == null)
            {
                throw new Exception("PLD file path is not set");
            }
            if (!System.IO.File.Exists(pldPath))
            {
                throw new Exception("PLD file does not exist");
            }
            if (!pldPath.EndsWith(".pld"))
            {
                throw new Exception("PLD file is not a .pld file");
            }
            // pld3d file path is valid and is *.pld
            if (pld3dPath == null)
            {
                throw new Exception("3D PLD file path is not set");
            }
            if (!System.IO.File.Exists(pld3dPath))
            {
                throw new Exception("3D PLD file does not exist");
            }
            if (!pld3dPath.EndsWith(".pld"))
            {
                throw new Exception("3D PLD file is not a .pld file");
            }
            if (SessionInfo == null)
            {
                throw new Exception("SessionInfo is not set");
            }
            // set paths
            // copy to config folder with the same name
            string configFolder = SessionFolder + "/config";
            string calibrationFileName = System.IO.Path.GetFileName(calibrationFilePath);
            string pldFileName = System.IO.Path.GetFileName(pldPath);
            string pld3dFileName = System.IO.Path.GetFileName(pld3dPath);
            System.IO.Directory.CreateDirectory(configFolder);
            System.IO.File.Copy(calibrationFilePath, configFolder + "/" + calibrationFileName);
            System.IO.File.Copy(pldPath, configFolder + "/" + pldFileName);
            System.IO.File.Copy(pld3dPath, configFolder + "/" + pld3dFileName);

            // set paths
            SessionInfo.PathCalibrationFile = configFolder + "/" + calibrationFileName;
            SessionInfo.PathPldFile = configFolder + "/" + pldFileName;
            SessionInfo.Path3dPldFile = configFolder + "/" + pld3dFileName;
            // update session info
            this.PgDbContext.Update(SessionInfo);
            this.PgDbContext.SaveChanges();
            SessionMessage = "Session info updated. Copy calibration and pld files to config folder";

        }
        private Task? ftpStreamTask = null;
        // Start log session
        private void StartFtpStream(bool isTest)
        {
            if (ftpStreamTask != null)
            {
                SessionMessage = "FTP stream is already running";
                return;
            }
            SessionMessage = "FTP stream started";
            if (isTest)
            {
                NccFtp.OpenFtpSession(new WinSCP.SessionOptions()
                {
                    HostName = "localhost",
                    UserName = "test",
                    Password = "test",
                    Protocol = WinSCP.Protocol.Ftp,
                    PortNumber = 21,
                    FtpSecure = WinSCP.FtpSecure.Explicit,
                    TlsHostCertificateFingerprint = "db:10:dd:0c:2c:f9:de:c8:c9:65:91:4c:f7:18:af:80:97:be:aa:e0:a6:c2:3b:fa:a0:c7:4b:18:ae:c3:dc:79"
                });
            }
            else
            {
                NccFtp.OpenFtpSession();
            }
            //NccFtp.SyncAndDownloadLogFile(
            //    localDirectory: SessionFolder,
            //    remoteDirectory: "logs");
            string nccLogFolder = SessionFolder + "/ncc_log";
            // create log folder
            System.IO.Directory.CreateDirectory(nccLogFolder);

            if (isTest)
            {
                ftpStreamTask = NccFtp.SyncAndDownloadLogFile(
                    localDirectory: nccLogFolder,
                    remoteDirectory: "logs");
            }
            else
            {
                ftpStreamTask = NccFtp.SyncAndDownloadLogFile(
                    localDirectory: nccLogFolder);
            }
        }
        // Stop log session
        private void StopFtpStream()
        {
            if (ftpStreamTask == null)
            {
                SessionMessage = "FTP stream is not running";
                return;
            }
            NccFtp.StopSyncAndDownloadLogFile();
            NccFtp.CloseFtpSession();
            ftpStreamTask.Wait();
            ftpStreamTask = null;

        }

        public List<NccLayer> NccLayers = new List<NccLayer>();
        private void ReadLogTask()
        {
            // read log data
            string logFolder = SessionFolder + "/ncc_log";

            List<string> files = Directory.GetFiles(logFolder).ToList();

            string? idtFile = files.Find(x => x.Contains("idt_config"));

            if (idtFile != null)
            {
                Console.WriteLine("IDT file: " + idtFile);
            }
            else
            {
                Console.WriteLine("IDT file is not found");
            }


            // read log data
            NccLogParameter logParameter = Ncc.GetNccLogParameter(idtFile);

            // print log data
            Console.WriteLine("Log data");
            // coefficient
            Console.WriteLine("Coefficient: " + logParameter.coeff_x + ", " + logParameter.coeff_y);

            // read map record and specific record
            List<NccLayer> layers = new List<NccLayer>();
            foreach (string file in files)
            {
                if (file.Contains("map_record") && file.Contains("xdr"))
                {
                    // find specific xdr change record to specif
                    string specifFile = file.Replace("map_record", "map_specif");
                    Console.WriteLine("Map record: " + file);
                    Console.WriteLine("Map specif: " + specifFile);

                    NccLayer nccLayer = new NccLayer(file, specifFile, logParameter.coeff_x, logParameter.coeff_y);


                    layers.Add(nccLayer);
                }
            }

            layers.Sort(Ncc.SortLayer);
            NccLayers = layers;
        }

        public void StartSession()
        {

        }
    }
}
