using System.Collections.Generic;

namespace migratedata
{
    public partial class CannedReplyAnswer
    {
        public CannedReplyAnswer(CannedReply cannedReply)
        {
            //this.Id = cannedReply.Id;
            this.MerchantId = cannedReply.MerchantId;
            this.UserId = cannedReply.UserId;
            this.IsAutoReplyEnabled = cannedReply.IsAutoReplyEnabled;
            this.CategoryName = cannedReply.CategoryName;
            this.ReplyContent = cannedReply.ReplyContent;
            this.CreationTime = cannedReply.CreationTime;
            this.CannedReplyQuestions = new HashSet<CannedReplyQuestion>();
        }
    }
}