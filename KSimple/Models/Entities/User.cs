using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KSimple.Models.Misc;

namespace KSimple.Models.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        
        public Guid DefaultGroupId { get; set; }

        public List<UserGroupRight> UserGroupRights { get; set; }
        
        public List<RefreshToken> RefreshTokens { get; set; }

        /// <summary>
        /// Merge users, except ID and Role fields
        /// </summary>
        /// <param name="user"></param>
        public void Merge(User user)
        {
            if (user.Name != null) Name = user.Name;
            if (user.Email != null) Email = user.Email;
            if (user.Password != null) Password = user.Password;
        }

        public void SetRole(string role)
        {
            Role = role;
        }
    }
}