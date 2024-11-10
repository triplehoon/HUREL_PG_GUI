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
        [ForeignKey("SessionInfo")]
        [Column("SessionInfo_id")]
        public string? SessionInfoId { get; set; }

        public SessionInfo? SessionInfo { get; set; }  // Should refer to SessionInfo, not Session
    }
}


// CREATE SQL postgres

