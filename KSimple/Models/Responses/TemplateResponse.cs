using System;
using System.Collections.Generic;
using KSimple.Models.Entities;
using KSimple.Models.Misc;

namespace KSimple.Models.Responses
{
    public class TemplateResponse
    {
        public Guid Id { get; set; }
        
        public string UserDefinedId { get; set; }
        
        public string Name { get; set; }
        
        public ModelTreeNode ModelTree { get; set; }
        
        public List<TemplateGroup> TemplateGroups { get; set; }
        public List<Storage> Storages { get; set; }
        
        public string Status { get; set; }
        
        public bool CanModifyTemplate { get; set; }

        public TemplateResponse(Template template, bool canModifyTemplate = false)
        {
            Id = template.Id;
            UserDefinedId = template.UserDefinedId;
            Name = template.Name;
            ModelTree = template.ModelTree;
            TemplateGroups = template.TemplateGroups;
            Storages = template.Storages;
            Status = template.Status;
            CanModifyTemplate = canModifyTemplate;
        }
    }
}