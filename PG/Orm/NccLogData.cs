using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSCP;

namespace PG.Orm
{
    public class NccLogData
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("start_time_sec")]
        public double StartTimeSec { get; set; }

        [Column("end_time_sec")]
        public double EndTimeSec { get; set; }

        [Column("idx_layer")]
        public int IdxLayer { get; set; }

        [Column("is_tuning")]
        public bool IsTuning { get; set; }

        [Column("idx_part")]
        public int IdxPart { get; set; }
        [Column("idx_resume")]
        public bool IdxResume { get; set; }

        [Column("x_pos")]
        public double XPos { get; set; }

        [Column("y_pos")]
        public double YPos { get; set; }

        // Foreign Key
        [ForeignKey("SessionInfoOrm")]
        [Column("SessionInfoOrm_id")]
        public string SessionId { get; set; }

        public SessionInfo Session { get; set; }  // Navigation property
    }
}
