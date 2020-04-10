using System;
using System.Collections.Generic;
using KSimple.Models.Misc;

namespace KSimple.Models.Entities
{
    public class Group
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        
        public List<StorageGroup>  StorageGroups { get; set; }
        public List<TemplateGroup> TemplateGroups { get; set; }
        public List<UserGroupRight> UserGroupRights { get; set; }
    }
}