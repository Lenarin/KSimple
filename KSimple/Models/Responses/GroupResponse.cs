using System;
using System.Collections.Generic;
using KSimple.Models.Entities;
using KSimple.Models.Misc;

namespace KSimple.Models.Responses
{
    public class GroupResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        
        public List<Guid> Storages { get; set; }
        public List<Guid> Templates { get; set; }
        public List<UserRight> UserRights { get; set; }

        public static GroupResponse FromGroup(Group group)
        {
            return new GroupResponse
            {
                Id = group.Id,
                Name = group.Name,
                Storages = group.StorageGroups.ConvertAll(sg => sg.StorageId),
                Templates = group.TemplateGroups.ConvertAll(tg => tg.TemplateId),
                UserRights = group.UserGroupRights.ConvertAll(ugr => new UserRight {UserId = ugr.UserId, Rights = ugr.Rights})
            };
        }
    }

    public sealed class UserRight
    {
        public Guid UserId;
        public string UserName;
        public string Email;
        public Right Rights;
    }
    
}