using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KSimple.Models.Misc;

namespace KSimple.Models.Entities
{
    public class Template
    {
        public Guid Id { get; set; }
        [MaxLength(32)]
        public string UserDefinedId { get; set; }
        [MaxLength(32)]
        public string Name { get; set; }
        
        public ModelTreeNode ModelTree { get; set; }
        
        public List<TemplateGroup> TemplateGroups { get; set; }
        public List<Storage> Storages { get; set; }
        
        public string Status { get; set; }
        
        /// <summary>
        /// Merge provided template, set not null (except Id) fields and mutate object
        /// </summary>
        /// <param name="template">Template to merge</param>
        public void Merge(Template template)
        {
            if (template.UserDefinedId != null) this.UserDefinedId = template.UserDefinedId;
            if (template.Name != null) this.Name = template.Name;
            if (template.ModelTree != null) this.ModelTree = template.ModelTree;
        }

        public bool Validate()
        {
            if (UserDefinedId == null) throw new Exception("User defined id must be set");
            if (Name == null) throw new Exception("Name must be set");
            if (ModelTree.Id != "root") throw new Exception("Top node of tree must have id - 'root'");
            return ModelTree.Validate();
        }
    }
}