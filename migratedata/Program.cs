using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Routing.Constraints;
using Microsoft.Practices.ObjectBuilder2;
using Newtonsoft.Json;
using System.Threading;
using System.Text.RegularExpressions;
namespace migratedata
{
    class Program
    {
        static void fromRegToSynonyms(string fileName,string synonymsCandFileName)
        {
            StreamReader sr = new StreamReader(fileName);
            StreamWriter sw = new StreamWriter(synonymsCandFileName);
            string line = null;
            string[] splitSegs = new string[] { @"[\s\S]*" };
            HashSet<string> allSynonyms = new HashSet<string>();
            while ((line = sr.ReadLine()) != null)
            {
                string[] tokens = line.Split("\t".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == 3)
                {
                    string[] innerTokens = tokens[2].Split(splitSegs, StringSplitOptions.RemoveEmptyEntries);
                    var nt = innerTokens.Select(x => x.Trim("()".ToArray()));
                    foreach (string s in nt)
                    {
                        string curStr = string.Join("\t", s.Split('|').Select(x => x.ToLower()).OrderBy(x => x));
                        if (allSynonyms.Contains(curStr) == false)
                            allSynonyms.Add(curStr);
                    }
                }
            }
            foreach (var s in allSynonyms)
            {
                sw.WriteLine(s);
            }
            sr.Close();
            sw.Close();
        }
        static void Main(string[] args)
        {
            /*var dtask = DeleteAutoReplyRule(3);
            dtask.Wait();
            Console.WriteLine(dtask.Result);*/
           // Task<int> t = AddAutoReplyRule();
            /*t.Wait();
            Console.WriteLine(t.Result);*/
           // SeeCurrentStatus();
            DateTime dt1 = DateTime.Now;
            LookupRule(@"D:\zhijie\regResult\testProblems.txt", @"D:\zhijie\regResult\extract.txt", @"D:\zhijie\regResult\many.txt");
            DateTime dt2 = DateTime.Now;
            TimeSpan ts = dt2 - dt1;
            Console.WriteLine(ts.TotalMilliseconds);
            return;
            var bart = BatchAddRules();
            bart.Wait();
            Console.WriteLine(bart.Result);
            
            
            
            return;
            //AddCannedReplies();
            //fromRegToSynonyms(@"D:\zhijie\regResult\regResult3.txt",@"D:\zhijie\regResult\synonymsCandidate.txt");
            //return;
           
            
        }
        /*private static List<CannedReply> crLstCn = new List<CannedReply>{
            new CannedReply{
            ReplyContent = "您好！请问有什么可以帮您的？",
            Questions = new List<string>
            {
                "你好","你好"
            }
            }};*/
        private static List<CannedReply> crLstCn = new List<CannedReply>{
            new CannedReply{
            ReplyContent = "您好！请问有什么可以帮您的？",
            Questions = new List<string>
            {
                "你好","您好","在吗","hi","hello"
            }
            },
            new CannedReply{
            ReplyContent = "别客气，很高兴能帮到您",
            Questions = new List<string>
            {
                "谢谢","多谢","太感谢了","非常感谢"
            }
            },
            new CannedReply{
            ReplyContent = "再见，有问题您随时找我",
            Questions = new List<string>
            {
                "再见","拜拜","bye"
            }
            }
        };
        private static List<CannedReply> crLstEn = new List<CannedReply>{
            new CannedReply{
            ReplyContent = "Hi, how may I help you?",
            Questions = new List<string>
            {
                "hey","hi","hello"
            }
            },
            new CannedReply{
            ReplyContent = "My pleasure",
            Questions = new List<string>
            {
                "thanks","thank you"
            }
            },
            new CannedReply{
            ReplyContent = "bye",
            Questions = new List<string>
            {
                "see you","bye"
            }
            }
        };
        private static bool isGoodRule(AutoReplyRule rule, string problem)
        {
            bool ret = false;
            if (rule == null || rule.AutoReplyRegexes == null || rule.AutoReplyRegexes.Count() == 0) return ret;
            ret = rule.AutoReplyRegexes.Any(x => (new Regex(x.Regex)).IsMatch(problem));
            return ret;
        }

        private static List<AutoReplyRule> GetGoodRules(string problem, List<AutoReplyRule> allRules)
        {


            var goodRuels = allRules.Where(x => isGoodRule(x, problem)).ToList();

            if (goodRuels != null)
                return goodRuels.ToList();
            else return new List<AutoReplyRule>();
            //return goodRuels;
        }
        public static void LookupRule(string fileName,string oneResult,string manyResult)
        {
            AddCannedReply acr = new AddCannedReply();
            var allRules = new List<AutoReplyRule>();
            foreach (var  rule in acr.Db.AutoReplyRules)
            {
                allRules.Add(rule);
            }
            int i = 0;
            using (StreamReader sr = new StreamReader(fileName))
            {
                
                StreamWriter oneExtractRule = new StreamWriter(oneResult);
                StreamWriter manyRule = new StreamWriter(manyResult);
                StreamWriter zeroFile = new StreamWriter("D:/zhijie/regResult/zeroFile.txt");
                string line = null;
                while ((line = sr.ReadLine() )!= null)
                {
                    Console.WriteLine(i++);
                    var goodRules = GetGoodRules(line, allRules);
                    if (goodRules.Count() == 0)
                    {
                        zeroFile.WriteLine(line);
                    }else if (goodRules.Count() == 1)
                    {
                        oneExtractRule.WriteLine("{0}\n{1}\n{2}", line, goodRules[0].AutoReplyRegexes.First().Regex,goodRules[0].Description);
                        oneExtractRule.WriteLine("----------------------------------");
                    }
                    else
                    {
                        manyRule.WriteLine(line);
                        foreach (AutoReplyRule rule in goodRules)
                        {
                            manyRule.WriteLine(rule.AutoReplyRegexes.First().Regex);
                            manyRule.WriteLine(rule.Description);
                        }
                        manyRule.WriteLine("----------------------------------");
                    }
                }
                oneExtractRule.Close();
                manyRule.Close();
            }
            //Console.ReadKey();
        }
        
        
        public static void SeeCurrentStatus()
        {
            AddCannedReply acr = new AddCannedReply();
            var allRules = acr.Db.AutoReplyRules;
            string outputFileName = @"D:/zhijie/tmp/regexes.txt";
            using (StreamWriter sw = new StreamWriter(outputFileName))
            {
                foreach (var rule in allRules)
                {
                    Console.WriteLine(rule.AutoReplyRegexes.Count());
                    foreach (var regex in rule.AutoReplyRegexes)
                    {
                        //Console.WriteLine(regex.Regex);
                        sw.WriteLine(regex.Regex);
                    }
                }
                Console.WriteLine("please input a key");
                Console.ReadKey();
            }
            
        }
        public static async Task<int> BatchAddRules()
        {
            AddCannedReply acr = new AddCannedReply();
            int count = await acr.AddAutoReplyRuleAndSave(@"D:/zhijie/regResult/regResult3.txt");
            Console.WriteLine(count);
            return count;
        }
        public static async Task<int> DeleteAutoReplyRule(int ruleType)
        {
            AddCannedReply acr = new AddCannedReply();
            int ret = await acr.DeleteAutoReplyRule3(ruleType);
            Console.WriteLine(ret);
            return ret;
        }
        public static async Task<int> AddAutoReplyRule()
        {
            long merchantId = 9842;
            string describ = "";

            AutoReplyRule ruleobj = new AutoReplyRule()

            {
                MerchantId = merchantId,
                RuleType = 3,
                Description = describ,
                AutoReplyMessages = new List<AutoReplyMessage>(),
                AutoReplyKeywords = new List<AutoReplyKeyword>(),
                AutoReplyRegexes = new List<AutoReplyRegex>()
            };
            ruleobj.AutoReplyMessages.Add(new AutoReplyMessage()
            {
                MerchantId = merchantId,
                MsgJson = "",
                MsgType = "text"
            });
            AddCannedReply acr = new AddCannedReply();
            int t = await acr.AddAutoReplyRuleAndSave(ruleobj);
            Console.WriteLine(t);
            return t;
        }
        public static void AddCannedReplies()
        {
            AddCannedReply acr = new AddCannedReply();
            var options = new ParallelOptions {MaxDegreeOfParallelism = 5};
            /*var merchantsCn = acr.Db.Merchants.Where(m=> m.LanguageCode =="zh" && m.CountryCode == "cn").ToList();
            Parallel.ForEach(merchantsCn, options, (m) =>
            {
                AddCannedRepliesForOneMerchant(m.Id, true);
            });*/
            var merchantsEn = acr.Db.Merchants.Where(m =>
            //m.Id == 52 || m.Id==53
            m.LanguageCode == "en" && m.CountryCode == "us"
            ).ToList();
            Parallel.ForEach(merchantsEn, options,(m) =>
            {
                AddCannedRepliesForOneMerchant(m.Id, false);
            });
            /*var merchantsDefault = acr.Db.Merchants.Where(m=> !(m.LanguageCode == "zh" && m.CountryCode == "cn")&&!(m.LanguageCode == "en" && m.CountryCode == "us")).ToList();
            Parallel.ForEach(merchantsDefault, options, (m) =>
            {
                AddCannedRepliesForOneMerchant(m.Id, true);
            });*/
        }

        public static void AddCannedRepliesForOneMerchant(long merchantId, bool cn)
        {
            var acr = new AddCannedReply();
            List<CannedReply> cannedReplies = new List<CannedReply>();
            var lst = cn ? crLstCn : crLstEn;
            lst.ForEach((cr) =>
            {
                var cannedReply = new CannedReply
                {
                    MerchantId = merchantId,
                    UserId = null,
                    CategoryName = cn ? "问候语" : "Greeting",
                    ReplyContent = cr.ReplyContent,
                    IsAutoReplyEnabled = true,
                    Questions = cr.Questions,
                    CreationTime = DateTimeOffset.UtcNow
                };
                cannedReplies.Add(cannedReply);
            });
            var task = acr.BulkPost(cannedReplies);
            task.Wait();
            List<CannedReply.CannedReplyBulkResult> cbr = task.Result;
            File.WriteAllText($"d:\\logs\\{merchantId}.txt", JsonConvert.SerializeObject(cannedReplies));
            File.WriteAllText($"d:\\logs\\{merchantId}.result.txt", JsonConvert.SerializeObject(cbr));
            Console.Write(JsonConvert.SerializeObject(cannedReplies));
            Console.Write(JsonConvert.SerializeObject(cbr));
        }
        
    }
}
