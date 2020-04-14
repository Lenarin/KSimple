using System;
using System.Collections.Generic;
using System.Data.Common;
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

        public async Task<Template> GetTemplateById(Guid id)
        {
            return await _connection.QueryFirstOrDefaultAsync<Template>(@"
                SELECT * FROM Templates
                WHERE Id = @Id
            ", new {Id = id});
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