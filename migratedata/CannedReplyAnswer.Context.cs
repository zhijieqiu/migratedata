﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace migratedata
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class cssmanagementdbEntities : DbContext
    {
        public cssmanagementdbEntities()
            : base("name=cssmanagementdbEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<CannedReplyAnswer> CannedReplyAnswers { get; set; }
        public virtual DbSet<CannedReplyQuestion> CannedReplyQuestions { get; set; }
        public virtual DbSet<Merchant> Merchants { get; set; }
        public virtual DbSet<AutoReplyKeyword> AutoReplyKeywords { get; set; }
        public virtual DbSet<AutoReplyMessage> AutoReplyMessages { get; set; }
        public virtual DbSet<AutoReplyRule> AutoReplyRules { get; set; }
        public virtual DbSet<AutoReplyRegex> AutoReplyRegexes { get; set; }
    }
}
