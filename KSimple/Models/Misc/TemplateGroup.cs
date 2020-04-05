using System;
using KSimple.Models.Entities;

namespace KSimple.Models.Misc
{
    public class TemplateGroup
    {
        public Guid TemplateId { get; set; }
        public Template Template { get; set; }
        
        public Guid GroupId { get; set; }
        public Group Group { get; set; }
    }
}