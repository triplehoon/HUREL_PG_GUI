using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PG.Orm
{
    public class SessionLogSpots
    {
        // Foreign key for SessionInfo
        [ForeignKey("SessionInfoId")]
        [Column("session_id")]
        public string SessionInfoId { get; set; }
        public SessionInfo? SessionInfo { get; set; }

        // int based key
        [Key]
        [Column("id")]
        public int Id { get; set; }

        // spot_sequence_number
        [Column("spot_sequence_number")]
        public int SpotSequenceNumber { get; set; }

        
        // Start time
        [Column("start_time")]
        public DateTime StartTime { get; set; }
        // End time
        [Column("end_time")]
        public DateTime EndTime { get; set; }

        // layer number
        [Column("layer_index")]
        public int LayerIndex { get; set; }        

        // is_tunning
        [Column("is_tunning")]
        public bool IsTunning { get; set; } = false;
        // part_index
        [Column("part_index")]
        public int PartIndex { get; set; }
        //resume index
        [Column("resume_index")]
        public int ResumeIndex { get; set; }
        // position X
        [Column("position_x")]
        public double PositionX { get; set; }
        // position Y
        [Column("position_y")]
        public double PositionY { get; set; }

        // To String
        public override string ToString()
        {
            return $"Id: {Id}, SessionInfoId: {SessionInfoId}, SpotSequenceNumber: {SpotSequenceNumber}, StartTime: {StartTime}, EndTime: {EndTime}, LayerIndex: {LayerIndex}, IsTunning: {IsTunning}, PartIndex: {PartIndex}, ResumeIndex: {ResumeIndex}, PositionX: {PositionX}, PositionY: {PositionY}";
        }
    }
}
