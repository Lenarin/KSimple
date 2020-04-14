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
            if (Type == "value" && !StorageField.TypeList.ContainsKey(ValueType)) throw new Exception($"Unknown value type {ValueType} on {Id}");
            if (!StorageField.Check(ValueType, InitValue)) throw new Exception($"Bad init value on {Id}");

            ids.Add(Id);

            return Children.Count == 0 || Children.All(node => node.Validate(ids));
        }

        private static List<string> _types = new List<string>
        {
            "subsystem",
            "value",
            "action"
        };
    }
}