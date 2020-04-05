using Microsoft.EntityFrameworkCore;

namespace KSimple.Models.Misc
{
    [Owned]
    public class Right
    {
        public bool CanReadStorages { get; set; }
        public bool CanModifyStorages { get; set; }
    }
}