using System;
using System.Collections.Generic;

namespace KSimple.Models
{
    public class Group
    {
        public Guid Id { get; set; }
        
        public List<StorageGroup>  StorageGroups { get; set; }
        public List<TemplateGroup> TemplateGroups { get; set; }
        public List<UserGroupRight> UserGroupRights { get; set; }
    }
}