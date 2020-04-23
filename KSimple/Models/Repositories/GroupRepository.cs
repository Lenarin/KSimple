using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using KSimple.Models.Entities;
using KSimple.Models.Misc;
using KSimple.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Serilog.Core;
using SqlConnection = Microsoft.Data.SqlClient.SqlConnection;

namespace KSimple.Models.Repositories
{
    public class GroupRepository
    {
        private readonly DbConnection _connection;

        public GroupRepository(DbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<Group>> GetAllGroups()
        {
            return await _connection.QueryAsync<Group>(@"
                SELECT * 
                FROM Groups
            ");
        }

        public async Task<IEnumerable<Group>> GetAllUserGroups(Guid userId)
        {
            return await _connection.QueryAsync<Group>(@"
                SELECT *
                FROM Groups
                WHERE Id IN (
                    SELECT GroupId FROM UserGroupRights
                    WHERE UserId = @userId
                )
            ", new {userId});
        }

        public async Task<Group> GetGroupById(Guid id)
        {
            return await _connection.QuerySingleOrDefaultAsync<Group>(@"
                SELECT * 
                FROM Groups
                WHERE Id = @id
            ", new {id});
        }

        public async Task<Group> GetUserGroupById(Guid userId, Guid groupId)
        {
            return await _connection.QuerySingleOrDefaultAsync<Group>(@"
                SELECT *
                FROM Groups
                WHERE Id = @groupId AND Id IN (
                    SELECT GroupId FROM UserGroupRights
                    WHERE UserId = @userId
                )
            ", new {userId, groupId});
        }

        public async Task<Group> GetFullUserGroupById(Guid userId, Guid groupId)
        {
            var group = await _connection.QuerySingleOrDefaultAsync<Group>(@"
                SELECT *
                FROM Groups
                WHERE Id = @groupId AND Id IN (
                    SELECT GroupId FROM UserGroupRights
                    WHERE UserId = @userId
                )
            ", new {userId, groupId});

            if (group == null) return null;

            group.StorageGroups = (await _connection.QueryAsync<StorageGroup>(@"
                SELECT *
                FROM StorageGroups
                WHERE GroupId = @groupId
            ", new {groupId = group.Id})).ToList();
            
            group.TemplateGroups = (await _connection.QueryAsync<TemplateGroup>(@"
                SELECT *
                FROM TemplateGroups
                WHERE GroupId = @groupId
            ", new {groupId = group.Id})).ToList();

            group.UserGroupRights = (await _connection.QueryAsync<UserGroupRight, Right, UserGroupRight>(@"
                SELECT UserId, GroupId, 
                       Rights_CanReadStorages as CanReadStorages, Rights_CanModifyStorages as CanModifyStorages, 
                       Rights_CanModifyGroup as CanModifyGroup, Rights_CanReadTemplates as CanReadTemplates, 
                       Rights_CanModifyTemplates as CanModifyTemplates
                FROM UserGroupRights
                WHERE GroupId = @groupId
            ", (userRight, rights) =>
            {
                userRight.Rights = rights;
                return userRight;
            }, new {groupId}, splitOn: "CanReadStorages")).ToList();

            return group;
        }

        public async Task<Group> GetFullGroupById(Guid groupId)
        {
            var group = await _connection.QuerySingleOrDefaultAsync<Group>(@"
                SELECT *
                FROM Groups
                WHERE Id = @groupId
            ", new { groupId});

            if (group == null) return null;

            group.StorageGroups = (await _connection.QueryAsync<StorageGroup>(@"
                SELECT *
                FROM StorageGroups
                WHERE GroupId = @groupId
            ", new {groupId = group.Id})).ToList();
            
            group.TemplateGroups = (await _connection.QueryAsync<TemplateGroup>(@"
                SELECT *
                FROM TemplateGroups
                WHERE GroupId = @groupId
            ", new {groupId = group.Id})).ToList();

            group.UserGroupRights = (await _connection.QueryAsync<UserGroupRight>(@"
                SELECT *
                FROM UserGroupRights
                WHERE GroupId = @groupId
            ", new {groupId = group.Id})).ToList();

            return group;
        }

        public async Task<Group> GetModerUserGroupById(Guid userId, Guid groupId)
        {
            return await _connection.QuerySingleOrDefaultAsync<Group>(@"
                SELECT *
                FROM Groups
                WHERE Id = @groupId AND Id IN (
                    SELECT GroupId FROM UserGroupRights
                    WHERE UserId = @userId
                    AND Rights_CanModifyGroup = 1
                )
            ", new {userId, groupId});
        }

        public async Task DeleteGroupById(Guid id)
        {
            await _connection.QueryAsync(@"
                DELETE FROM Groups
                WHERE Id = @id
            ", new {id});
        }

        public async Task UpdateGroup(Group group)
        {
            await _connection.QueryAsync(@"
                UPDATE Groups
                SET Name = @Name
                WHERE Id = @Id
            ", group);
        }

        public async Task InsertGroup(Group group)
        {
            await _connection.QueryAsync(@"
                INSERT INTO Groups(Id, Name) 
                VALUES (@Id, @Name)
            ", group);
        }

        public async Task<int> InsertStoragesToGroupById(Guid groupId, List<Guid> storageIds)
        {
            
            var toInsert = new List<dynamic>();
            storageIds.ForEach(storageId => { toInsert.Add(new {storageId, groupId}); });
            
            return await _connection.ExecuteAsync(@"
                INSERT INTO StorageGroups(StorageId, GroupId)
                VALUES (@storageId, @groupId)
                ON CONFLICT DO NOTHING 
            ", toInsert);
        }

        public async Task<int> DeleteStoragesFromGroupById(Guid groupId, List<Guid> storagesIds)
        {
            var toDelete = new List<dynamic>();
            storagesIds.ForEach(storageId => { toDelete.Add(new {groupId, storageId}); });

            return await _connection.ExecuteAsync(@"
                DELETE FROM StorageGroups
                WHERE GroupId = @groupId AND StorageId = @storageId
            ", toDelete);
        }

        public async Task<int> InsertTemplatesToGroupById(Guid groupId, List<Guid> templateIds)
        {
            var toInsert = new List<dynamic>();
            templateIds.ForEach(templateId => { toInsert.Add(new {templateId, groupId}); });

            return await _connection.ExecuteAsync(@"
                INSERT INTO TemplateGroups(TemplateId, GroupId)
                VALUES (@templateId, @groupId)
                ON CONFLICT DO NOTHING 
            ", toInsert);
        }
        
        public async Task<int> DeleteTemplatesFromGroupById(Guid groupId, List<Guid> templateIds)
        {
            var toDelete = new List<dynamic>();
            templateIds.ForEach(templateId => { toDelete.Add(new {groupId, templateId}); });

            return await _connection.ExecuteAsync(@"
                DELETE FROM TemplateGroups
                WHERE GroupId = @groupId AND TemplateId = @templateId
            ", toDelete);
        }

        public async Task<int> InsertUsersToGroupById(Guid groupId, List<UserRight> userRights)
        {
            var toInsert = new List<object>();
            userRights.ForEach(userRight => { toInsert.Add(new
            {
                userId = userRight.Id,
                groupId,
                canReadStorages = userRight.Rights.CanReadStorages,
                canModifyStorages = userRight.Rights.CanModifyStorages,
                canModifyGroup = userRight.Rights.CanModifyGroup,
                canReadTemplates = userRight.Rights.CanReadTemplates,
                canModifyTemplates = userRight.Rights.CanModifyTemplates
            }); });
            
            return await _connection.ExecuteAsync(@"
                INSERT INTO UserGroupRights(UserId, GroupId, Rights_CanReadStorages, Rights_CanModifyStorages, Rights_CanModifyGroup, Rights_CanReadTemplates, Rights_CanModifyTemplates)
                VALUES (@userId, @groupId, @canReadStorages, @canModifyStorages, @canModifyGroup, @canReadTemplates, @canModifyTemplates)
            ", toInsert);
        }

        public async Task<int> InsertUsersToGroup(UserGroupRight userGroupRight)
        {
            var userRight = new UserRight
            {
                Id = userGroupRight.UserId,
                Rights = userGroupRight.Rights
            };

            return await InsertUsersToGroupById(userGroupRight.GroupId, new List<UserRight> { userRight });
        }

        public async Task<int> DeleteUsersFromGroupById(Guid groupId, List<Guid> userIds)
        {
            var toDelete = new List<dynamic>();
            userIds.ForEach(userId => { toDelete.Add(new {groupId, userId}); });

            return await _connection.ExecuteAsync(@"
                DELETE FROM UserGroupRights
                WHERE GroupId = @groupId AND UserId = @userId
            ", toDelete);
        }
    }
}