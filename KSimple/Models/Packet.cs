using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSimple.Models
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