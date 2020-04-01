using System;
using System.Collections.Generic;

namespace KSimple.Models
{
    public class Template
    {
        public Guid Id { get; set; }
        public string UserDefinedId { get; set; }
        public string Name { get; set; }
        
        public string ModelTree { get; set; }
        
        public List<TemplateGroup> TemplateGroups { get; set; }
        public List<Storage> Storages { get; set; }
        
        public Status Status { get; set; }
    }
}