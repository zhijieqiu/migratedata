using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Practices.ObjectBuilder2;
using System.IO;

namespace migratedata
{
    class AddCannedReply
    {
        public cssmanagementdbEntities Db = new cssmanagementdbEntities();
        private Exception GetInnerException(Exception e)
        {
            while (e.InnerException != null)
            {
                e = e.InnerException;
            }
            return e;
        }

        public AddCannedReply()
        {
            InitializeDbContext();
        }

        public void InitializeDbContext()
        {
            try
            {
                if (Db.Database.Connection.State != ConnectionState.Open)
                {
                    Db.Database.Connection.Open();
                    
                }
            }
            catch (Exception)
            {
                // ignore open twice
            }
            using (var cmd = Db.Database.Connection.CreateCommand())
            {
                cmd.CommandText = "exec sp_set_session_context @key, @value";
                cmd.Parameters.Add(new SqlParameter("key", SqlDbType.NVarChar) { Value = "Role" });
                cmd.Parameters.Add(new SqlParameter("value", SqlDbType.NVarChar) { Value = "root" });

                cmd.ExecuteNonQuery();
            }
            
        }
        /*
         * Db.AutoReplyMessages.RemoveRange(autoReplyRule.AutoReplyMessages);
            Db.AutoReplyKeywords.RemoveRange(autoReplyRule.AutoReplyKeywords);
            Db.AutoReplyRules.Remove(autoReplyRule);
         */
        public async Task<int> DeleteAutoReplyRule3(int ruleType = 3)
        {
            Console.WriteLine(Db.AutoReplyRules.Count());
            foreach (var t in Db.AutoReplyRules)
            {
                Console.WriteLine(t.RuleType);
            }
            var deletingRules = Db.AutoReplyRules.Where(rule => rule.RuleType == ruleType).ToList();
            
            foreach (var rule in deletingRules)
            {
                Db.AutoReplyKeywords.RemoveRange(rule.AutoReplyKeywords);
                Db.AutoReplyMessages.RemoveRange(rule.AutoReplyMessages);
                Db.AutoReplyRegexes.RemoveRange(rule.AutoReplyRegexes);
                Db.AutoReplyRules.Remove(rule);
            }
            int ret = await Db.SaveChangesAsync();
            return ret;
        }
        public async Task<int> AddAutoReplyRuleAndSave(string fileName)
        {
            
            string outFileName = @"D:\zhijie\regResult\regResult_out.txt";
            //var client = WebAppHelper.CreateRestClient();
            StreamReader sr = new StreamReader(fileName);
            StreamWriter sw = new StreamWriter(outFileName);
            string line = null;
            string merS = null;
            long merchantId = 0;
            string describ = null;
            string answer = null;
            const int RULE_TYPE = 3;
            //client.Login("pfizer_test_admin", "password", "Admin_UI_Client");
            AutoReplyRule ruleobj = new AutoReplyRule()
            {
                MerchantId = merchantId,
                RuleType = RULE_TYPE,
                Description = describ,
                AutoReplyMessages = new List<AutoReplyMessage>()
                        {
                            new AutoReplyMessage()
                            {
                                MerchantId = merchantId,
                                MsgJson = "" ,
                                MsgType = "text"
                            }
                        },
                AutoReplyKeywords = new List<AutoReplyKeyword>(),
                AutoReplyRegexes = new List<AutoReplyRegex>()
            };
            bool isFirst = true;
           // bool isGreater64 = false;
            int i = 0;
            while ((line = sr.ReadLine()) != null)
            {
                if (line == "BEGIN_NEW_RULE")
                {
                    if (!isFirst)
                    {
                        Db.AutoReplyRules.Add(ruleobj);
                        sw.WriteLine(ruleobj.ToString());
                        
                    }
                    isFirst = false;
                    merS = sr.ReadLine();
                    if (merS == null) break;
                    merchantId = long.Parse(merS);
                    describ = sr.ReadLine();
                    answer = sr.ReadLine();
                    ruleobj = new AutoReplyRule()
                    {
                        MerchantId = merchantId,
                        RuleType = RULE_TYPE,
                        Description = describ,
                        AutoReplyMessages = new List<AutoReplyMessage>()
                        {
                            new AutoReplyMessage()
                            {
                                MerchantId = merchantId,
                                MsgJson = answer ,
                                MsgType = "text"
                            }
                        },
                        AutoReplyKeywords = new List<AutoReplyKeyword>(),
                        AutoReplyRegexes = new List<AutoReplyRegex>()
                    };
                    continue;
                }
                string[] tokens = line.Split("\t".ToArray(), StringSplitOptions.None);
                ruleobj.AutoReplyRegexes.Add(new AutoReplyRegex()
                {
                    MerchantId = merchantId,
                    Regex = tokens[2]
                });
                Console.WriteLine(i++);
            }
            if (!isFirst)
            {
                Db.AutoReplyRules.Add(ruleobj);
                sw.WriteLine(ruleobj.ToString());
            }
            try
            {
                int rCount = await Db.SaveChangesAsync();
                return rCount;
            }
            catch (DbUpdateException due)
            {
                Console.WriteLine(due.Message);
            }
            finally
            {
                sr.Close();
                sw.Close();
            }
            return 0;
        }
        public async Task<int>  AddAutoReplyRuleAndSave(AutoReplyRule arr)
        {
            Db.AutoReplyRules.Add(arr);
            int t = 0;
            try
            {
                t = await Db.SaveChangesAsync();
            }
            catch (DbUpdateException due)
            {
                Console.WriteLine(due.InnerException.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine(t);
            return t;
        }
        private async Task<List<long>> AddQuestionsToAnswerAndNotSaveInDb(CannedReplyAnswer cannedReplyAnswer, HashSet<string> targetQuestions)
        {
            //these question ids in this lst will not be inserted
            List<long> specialQuestionIdLst = new List<long>();
            if (targetQuestions != null)
            {
                foreach (var question in targetQuestions)
                {
                    string csQuestion = CannedReply.GetMd5Hash(question);
                    var crqFromDbId = await Db.CannedReplyQuestions.Where(c => c.CS_Question == csQuestion
                                                                            && c.MerchantId == cannedReplyAnswer.MerchantId
                                                                            && c.UserId == cannedReplyAnswer.UserId)
                                                                 .Where(c => c.Question == question)
                                                                 .Select(c => c.Id)
                                                                 .FirstOrDefaultAsync();
                    if (crqFromDbId == 0)
                    {
                        Db.CannedReplyQuestions.Add(new CannedReplyQuestion(cannedReplyAnswer, question));
                    }
                    else
                    {
                        //Instead of update, we choose skip
                        /*
                        if (crqFromDb.CannedReplyAnswerId != cannedReplyAnswer.Id)
                        {
                            crqFromDb.CannedReplyAnswerId = cannedReplyAnswer.Id;
                        }
                        */
                        specialQuestionIdLst.Add(crqFromDbId);
                    }
                    //We don't need this line unless we choose update instead of skip
                    //await Db.SaveChangesAsync();
                }
            }
            return specialQuestionIdLst;
        }
        private async Task<List<long>> AttachQuestionsToAnswerAndSaveInDb(CannedReplyAnswer cannedReplyAnswer, IEnumerable<string> targetQuestions)
        {
            List<long> specialQuestionIdLst = new List<long>();

            if (targetQuestions != null)
            {
                var targetQuestionsHashSet = new HashSet<string>(targetQuestions);
                if (targetQuestionsHashSet.Any())
                {

                    specialQuestionIdLst = await AddQuestionsToAnswerAndNotSaveInDb(cannedReplyAnswer, targetQuestionsHashSet);
                    await Db.SaveChangesAsync();
                }
            }
            return specialQuestionIdLst;
        }
        private async Task<List<long>> ForceAttachWithNewQuestionsToAnswerAndNotSaveInDb(CannedReplyAnswer cannedReplyAnswer, IEnumerable<string> targetQuestions)
        {
            if (targetQuestions == null)
            {
                throw new Exception("Error: not set question parameter when trying to update questions");
            }
            Dictionary<string, CannedReplyQuestion> oriQuestionDict = new Dictionary<string, CannedReplyQuestion>();
            cannedReplyAnswer.CannedReplyQuestions.ForEach(crq =>
            {
                if (!oriQuestionDict.ContainsKey(crq.Question))
                {
                    oriQuestionDict.Add(crq.Question, crq);
                }
            });
            HashSet<string> oriQuestionSet = new HashSet<string>(cannedReplyAnswer.CannedReplyQuestions.Select(crq => crq.Question));
            HashSet<string> desQuestionSet = new HashSet<string>(targetQuestions);
            HashSet<string> innerSet = new HashSet<string>(oriQuestionSet);
            innerSet.IntersectWith(desQuestionSet);
            oriQuestionSet.ExceptWith(innerSet);
            desQuestionSet.ExceptWith(innerSet);
            foreach (var deleteQuestion in oriQuestionSet)
            {
                //this may throw error when save, because there may be multiple thread trying to update or delete
                Db.CannedReplyQuestions.Remove(oriQuestionDict[deleteQuestion]);
            }
            //these question ids in this lst will not be inserted
            List<long> specialQuestionIdLst = await AddQuestionsToAnswerAndNotSaveInDb(cannedReplyAnswer, desQuestionSet);
            if (specialQuestionIdLst.Count == desQuestionSet.Count && innerSet.Count == 0)
            {
                throw new Exception("Error: all questions have existing answer when trying to update this answer's questions");
            }
            //await Db.SaveChangesAsync();
            return specialQuestionIdLst;
        }

        private async Task<CannedReply.CannedReplyAnswerInternalResult> MergeCannedReplyToExistingAnswerInDb(CannedReply cannedReply, CannedReplyAnswer craFromDb)
        {
            craFromDb.IsAutoReplyEnabled = cannedReply.IsAutoReplyEnabled;
            craFromDb.CategoryName = cannedReply.CategoryName ?? craFromDb.CategoryName;
            //don't use this if we have changes on CannedReply's dependency, 
            //we may lose info in the CannedReply after it's dependency get changed by this operation.
            //Here I remove it just because it is no needed,
            //await Db.SaveChangesAsync();
            List<long> specialQuestionIdLst = await AttachQuestionsToAnswerAndSaveInDb(craFromDb, cannedReply.Questions);
            if (specialQuestionIdLst.Any())
            {
                return new CannedReply.CannedReplyAnswerInternalResult
                {
                    CannedReplyAnswer = craFromDb,
                    Status = "UpdatedWithSkip",
                    ErrorMessage = $"Merged Questions to Answer {craFromDb.Id}. Questions are skipped because this answer or other answers have these questions, questionIds: {string.Join(",", specialQuestionIdLst)}",
                    //Status = "UpdateWithForceUpdate",
                    //ErrorMessage = $"Questions are updated with this new answer, questionIds: {string.Join(",", specialQuestionIdLst)}",
                    QuestionIds = specialQuestionIdLst
                };
            }
            else
            {
                return new CannedReply.CannedReplyAnswerInternalResult
                {
                    CannedReplyAnswer = craFromDb,
                    Status = "Updated",
                    ErrorMessage = $"Merged Questions to Answer {craFromDb.Id}"
                };
            }
        }

        private async Task<CannedReply.CannedReplyAnswerInternalResult> MergeExistingQuestionsToExistingAnswerInDb(CannedReplyAnswer craFromDbOri, CannedReplyAnswer craFromDbWithSameAnswerDes)
        {
            craFromDbOri.CannedReplyQuestions.ForEach(crq => crq.CannedReplyAnswerId = craFromDbWithSameAnswerDes.Id);
            await Db.SaveChangesAsync();
            return new CannedReply.CannedReplyAnswerInternalResult
            {
                CannedReplyAnswer = craFromDbWithSameAnswerDes,
                Status = "Updated",
                ErrorMessage = $"Merged Questions of Answer {craFromDbOri.Id} to Answer {craFromDbWithSameAnswerDes.Id}"
            };
        }
        private async Task<CannedReply.CannedReplyAnswerInternalResult> CreateCannedReplyAnswerToDb(CannedReply cannedReply)
        {
            var curMerchantIdFromDb = await Db.Merchants.Select(m => m.Id).FirstOrDefaultAsync(m => m == cannedReply.MerchantId);
            if (curMerchantIdFromDb == 0)
            {
                return new CannedReply.CannedReplyAnswerInternalResult
                {
                    CannedReplyAnswer = null,
                    Status = "Error",
                    ErrorMessage = "MerchantId is not valid or There is no such MerchantId in DB."
                };
            }
            if (cannedReply.Questions == null || !cannedReply.Questions.Any())
            {
                return new CannedReply.CannedReplyAnswerInternalResult
                {
                    CannedReplyAnswer = null,
                    Status = "Error",
                    ErrorMessage = "No questions to Add when creat canned Reply."
                };
            }
            string csReplyContent = CannedReply.GetMd5Hash(cannedReply.ReplyContent);
            CannedReplyAnswer craFromDb = await Db.CannedReplyAnswers.Where(c => c.CS_ReplyContent == csReplyContent
                                                                            && c.MerchantId == cannedReply.MerchantId
                                                                            && c.UserId == cannedReply.UserId)
                                                                     .Include(c => c.CannedReplyQuestions)
                                                           .FirstOrDefaultAsync(c => c.ReplyContent == cannedReply.ReplyContent);
            if (craFromDb != null)
            {
                cannedReply.CategoryName = cannedReply.CategoryName ?? "";
                return await MergeCannedReplyToExistingAnswerInDb(cannedReply, craFromDb);
            }
            else
            {
                try
                {
                    CannedReplyAnswer cannedReplyAnswer = new CannedReplyAnswer(cannedReply);
                    Db.CannedReplyAnswers.Add(cannedReplyAnswer);
                    await Db.SaveChangesAsync();
                    //these question ids in this lst will not be inserted
                    List<long> specialQuestionIdLst = await AttachQuestionsToAnswerAndSaveInDb(cannedReplyAnswer, cannedReply.Questions);
                    if (specialQuestionIdLst.Any())
                    {
                        if (specialQuestionIdLst.Count == new HashSet<string>(cannedReply.Questions).Count)
                        {
                            //TODO: backend job to clean these answers if transaction failed
                            Db.CannedReplyAnswers.Remove(cannedReplyAnswer);
                            await Db.SaveChangesAsync();
                            return new CannedReply.CannedReplyAnswerInternalResult
                            {
                                CannedReplyAnswer = null,
                                Status = "Error",
                                ErrorMessage = "No questions to Add or all questions are duplicated when creating canned Reply."
                            };
                        }
                        return new CannedReply.CannedReplyAnswerInternalResult
                        {
                            CannedReplyAnswer = cannedReplyAnswer,
                            Status = "CreatedWithSkip",
                            ErrorMessage = $"Questions are skipped because this answer or other answers have these questions, questionIds: {string.Join(",", specialQuestionIdLst)}",
                            //Status = "CreatedWithUpdate",
                            //ErrorMessage = $"Questions are updated to be attached with this new answer, questionIds: {string.Join(",", specialQuestionIdLst)}",
                            QuestionIds = specialQuestionIdLst
                        };
                    }
                    else
                    {
                        return new CannedReply.CannedReplyAnswerInternalResult
                        {
                            CannedReplyAnswer = cannedReplyAnswer,
                            Status = "Created",
                            ErrorMessage = ""
                        };
                    }
                }
                catch (Exception ex)
                {
                    ex = GetInnerException(ex);
                    return new CannedReply.CannedReplyAnswerInternalResult
                    {
                        CannedReplyAnswer = null,
                        Status = "Error",
                        ErrorMessage = ex.Message + ex.StackTrace
                    };
                }

            }

        }

        private async Task<CannedReply.CannedReplyAnswerInternalResult> UpdateCannedReplyAnswerToDb(CannedReply cannedReply)
        {
            var curMerchantIdFromDb = await Db.Merchants.Select(m => m.Id).FirstOrDefaultAsync(m => m == cannedReply.MerchantId);
            if (curMerchantIdFromDb == 0)
            {
                return new CannedReply.CannedReplyAnswerInternalResult
                {
                    CannedReplyAnswer = null,
                    Status = "Error",
                    ErrorMessage = "MerchantId is not valid or There is no such MerchantId in DB."
                };
            }
            CannedReplyAnswer craFromDb = await Db.CannedReplyAnswers.Include(c => c.CannedReplyQuestions).SingleOrDefaultAsync(c => c.Id == cannedReply.Id);
            if (craFromDb == null)
            {
                return new CannedReply.CannedReplyAnswerInternalResult
                {
                    CannedReplyAnswer = null,
                    Status = "Error",
                    ErrorMessage = "record not found in DB"
                };
            }

            craFromDb.ReplyContent = cannedReply.ReplyContent ?? craFromDb.ReplyContent;
            string csReplyContent = CannedReply.GetMd5Hash(craFromDb.ReplyContent);
            CannedReplyAnswer craFromDbWithSameAnswer = await Db.CannedReplyAnswers.Where(c => c.CS_ReplyContent == csReplyContent
                                                                                    && c.MerchantId == craFromDb.MerchantId
                                                                                    && c.UserId == craFromDb.UserId
                                                                                    && c.Id != craFromDb.Id)
                                                                                   .Include(c => c.CannedReplyQuestions)
                                                                     .FirstOrDefaultAsync(c => c.ReplyContent == craFromDb.ReplyContent);
            if (craFromDbWithSameAnswer == null)
            {
                craFromDb.IsAutoReplyEnabled = cannedReply.IsAutoReplyEnabled;
                craFromDb.CategoryName = cannedReply.CategoryName ?? craFromDb.CategoryName;
                try
                {
                    if (cannedReply.Questions != null)
                    {
                        List<long> specialQuestionIdLst = await ForceAttachWithNewQuestionsToAnswerAndNotSaveInDb(craFromDb, cannedReply.Questions);
                        await Db.SaveChangesAsync();
                        if (specialQuestionIdLst.Any())
                        {
                            return new CannedReply.CannedReplyAnswerInternalResult
                            {
                                CannedReplyAnswer = craFromDb,
                                Status = "UpdatedWithSkip",
                                ErrorMessage = $"Questions are skipped because this answer or other answers have these questions, questionIds: {string.Join(",", specialQuestionIdLst)}",
                                //Status = "UpdateWithForceUpdate",
                                //ErrorMessage = $"Questions are updated with this new answer, questionIds: {string.Join(",", specialQuestionIdLst)}",
                                QuestionIds = specialQuestionIdLst

                            };
                        }
                        else
                        {
                            return new CannedReply.CannedReplyAnswerInternalResult
                            {
                                CannedReplyAnswer = craFromDb,
                                Status = "Updated",
                                ErrorMessage = ""
                            };
                        }
                    }
                    else
                    {
                        await Db.SaveChangesAsync();
                        return new CannedReply.CannedReplyAnswerInternalResult
                        {
                            CannedReplyAnswer = craFromDb,
                            Status = "Updated",
                            ErrorMessage = ""
                        };
                    }

                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!CannedReplyExists(cannedReply.Id))
                    {
                        return new CannedReply.CannedReplyAnswerInternalResult
                        {
                            CannedReplyAnswer = null,
                            Status = "Error",
                            ErrorMessage = "record not found in DB"
                        };
                    }
                    else
                    {
                        return new CannedReply.CannedReplyAnswerInternalResult
                        {
                            CannedReplyAnswer = null,
                            Status = "Error",
                            ErrorMessage = ex.Message + ex.StackTrace
                        };
                    }
                }
                catch (Exception ex)
                {
                    ex = GetInnerException(ex);
                    return new CannedReply.CannedReplyAnswerInternalResult
                    {
                        CannedReplyAnswer = null,
                        Status = "Error",
                        ErrorMessage = ex.Message + ex.StackTrace
                    };
                }
            }
            else
            {
                cannedReply.CategoryName = cannedReply.CategoryName ?? craFromDb.CategoryName;
                if (cannedReply.Questions == null)
                {
                    var mergeResult = await MergeExistingQuestionsToExistingAnswerInDb(craFromDb, craFromDbWithSameAnswer);
                    Db.CannedReplyAnswers.Remove(craFromDb);
                    await Db.SaveChangesAsync();
                    return mergeResult;
                }
                else
                {
                    Db.CannedReplyQuestions.RemoveRange(craFromDb.CannedReplyQuestions);
                    Db.CannedReplyAnswers.Remove(craFromDb);
                    await Db.SaveChangesAsync();
                    return await MergeCannedReplyToExistingAnswerInDb(cannedReply, craFromDbWithSameAnswer);
                }
            }
        }
        public IQueryable<CannedReply> GetCannedReplies()
        {
            return Db.CannedReplyAnswers.Select(CannedReply.TransCraToCrLambda);
        }

        public IQueryable<CannedReply> GetCannedReplies(long merchantId, long? userId)
        {
            return Db.CannedReplyAnswers.Where(m => m.MerchantId == merchantId && m.UserId == userId)
                                        .Select(CannedReply.TransCraToCrLambda);
        }

        public SingleResult<CannedReply> GetCannedReply( long key)
        {
            return SingleResult.Create(
                Db.CannedReplyAnswers.Include(cra => cra.CannedReplyQuestions)
                .Where(cra => cra.Id == key)
                .Select(CannedReply.TransCraToCrLambda));
        }

        public async Task<CannedReply.CannedReplyBulkResult> Put( long key, CannedReply cannedReply)
        {
            CannedReply.CannedReplyAnswerInternalResult res = await UpdateCannedReplyAnswerToDb(cannedReply);

            return new CannedReply.CannedReplyBulkResult
            {
                Id = res.CannedReplyAnswer?.Id ?? -1,
                Status = res.Status,
                ErrorMessage = res.ErrorMessage,
                QuestionIds = res.QuestionIds
            };
        }

        public async Task<CannedReply.CannedReplyBulkResult> Post(CannedReply cannedReply)
        {
            CannedReply.CannedReplyAnswerInternalResult res = await CreateCannedReplyAnswerToDb(cannedReply);
            return new CannedReply.CannedReplyBulkResult
            {
                Id = res.CannedReplyAnswer?.Id ?? -1,
                Status = res.Status,
                ErrorMessage = res.ErrorMessage,
                QuestionIds = res.QuestionIds
            };
        }

        public async Task<List<CannedReply.CannedReplyBulkResult>> BulkPost(IEnumerable<CannedReply> cannedReplies)
        {
            List<CannedReply.CannedReplyBulkResult> result = new List<CannedReply.CannedReplyBulkResult>();
            foreach (var crWithIndex in cannedReplies.Select((item, index) => new { index, item }))
            {
                if (crWithIndex.item.Id == 0)
                {
                    try
                    {
                        CannedReply.CannedReplyAnswerInternalResult cannedReplyAnswerInternalResult = await CreateCannedReplyAnswerToDb(crWithIndex.item);
                        result.Add(new CannedReply.CannedReplyBulkResult
                        {
                            Index = crWithIndex.index,
                            Status = cannedReplyAnswerInternalResult.Status,
                            Id = cannedReplyAnswerInternalResult.CannedReplyAnswer?.Id ?? -1,
                            ErrorMessage = cannedReplyAnswerInternalResult.ErrorMessage,
                            QuestionIds = cannedReplyAnswerInternalResult.QuestionIds //?? new List<long>()
                        });
                    }
                    catch (Exception ex)
                    {
                        ex = GetInnerException(ex);
                        result.Add(new CannedReply.CannedReplyBulkResult
                        {
                            Index = crWithIndex.index,
                            Status = "Error",
                            Id = -1,
                            ErrorMessage = ex.Message + ex.StackTrace,
                            //QuestionIds = new List<long>()
                        });
                    }

                }
                else
                {
                    try
                    {
                        CannedReply.CannedReplyAnswerInternalResult cannedReplyAnswerInternalResult =
                                                        await UpdateCannedReplyAnswerToDb(crWithIndex.item);
                        result.Add(new CannedReply.CannedReplyBulkResult
                        {
                            Index = crWithIndex.index,
                            Status = cannedReplyAnswerInternalResult.Status,
                            Id = cannedReplyAnswerInternalResult.CannedReplyAnswer?.Id ?? crWithIndex.item.Id,
                            ErrorMessage = cannedReplyAnswerInternalResult.ErrorMessage,
                            QuestionIds = cannedReplyAnswerInternalResult.QuestionIds //?? new List<long>()
                        });
                    }
                    catch (Exception ex)
                    {
                        ex = GetInnerException(ex);
                        result.Add(new CannedReply.CannedReplyBulkResult
                        {
                            Index = crWithIndex.index,
                            Status = "Error",
                            Id = crWithIndex.item.Id,
                            ErrorMessage = ex.Message + ex.StackTrace,
                            //QuestionIds = new List<long>()
                        });
                    }

                }
            }
            return result;
        }
        public async Task<CannedReply.CannedReplyBulkResult> Patch( long key, CannedReply cannedReply)
        {
            CannedReply.CannedReplyAnswerInternalResult res = await UpdateCannedReplyAnswerToDb(cannedReply);

            return new CannedReply.CannedReplyBulkResult
            {
                Id = res.CannedReplyAnswer?.Id ?? -1,
                Status = res.Status,
                ErrorMessage = res.ErrorMessage,
                QuestionIds = res.QuestionIds
            };
        }

        // DELETE: odata/CannedReplies(5)
        public async Task<string> Delete( long key)
        {
            CannedReplyAnswer cannedReplyAnswer = await Db.CannedReplyAnswers.Include(c => c.CannedReplyQuestions).SingleOrDefaultAsync(c => c.Id == key);
            if (cannedReplyAnswer == null)
            {
                return "404";
            }
            Db.CannedReplyQuestions.RemoveRange(cannedReplyAnswer.CannedReplyQuestions);
            Db.CannedReplyAnswers.Remove(cannedReplyAnswer);
            await Db.SaveChangesAsync();
            return "";

        }

        private bool CannedReplyExists(long key)
        {
            return Db.CannedReplyAnswers.Count(e => e.Id == key) > 0;
        }
    }

}
