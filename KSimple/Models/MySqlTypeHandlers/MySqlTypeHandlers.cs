using Dapper;

namespace KSimple.Models.MySqlTypeHandlers
{
    public class MySqlTypeHandlers
    {
        public static void AddMySqlTypeHandlers()
        {
            SqlMapper.AddTypeHandler(new MySqlGuidTypeHandler());
            SqlMapper.AddTypeHandler(new MySqlJsonDictTypeHandler());
            SqlMapper.AddTypeHandler(new MySqlDictStorageFieldTypeHandler());
            SqlMapper.AddTypeHandler(new MySqlModelTreeNodeTypeHandler());
        }
        
    }
}