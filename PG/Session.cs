using PG.Fpga;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace PG
{
    public class Session
    {
        public enum SessionStatus
        {
            Ready,
            Configuring,
            Configured,
            Running,
            Paused,
            Stopped,
            Completed
        }
        public List<string> SessionLog { get; private set; }

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
            }
        }

        public SessionStatus Status { get; private set; }

        public string SessionId { get; private set; }
        public string SessionName { get; private set; }
        public string SessionDescription { get; private set; }
        public DateTime SessionStartTime { get; private set; }
        public DateTime? SessionEndTime { get; private set; }
        public string PatientId { get; private set; }

        public List<DaqData> FpgaRawData { get; private set; }

        public Session(string patientId = "", string sessionDescription = "")
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
            SessionMessage = "Session Created";
            Status = SessionStatus.Ready;
        }
        public void StartSession()
        {
            // Start FPGA
        }

    }
}
