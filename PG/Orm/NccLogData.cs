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
    internal class NccLogData
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("start_time_sec")]
        public required double StartTimeSec { get; set; }

        [Column("end_time_sec")]
        public required double EndTimeSec { get; set; }

        [Column("idx_layer")]
        public required int IdxLayer { get; set; }

        [Column("is_tuning")]
        public required bool IsTuning { get; set; }

        [Column("idx_part")]
        public required int IdxPart { get; set; }

        [Column("idx_resume")]
        public required bool IdxResume { get; set; }

        [Column("x_pos")]
        public required double XPos { get; set; }

        [Column("y_pos")]
        public required double YPos { get; set; }

        // Foreign Key
        [ForeignKey("SessionInfoOrm")]
        [Column("SessionInfoOrm_id")]
        public required string SessionId { get; set; }

        public required SessionInfo Session { get; set; }  // Navigation property
    }
}
