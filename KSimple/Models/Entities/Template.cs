using System;
using System.Collections.Generic;
using KSimple.Models.Misc;

namespace KSimple.Models.Entities
{
    public class Template
    {
        public Guid Id { get; set; }
        public string UserDefinedId { get; set; }
        public string Name { get; set; }
        
        public string ModelTree { get; set; }
        
        public List<TemplateGroup> TemplateGroups { get; set; }
        public List<Storage> Storages { get; set; }
        
        public string Status { get; set; }
    }
}