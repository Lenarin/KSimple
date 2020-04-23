using System;
using System.Collections.Generic;
using KSimple.Models.Misc;

namespace KSimple.Models.Requests
{
    public class PutGroupRequest
    {
        public string Name { get; set; }
        public List<Guid> UsersToDelete { get; set; } 
        public List<UserRight> UserRightsToInsert { get; set; }
        public List<Guid> TemplatesToDelete { get; set; }
        public List<Guid> TemplatesToInsert { get; set; }
        public List<Guid> StoragesToDelete { get; set; }
        public List<Guid> StoragesToInsert { get; set; }
    }
    
    public class UserRight
    {
        public Guid Id { get; set; }
        public Right Rights { get; set; }
    }
}