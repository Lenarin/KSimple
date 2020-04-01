using System;

namespace KSimple.Models
{
    public class StorageGroup
    {
        public Guid StorageId { get; set; }
        public Storage Storage { get; set; }
        
        public Guid GroupId { get; set; }
        public Group Group { get; set; }
    }
}