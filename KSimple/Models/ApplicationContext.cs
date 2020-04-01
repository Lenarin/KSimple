using Microsoft.EntityFrameworkCore;

namespace KSimple.Models
{
    public sealed class ApplicationContext : DbContext
    {
        public DbSet<Template> Templates { get; set; }
        public DbSet<Storage> Storages { get; set; }
        public DbSet<Packet> Packets { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<User> Users { get; set; }
        
        public DbSet<UserGroupRight> UserGroupRights { get; set; }
        public DbSet<StorageGroup> StorageGroups { get; set; }
        public DbSet<TemplateGroup> TemplateGroups { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            // Database.EnsureDeleted();
            // Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserGroupRight>()
                .HasKey(t => new {t.GroupId, t.UserId});

            modelBuilder.Entity<StorageGroup>()
                .HasKey(t => new {t.GroupId, t.StorageId});

            modelBuilder.Entity<TemplateGroup>()
                .HasKey(t => new {t.GroupId, t.TemplateId});
            
            base.OnModelCreating(modelBuilder);
        }
        
    }
    
}