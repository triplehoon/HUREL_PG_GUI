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
        public string? SessionId { get; set; }  // 환자번호_날짜 (e.g., 123456_230918)

        [Column("patient_number")]
        public string? PatientNumber { get; set; }  // 환자번호 (6-digit)

        private DateTime _date;

        [Column("date")]
        public DateTime Date
        {
            get => _date;
            set => _date = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
        // 날짜
        [Column("is_running")]
        public bool IsRunning { get; set; }
        // path calibration file
        [Column("path_calibration_file")]
        public string? PathCalibrationFile { get; set; }
        [Column("path_pld_file")]
        public string? PathPldFile { get; set; }
        [Column("path_3d_pld_file")]
        public string? Path3dPldFile { get; set; }

    }
}
