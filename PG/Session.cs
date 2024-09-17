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
    public class Session
    {
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
        const string logFileName = "log.txt";
        public string SessionMessage { 
            get
            {
                return SessionLog.Last();
            }
            private set
            {
                string time = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]");
                string log = SessionId + " " + time + " " + value;
                SessionLog.Add(log);
                Trace.WriteLine(log);
                // write to log file
                StreamWriter sw = new StreamWriter(SessionId + "/" + logFileName, true);
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

        public string SessionId { get; private set; }
        public string SessionDescription { get; private set; }
        public DateTime SessionStartTime { get; private set; }
        public DateTime? SessionEndTime { get; private set; }
        public string PatientId { get; private set; }

        public List<DaqData> FpgaRawData { get; private set; }

        public Session(eSessionType sessionType, string patientId = "", string sessionDescription = "")
        {
            if (patientId == "")
            {
                patientId = Guid.NewGuid().ToString();
            }
            SessionId = DateTime.Now.ToString("yyyy-MM-dd-hh:mm") + "-" + patientId;
            SessionDescription = sessionDescription;
            SessionStartTime = DateTime.Now;
            SessionEndTime = null;
            PatientId = patientId;
            FpgaRawData = new List<DaqData>();
            SessionLog = new List<string>();
            // create folder with SessionId
            Directory.CreateDirectory(SessionId);
            // create log file
            File.CreateText(SessionId + "/" + logFileName);
            SessionType = sessionType;
            SessionMessage = "Session Created";
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
