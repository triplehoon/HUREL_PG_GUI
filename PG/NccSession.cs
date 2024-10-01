using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PG
{
    public class NccSession : PgSession
    {
        public NccSession(string patientId = "", string sessionDescription = "") : base(eSessionType.NCC, patientId, sessionDescription)
        {
        }        
    }
}
