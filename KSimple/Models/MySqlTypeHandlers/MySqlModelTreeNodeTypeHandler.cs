using System.Data;
using Dapper;
using KSimple.Models.Misc;
using Newtonsoft.Json;

namespace KSimple.Models.MySqlTypeHandlers
{
    public class MySqlModelTreeNodeTypeHandler : SqlMapper.TypeHandler<ModelTreeNode>
    {
        public override void SetValue(IDbDataParameter parameter, ModelTreeNode value)
        {
            parameter.Value = JsonConvert.SerializeObject(value, Formatting.Indented);
        }

        public override ModelTreeNode Parse(object value)
        {
            return JsonConvert.DeserializeObject<ModelTreeNode>((string) value);
        }
    }
}