using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using KSimple.Models.Misc;
using Newtonsoft.Json.Linq;

namespace KSimple.Models.Entities
{
    public class Storage
    {
        public Guid Id { get; set; }
        [Required]
        [MaxLength(32)]
        public string UserDefinedId { get; set; }
        [Required]
        [MaxLength(32)]
        public string Name { get; set; }
        [Required]
        public Dictionary<string, StorageField> StorageFields { get; set; }
        [Required]
        public string Status { get; set; }
        
        public Guid? TemplateId { get; set; }
        public Template Template { get; set; }

        public List<StorageGroup> StorageGroups { get; set; }
        
        public List<Packet> Packets { get; set; }

        /// <summary>
        /// Parse incoming data: check types, delete wrong keys and set default values to not provided keys.
        /// </summary>
        /// <param name="data">Data to parse</param>
        /// <param name="defaultData">Default values. If null set default values from storage</param>
        /// <returns>Parsed dict</returns>
        /// <exception cref="Exception">Hello there!</exception>
        public Dictionary<string, JToken> ParseData(Dictionary<string, JToken> data, Dictionary<string, JToken> defaultData = null)
        {
            var res = new Dictionary<string, JToken>();
            foreach (var (key, value) in StorageFields)
            {
                try
                {
                    if (!StorageField.Check(value.DataType, data[key])) throw new Exception($"{key} value type not correct");
                    res[key] = data[key];
                }
                catch (KeyNotFoundException)
                {
                    if (defaultData == null)
                    {
                        res[key] = value.InitValue;
                    }
                    else
                    {
                        res[key] = defaultData[key];
                    }
                }
            }

            return res;
        }
    }
}