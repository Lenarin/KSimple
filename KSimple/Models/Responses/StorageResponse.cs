﻿using System;
 using System.Collections.Generic;
 using System.ComponentModel.DataAnnotations.Schema;
 using KSimple.Models.Entities;
 using KSimple.Models.Misc;

 namespace KSimple.Models.Responses
{
    [NotMapped]
    public class StorageResponse
    {
        public Guid Id { get; set; }
        
        public string UserDefinedId { get; set; }
        
        public string Name { get; set; }
        
        public Dictionary<string, StorageField> StorageFields { get; set; }

        public string Status { get; set; }
        
        public Guid? TemplateId { get; set; }

        public List<StorageGroup> StorageGroups { get; set; }
        
        public bool UserCanModifyStorage { get; set; }

        public StorageResponse(Storage storage, bool userCanModifyStorage = false)
        {
            Id = storage.Id;
            UserDefinedId = storage.UserDefinedId;
            Name = storage.Name;
            StorageFields = storage.StorageFields;
            Status = storage.Status;
            TemplateId = storage.TemplateId;
            StorageGroups = storage.StorageGroups;
            UserCanModifyStorage = userCanModifyStorage;
        }
    }
}