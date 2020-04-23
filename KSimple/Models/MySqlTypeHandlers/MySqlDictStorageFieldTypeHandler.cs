using System.Collections.Generic;
using System.Data;
using Dapper;
using KSimple.Models.Misc;
using Newtonsoft.Json;

namespace KSimple.Models.MySqlTypeHandlers
{
    public class MySqlDictStorageFieldTypeHandler : SqlMapper.TypeHandler<Dictionary<string, StorageField>>
    {
        public override void SetValue(IDbDataParameter parameter, Dictionary<string, StorageField> value)
        {
            parameter.Value = JsonConvert.SerializeObject(value, Formatting.Indented);
        }

        public override Dictionary<string, StorageField> Parse(object value)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, StorageField>>((string) value);
        }
    }
}