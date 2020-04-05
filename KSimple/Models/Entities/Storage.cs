using System;
using System.Collections.Generic;
using KSimple.Models.Misc;

namespace KSimple.Models.Entities
{
    public class Storage
    {
        public Guid Id { get; set; }
        public string UserDefinedId { get; set; }
        public string Name { get; set; }
        
        public Guid TemplateId { get; set; }
        public Template Template { get; set; }

        public List<StorageGroup> StorageGroups { get; set; }
        public List<Packet> Packets { get; set; }
        
        public Status Status { get; set; }
    }
}