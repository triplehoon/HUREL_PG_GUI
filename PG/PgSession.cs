using PG.Fpga;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace PG
{
    public class PgSession
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
            Completed
        }
        public eSessionType SessionType { get; private set; }
        public List<string> SessionLog { get; private set; }
        public string SessionMessage { 
            get
            {
                return SessionLog.Last();
            }
            private set
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

        public PgSession(eSessionType sessionType, string patientId = "", string sessionDescription = "")
        {
            if (patientId == "")
            {
                patientId = "JohnDoe";
            }
            SessionName = DateTime.Now.ToString("yyyy-MM-dd-hhmm") + "_" + patientId;
            SessionId = Guid.NewGuid();
            SessionDescription = sessionDescription;
            SessionStartTime = DateTime.Now;
            SessionEndTime = null;
            PatientId = patientId;
            FpgaRawData = new List<DaqData>();
            SessionLog = new List<string>();
            // If the directory does not exist, create it.
            if (!System.IO.Directory.Exists(MAIN_FOLDER))
            {
                System.IO.Directory.CreateDirectory(MAIN_FOLDER);
            }
            // create folder with SessionId
            SessionFolder = MAIN_FOLDER + "/" + SessionName;
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
        }

        public void StartSession()
        {
            
        }
        private void UpdateSessionInfoToDb()
        {
            // update session info to db
        }
        private bool RunMatlabAnalysis()
        {
            return true;
        }

        private bool ConnectNccFtpServer()
        {

            return true;
        }
        private bool RunFpgaDaq()
        {
            return true;
        }

        private async Task UpdateFpgaToDb()
        {
            // update fpga data to db
        }

        private async Task UpdateNccToDb()
        {
            // update ncc data to db
        }
        
        public void StopSession()
        {
            SessionMessage = "Session Stopped";
            SessionEndTime = DateTime.Now;
            Status = eSessionStatus.Stopped;
            UpdateSessionInfoToDb();
        }
    }
}
