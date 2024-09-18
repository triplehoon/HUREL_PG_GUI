using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PG.Orm
{
    public class SessionInfo
    {
        [Key]
        [Column("session_id")]
        public string SessionId { get; set; }  // 환자번호_날짜 (e.g., 123456_230918)

        [Column("patient_number")]
        public string PatientNumber { get; set; }  // 환자번호 (6-digit)

        [Column("date")]
        public DateTime Date { get; set; }  // 날짜
        [Column("IsRunning")]
        public bool IsRunning { get; set; }
    }
}
