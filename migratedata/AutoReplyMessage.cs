//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class AutoReplyMessage
    {
        public long Id { get; set; }
        public long MerchantId { get; set; }
        public long RuleId { get; set; }
        public string MsgType { get; set; }
        public string MsgJson { get; set; }
    
        public virtual AutoReplyRule AutoReplyRule { get; set; }
        public virtual Merchant Merchant { get; set; }
    }
}