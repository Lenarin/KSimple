using System.Data;
using Dapper;

namespace KSimple.Models.MySqlTypeHandlers
{
    public class MySqlBoolTypeHandler : SqlMapper.TypeHandler<bool>
    {
        public override void SetValue(IDbDataParameter parameter, bool value)
        {
            parameter.Value = value ? 1 : 0;
        }

        public override bool Parse(object value)
        {
            return (long) value == 1;
        }
    }
}