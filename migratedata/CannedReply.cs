using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace migratedata
{
    public class CannedReply
    {
        [Key]
        public long Id { get; set; }
        public long MerchantId { get; set; }
        public long? UserId { get; set; }
        public string CategoryName { get; set; }
        public string ReplyContent { get; set; }
        public bool IsAutoReplyEnabled { get; set; }
        public DateTimeOffset CreationTime { get; set; }
        public IEnumerable<string> Questions { get; set; }

        public CannedReply()
        {
            
        }
        public CannedReply(CannedReplyAnswer cra)
        {
            Id = cra.Id;
            MerchantId = cra.MerchantId;
            UserId = cra.UserId;
            CategoryName = cra.CategoryName;
            ReplyContent = cra.ReplyContent;
            IsAutoReplyEnabled = cra.IsAutoReplyEnabled;
            CreationTime = cra.CreationTime;
            Questions = cra.CannedReplyQuestions?.Select(crq => crq.Question) ?? Enumerable.Empty<string>();
        }

        public static Expression<Func<CannedReplyAnswer, CannedReply>> TransCraToCrLambda = cra => new CannedReply
            {
                Id = cra.Id,
                MerchantId = cra.MerchantId,
                UserId = cra.UserId,
                CategoryName = cra.CategoryName,
                ReplyContent = cra.ReplyContent,
                IsAutoReplyEnabled = cra.IsAutoReplyEnabled,
                CreationTime = cra.CreationTime,
                Questions = cra.CannedReplyQuestions.Select(crq => crq.Question)//CannedReplyQuestions should not be null, this is used for CannedReply selected from DB
            };

        public static string GetMd5Hash(string input)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.Unicode.GetBytes(input);

            bs = x.ComputeHash(bs);

            System.Text.StringBuilder s = new System.Text.StringBuilder();

            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            return s.ToString();
        }
        public class CannedReplyBulkResult
        {
            public int Index { get; set; }
            public string Status { get; set; }
            public long Id { get; set; }
            public string ErrorMessage { get; set; }
            public IEnumerable<long> QuestionIds { get; set; }
        }
        public class CannedReplyAnswerInternalResult
        {
            public CannedReplyAnswer CannedReplyAnswer;
            public string Status;
            public string ErrorMessage;
            public IEnumerable<long> QuestionIds;
        }
    }
}