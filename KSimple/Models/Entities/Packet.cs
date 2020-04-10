using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KSimple.Models.Misc;
using Newtonsoft.Json.Linq;

namespace KSimple.Models.Entities
{
    public class Packet
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public Guid StorageId { get; set; }

        public long? UserTimestamp { get; set; }
        public long? ServerTimestamp { get; set; }
        public Dictionary<string, JToken> Data { get; set; }
        
    }
}