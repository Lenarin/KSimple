using System;
using System.Collections.Generic;
using KSimple.Models.Entities;
using KSimple.Models.Misc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonConverter = System.Text.Json.Serialization.JsonConverter;
using JsonSerializer = System.Text.Json.JsonSerializer;

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

            modelBuilder.Entity<Storage>()
                .HasAlternateKey(s => s.UserDefinedId);

            modelBuilder.Entity<Template>()
                .HasAlternateKey(t => t.UserDefinedId);

            modelBuilder.Entity<User>()
                .HasAlternateKey(u => u.Name);

            
            modelBuilder.Entity<Storage>()
                .Property(s => s.StorageFields)
                .HasConversion(
                    d => JsonConvert.SerializeObject(d, Formatting.None),
                    s => JsonConvert.DeserializeObject<Dictionary<string, StorageField>>(s));
            
            modelBuilder.Entity<Packet>()
                .Property(p => p.Data)
                .HasConversion(
                    d => JsonConvert.SerializeObject(d, Formatting.None),
                    s => JsonConvert.DeserializeObject<Dictionary<string, JToken>>(s));
                    

            base.OnModelCreating(modelBuilder);
        }

        public void NotNullUpdate<T>(T obj)
        {
            foreach (var prop in obj.GetType().GetProperties())
            {
                if (prop.GetValue(obj) == null) continue;
                if (!this.Entry(obj).Property(prop.Name).Metadata.IsKey())
                {
                    this.Entry(obj).Property(prop.Name).IsModified = true;
                }
            }
        }
        
    }
    
}