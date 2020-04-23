﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Options;
 using Newtonsoft.Json;
 using Newtonsoft.Json.Linq;

namespace KSimple.Models.Misc
{
    public class StorageField
    {
        public string DataType { get; set; }
        public JToken InitValue { get; set; }
        
        
        public static bool Check(string dataType, JToken value)
        {
            return TypeList[dataType.ToLower()](value);
        }
        
        public static Dictionary<string, Func<JToken, bool>> TypeList = 
            new Dictionary<string, Func<JToken, bool>>
            {
                {"number", elem =>  elem.Type == JTokenType.Float || elem.Type == JTokenType.Integer},
                {"boolean", elem => elem.Type == JTokenType.Boolean},
                {"string", elem => elem.Type == JTokenType.String},
                {"object", elem => elem.Type == JTokenType.Object},
                {"number[]", elem => elem.Type == JTokenType.Array && 
                                     elem.ToObject<List<JToken>>()
                                         .All(val => val.Type == JTokenType.Float || val.Type == JTokenType.Integer)},
                {"boolean[]", elem => elem.Type == JTokenType.Array && 
                                      elem.ToObject<List<JToken>>()
                                          .All(val => val.Type == JTokenType.Boolean)},
                {"string[]", elem => elem.Type == JTokenType.Array && 
                                     elem.ToObject<List<JToken>>()
                                         .All(val => val.Type == JTokenType.String)}
            };
        
    }
}