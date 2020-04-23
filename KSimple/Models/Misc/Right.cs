using Microsoft.EntityFrameworkCore;

namespace KSimple.Models.Misc
{
    [Owned]
    public class Right
    {
        public bool CanReadStorages { get; set; }
        public bool CanModifyStorages { get; set; }
        
        public bool CanModifyGroup { get; set; }
        
        public bool CanReadTemplates { get; set; }
        public bool CanModifyTemplates { get; set; }

        public static Right GetAdminRights()
        {
            return new Right()
            {
                CanModifyGroup = true,
                CanModifyStorages = true,
                CanModifyTemplates = true,
                CanReadStorages = true,
                CanReadTemplates = true
            };
        }

        public static Right GetUserRights()
        {
            return new Right()
            {
                CanModifyGroup = false,
                CanModifyStorages = false,
                CanModifyTemplates = false,
                CanReadStorages = true,
                CanReadTemplates = true
            };
        }
    }
}