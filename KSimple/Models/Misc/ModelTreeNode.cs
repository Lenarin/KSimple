using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using KSimple.Models.Entities;
using Newtonsoft.Json.Linq;

namespace KSimple.Models.Misc
{
    public class ModelTreeNode
    {
        [Required]
        public string Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Status { get; set; }
        
        [Required]
        public string Type { get; set; }
        
        [Required]
        public List<ModelTreeNode> Children { get; set; }
        
        public string ValueType { get; set; }
        
        public JToken InitValue { get; set; }
        
        
        /// <summary>
        /// Recursively validate model tree by given rules:
        /// - All ids are unique
        /// - Only acceptable types
        /// - Only acceptable value types
        /// - Only acceptable init values
        /// - Only acceptable statuses
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool Validate(HashSet<string> ids = null)
        {
            if (ids == null) ids = new HashSet<string>();
            
            if (ids.Contains(Id)) throw new Exception($"{Id} not unique");
            
            if (!_types.Contains(Type)) throw new Exception($"Unknown type {Type} on {Id}");
            
            if (Type == "value" && !StorageField.TypeList.ContainsKey(ValueType.ToLower())) 
                throw new Exception($"Unknown value type {ValueType} on {Id}");
            
            if (Type == "value" && !StorageField.Check(ValueType, InitValue)) 
                throw new Exception($"Bad init value on {Id}");

            ids.Add(Id);

            return Children.Count == 0 || Children.All(node => node.Validate(ids));
        }

        private static List<string> _types = new List<string>
        {
            "folder",
            "value",
            "action"
        };

        /// <summary>
        /// Recursively generate storage fields dictionary from model tree. 
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public Dictionary<string, StorageField> ToStorageFields(Dictionary<string, StorageField> dict = null)
        {
            if (Id == "root")
            {
                if (dict != null) throw new Exception("On root node dict must be null. Do not provide it outside of function.");
                dict = new Dictionary<string, StorageField>();
            }
            if (Type == "value")
            {
                dict.Add(Name, new StorageField()
                {
                    DataType = ValueType,
                    InitValue = InitValue
                });
            }

            foreach (var node in Children)
            {
                node.ToStorageFields(dict);
            }

            return dict;
        }
    }
}