using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using KSimple.Models.Entities;
using KSimple.Models.Misc;
using Microsoft.Data.Sqlite;

namespace KSimple.Models.Repositories
{
    public class TemplateRepository
    {
        private readonly DbConnection _connection;

        public TemplateRepository(DbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<Template>> GetAllTemplates()
        {
            return await _connection.QueryAsync<Template>(@"
                SELECT * FROM Templates
            ");
        }

        public async Task<IEnumerable<(Template, bool)>> GetAllUserTemplates(Guid userId)
        {
            return await _connection.QueryAsync<Template, long, (Template, bool)>(@"
                SELECT T.Id, T.UserDefinedId, T.Name, T.ModelTree, T.Status, Rights_CanModifyTemplates CanModifyTemplates FROM (
                    SELECT GroupId, Rights_CanModifyTemplates FROM UserGroupRights
                    WHERE UserId = @userId AND Rights_CanReadTemplates = 1) t1
                JOIN TemplateGroups TG ON t1.GroupId = TG.GroupId
                JOIN Templates T ON TG.TemplateId = T.Id
            ", (template, b) => (template, b != 0), new {userId}, splitOn: "CanModifyTemplates" );
        }

        public async Task<Template> GetTemplateById(Guid id)
        {
            return await _connection.QueryFirstOrDefaultAsync<Template>(@"
                SELECT * FROM Templates
                WHERE Id = @Id
            ", new {Id = id});
        }

        public async Task<(Template, bool)> GetUserTemplateById(Guid userId, Guid templateId)
        {
            return (await _connection.QueryAsync<Template, long, (Template, bool)>(@"
                SELECT T.Id, T.UserDefinedId, T.Name, T.ModelTree, T.Status, Rights_CanModifyTemplates CanModifyTemplates FROM (
                    SELECT GroupId, Rights_CanModifyTemplates FROM UserGroupRights
                    WHERE UserId = @userId AND Rights_CanReadTemplates = 1) t1
                JOIN TemplateGroups TG ON t1.GroupId = TG.GroupId
                JOIN Templates T ON TG.TemplateId = T.Id
                WHERE TemplateId = @templateId
            ", (template, b) => (template, b != 0), new {userId}, splitOn: "CanModifyTemplates")).ToArray()[0];
        }

        public async Task AddNewTemplate(Template template)
        {
            await _connection.QueryAsync(@"
                INSERT INTO Templates(Id, UserDefinedId, Name, ModelTree, Status)
                VALUES (@Id, @UserDefinedId, @Name, @ModelTree, @Status);
            ", template);
        }

        public async Task UpdateTemplate(Template template)
        {
            await _connection.QueryAsync(@"
                UPDATE Templates
                SET Name = @Name,
                ModelTree = @ModelTree,
                UserDefinedId = @UserDefinedId,
                Status = @Status
                WHERE Id = @Id
            ", template);
        }

        public async Task DeleteTemplateById(Guid Id)
        {
            await _connection.QueryAsync(@"
                DELETE FROM Templates
                WHERE Id = @Id
            ", new {Id});
        }

        public async Task<ModelTreeNode> GetModelTree(Guid Id)
        {
            return await _connection.QueryFirstOrDefaultAsync<ModelTreeNode>(@"
                SELECT ModelTree FROM Templates
                WHERE Id = @Id
            ", new {Id});
        }

        public async Task<(ModelTreeNode, bool)> GetUserModelTree(Guid userId, Guid templateId)
        {
            return (await _connection.QueryAsync<ModelTreeNode, long, (ModelTreeNode, bool)>(@"
                SELECT T.ModelTree, Rights_CanModifyTemplates CanModifyTemplates FROM (
                    SELECT GroupId, Rights_CanModifyTemplates FROM UserGroupRights
                    WHERE UserId = @userId AND Rights_CanReadTemplates = 1) t1
                    JOIN TemplateGroups TG ON t1.GroupId = TG.GroupId
                    JOIN Templates T ON TG.TemplateId = T.Id
                WHERE TemplateId = @templateId
            ", (node, b) => (node, b != 0), new {userId}, splitOn: "CanModifyTemplates")).ToArray()[0];
        }

        public async Task SetModelTree(Guid Id, ModelTreeNode ModelTree)
        {
            await _connection.QueryAsync(@"
                UPDATE Templates
                SET ModelTree = @ModelTree
                WHERE Id = @Id
            ", new {Id, ModelTree});
        }
    }
}