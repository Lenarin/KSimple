using System;
using System.ComponentModel.DataAnnotations;

namespace KSimple.Models.Entities
{
    public class Packet
    {
        [Key]
        public int Id { get; set; }
        
        public Storage Storage { get; set; }
        
        public DateTime Timestamp { get; set; }
        public string Data { get; set; }
    }
}