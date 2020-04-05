using System;
using KSimple.Models.Entities;

namespace KSimple.Models.Misc
{
    public class UserGroupRight
    {
        public Guid UserId { get; set; }
        public User User { get; set; }
        
        public Guid GroupId { get; set; }
        public Group Group { get; set; }
        
        public Right Rights { get; set; }
    }
}