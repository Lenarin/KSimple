using System.Collections.Generic;

namespace KSimple.Models
{
    public interface ITemplateRepository
    {
        void Create(Template template);
        void Delete(string id);
        Template Get(string id);
        List<Template> GetTemplates();
        void Update(Template template);
    }
    
    public class TemplateRepository : ITemplateRepository
    {
        public TemplateRepository()
        {
            
        }
        
        public void Create(Template template)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(string id)
        {
            throw new System.NotImplementedException();
        }

        public Template Get(string id)
        {
            throw new System.NotImplementedException();
        }

        public List<Template> GetTemplates()
        {
            throw new System.NotImplementedException();
        }

        public void Update(Template template)
        {
            throw new System.NotImplementedException();
        }
    }
}