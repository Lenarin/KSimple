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
            return TypeList[dataType](value);
        }
        
        public static Dictionary<string, Func<JToken, bool>> TypeList = 
            new Dictionary<string, Func<JToken, bool>>
            {
                {"Number", elem =>  elem.Type == JTokenType.Float || elem.Type == JTokenType.Integer},
                {"Boolean", elem => elem.Type == JTokenType.Boolean},
                {"String", elem => elem.Type == JTokenType.String},
                {"Object", elem => elem.Type == JTokenType.Object},
                {"Number[]", elem => elem.Type == JTokenType.Array && 
                                     elem.ToObject<List<JToken>>()
                                         .All(val => val.Type == JTokenType.Float || val.Type == JTokenType.Integer)},
                {"Boolean[]", elem => elem.Type == JTokenType.Array && 
                                      elem.ToObject<List<JToken>>()
                                          .All(val => val.Type == JTokenType.Boolean)},
                {"String[]", elem => elem.Type == JTokenType.Array && 
                                     elem.ToObject<List<JToken>>()
                                         .All(val => val.Type == JTokenType.String)}
            };
        
    }
}