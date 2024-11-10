using PG.Fpga;
using PG.Orm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace PG
{
    public abstract class PgSession
    {
        private const string MAIN_FOLDER = "C:/HUREL/PG/MultiSlit/Sessions";
        private const string LOG_FILE_NAME = "log.txt";

        public enum eSessionType
        {
            NCC,
            SMC
        }
        public enum eSessionStatus
        {
            Ready,
            Configuring,
            Configured,
            Running,
            Paused,
            Stopped,
            Completed,
            FpgaError
        }
        public eSessionType SessionType { get; private set; }
        public List<string> SessionLog { get; private set; }
        public string SessionMessage { 
            get
            {
                return SessionLog.Last();
            }
            protected set
            {
                string time = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]");
                string log = time + " " + value;
                SessionLog.Add(log);
                Trace.WriteLine(log);
                // write to log file
                StreamWriter sw = new StreamWriter(SessionFolder + "/" + LOG_FILE_NAME, true);
                sw.WriteLine(log);
                sw.Close();
            }
        }

        private eSessionStatus _status;
        public eSessionStatus Status
        {
            get
            {
                return _status;
            }
            private set
            {
                SessionMessage = "Status changed from " + _status.ToString() + " to " + value.ToString();
                _status = value;
            }
        }
        public string SessionFolder { get; private set; }
        public Guid SessionId { get; private set; }
        public string SessionName { get; private set; }
        public string SessionDescription { get; private set; }
        public DateTime SessionStartTime { get; private set; }
        public DateTime? SessionEndTime { get; private set; }
        public string PatientId { get; private set; }
        public List<DaqData> FpgaRawData { get; private set; }
        public SessionInfo? SessionInfo { get; private set; }
        public PgDbContext PgDbContext { get; private set; }



        public PgSession(eSessionType sessionType, 
                         string patientId, 
                         string sessionDescription
                         )
        {
            if (patientId == "")
            {
                patientId = "JohnDoe";
            }

            SessionName = DateTime.Now.ToString("yyyy-MM-dd-HHmm") + "_" + patientId;
            SessionId = Guid.NewGuid();
            SessionDescription = sessionDescription;
            SessionStartTime = DateTime.Now;
            SessionEndTime = null;
            PatientId = patientId;

            // if patient id is longer than 6 digits, truncate it
            if (PatientId.Length > 6)
            {
                PatientId = PatientId.Substring(0, 6);
            }
            FpgaRawData = new List<DaqData>();
            SessionLog = new List<string>();
            // If the directory does not exist, create it.
            if (!System.IO.Directory.Exists(MAIN_FOLDER))
            {
                System.IO.Directory.CreateDirectory(MAIN_FOLDER);
            }
            // create folder with SessionId
            SessionFolder = MAIN_FOLDER + "/" + SessionName;

            if (System.IO.Directory.Exists(SessionFolder))
            {
                System.IO.Directory.Delete(SessionFolder, true);
            }
            System.IO.Directory.CreateDirectory(SessionFolder);
            // create log file
            StreamWriter sw = File.CreateText(SessionFolder + "/" + LOG_FILE_NAME);
            sw.Close();
            // close the file
            SessionType = sessionType;
            SessionMessage = "Session Created";
            SessionMessage = "Session Name: " + SessionName;
            SessionMessage = "Session Id: " + SessionId.ToString();
            SessionMessage = "Session Type: " + SessionType.ToString();
            Status = eSessionStatus.Ready;

            // create db context
            PgDbContext = new PgDbContext();
            // create session info
          
            SessionInfo = new SessionInfo();
            SessionInfo.SessionId = SessionId.ToString();
            SessionInfo.PatientNumber = PatientId;
            SessionInfo.Date = SessionStartTime;
            SessionInfo.IsRunning = false;
            this.PgDbContext.SessionInfos.Add(SessionInfo);

            this.PgDbContext.SaveChanges();

        }

        private CancellationToken sessionCancelToken = new CancellationToken();
        public virtual void StartSession()
        {
            Status = eSessionStatus.Running;
            SessionStartTime = DateTime.UtcNow;
            SessionMessage = "Session Started";
            CruxellWrapper.StartFpgaDaq(this.SessionFolder);
            
            while (CruxellWrapper.GetDataCount() == 0)
            {
                if (CruxellWrapper.GetDataCount() > 0)
                {
                    SessionMessage = "FPGA: Data Received";
                    
                    break;
                }

                if (DateTime.UtcNow - SessionStartTime > TimeSpan.FromSeconds(60))
                {
                    SessionMessage = "FPGA: No Data Received";
                    Status = eSessionStatus.FpgaError;
                    return;                    
                }

                Task.Delay(100).Wait();
            }
            sessionCancelToken = new CancellationToken();
            // update default session info to db
        }
        // Task to run in the background
        // 1. Dump fpga data to db
        // 2. Dump log to db
        // 3. get session data from db
        // 4. update session info to db

        private async Task UpdateFpgaDataToDbTask(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                List<DaqData> daqDatas = CruxellWrapper.GetDaqData();

                if (daqDatas.Count > FpgaRawData.Count)
                {
                    for (int i = FpgaRawData.Count; i < daqDatas.Count; ++i)
                    {
                        FpgaRawData.Add(daqDatas[i]);
                    }
                }


                await Task.Delay(100);
            }
        }
        public virtual void StopSession()
        {
            Status = eSessionStatus.Stopped;
            SessionEndTime = DateTime.UtcNow;
            SessionMessage = "Session Stopped";
            CruxellWrapper.StopFpgaDaq();
        }
        
        public static PgSession GetSessionFromFolder(string folderDir)
        {
            throw new NotImplementedException ();
        }
    }
}
