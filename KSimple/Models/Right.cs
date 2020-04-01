using Microsoft.EntityFrameworkCore;

namespace KSimple.Models
{
    [Owned]
    public class Right
    {
        public bool CanReadStorages { get; set; }
        public bool CanModifyStorages { get; set; }
    }
}