using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PG.Orm
{
    public class SessionAggSpots
    {
        // int based key
        [Key]
        [Column("id")]
        public int Id { get; set; }

        // foreign key, session_id
        // Foreign key for SessionInfo
        [ForeignKey("SessionInfoId")]
        [Column("session_id")]
        public string? SessionInfoId { get; set; }
        public SessionInfo? SessionInfo { get; set; }

        // Foreign key for SessionLogSpots
        [ForeignKey("SessionLogSpots")]
        [Column("log_spot_id")]
        public int LogSpotId { get; set; }
        public SessionLogSpots? SessionLogSpots { get; set; }
        // plan layer index
        [Column("plan_layer_index")]
        public int PlanLayerIndex { get; set; }
        // plan proton beam energy
        [Column("plan_proton_beam_energy")]
        public double PlanProtonBeamEnergy { get; set; }
        // plan position X
        [Column("plan_position_x")]
        public double PlanPositionX { get; set; }
        // plan position Y
        [Column("plan_position_y")]
        public double PlanPositionY { get; set; }
        // plan position Z
        [Column("plan_position_z")]
        public double PlanPositionZ { get; set; }
        // plan monitor unit
        [Column("plan_monitor_unit")]
        public double PlanMonitorUnit { get; set; }
        // range difference
        [Column("range_difference")]
        public double RangeDifference { get; set; }
        // agreegate monitor unit
        [Column("agreegate_monitor_unit")]
        public double AgreegateMonitorUnit { get; set; }
        // true range 80
        [Column("true_range_80")]
        public double TrueRange80 { get; set; }


    }
}
