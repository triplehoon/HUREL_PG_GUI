using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WinSCP;

namespace PG.Orm
{
    public class FpgaData
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("channel")]
        public int Channel { get; set; }

        [Column("timestamp_ns")]
        public long TimestampNs { get; set; }

        [Column("signal_value_mv")]
        public double SignalValueMv { get; set; }

        // Foreign Key
        [ForeignKey("SessionInfoOrm")]
        [Column("SessionInfoOrm_id")]
        public required string SessionId { get; set; }

        public required Session Session { get; set; }  // Navigation property
    }
}
