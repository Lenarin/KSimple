using System.Collections.Generic;
using System.Data;
using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KSimple.Models.MySqlTypeHandlers
{
    public class MySqlJsonDictTypeHandler : SqlMapper.TypeHandler<Dictionary<string, JToken>>
    {
        public override void SetValue(IDbDataParameter parameter, Dictionary<string, JToken> value)
        {
            parameter.Value = JsonConvert.SerializeObject(value, Formatting.Indented);
        }

        public override Dictionary<string, JToken> Parse(object value)
        {
            return JToken.Parse((string) value).ToObject<Dictionary<string, JToken>>();
        }
    }
}