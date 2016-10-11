namespace migratedata
{
    public partial class CannedReplyQuestion
    {
        public CannedReplyQuestion (CannedReplyAnswer cannedReplyAnswer, string question)
        {
            this.Question = question;
            this.MerchantId = cannedReplyAnswer.MerchantId;
            this.UserId = cannedReplyAnswer.UserId;
            this.CannedReplyAnswerId = cannedReplyAnswer.Id;
            this.CreationTime = cannedReplyAnswer.CreationTime;
        }

        public CannedReplyQuestion()
        {
            
        }
    }
}