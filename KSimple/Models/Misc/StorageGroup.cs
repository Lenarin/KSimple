using System;
using KSimple.Models.Entities;

namespace KSimple.Models.Misc
{
    public class StorageGroup
    {
        public Guid StorageId { get; set; }
        public Storage Storage { get; set; }
        
        public Guid GroupId { get; set; }
        public Group Group { get; set; }
    }
}