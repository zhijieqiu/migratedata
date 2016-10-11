using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace migratedata
{
    class MigrateData
    {
        /*static bool Migrate()
        {

            using (var context = new cssmanagementdbEntities())
            {
                try
                {
                    if (context.Database.Connection.State != ConnectionState.Open)
                    {
                        context.Database.Connection.Open();
                    }
                }
                catch (Exception)
                {
                    // ignore open twice
                }
                using (var cmd = context.Database.Connection.CreateCommand())
                {
                    cmd.CommandText = "exec sp_set_session_context @key, @value";
                    cmd.Parameters.Add(new SqlParameter("key", SqlDbType.NVarChar)
                    {
                        Value = "Role"
                    });
                    cmd.Parameters.Add(new SqlParameter("value", SqlDbType.NVarChar)
                    {
                        Value = "root"
                    });

                    cmd.ExecuteNonQuery();
                    var cList = context.CannedReplies.Where(c => c.MoreQuestionsJson != null || c.StandardQuestion != null).OrderBy(c => c.Id).ToList();
                    Console.OutputEncoding = System.Text.Encoding.Unicode;
                    foreach (var c in cList)
                    {
                        CannedReplyAnswer cra = new CannedReplyAnswer(c);
                        HashSet<string> qset = new HashSet<string>(c.MoreQuestions);
                        qset.Add(c.StandardQuestion);
                        qset.Remove(null);
                        if (qset.Any())
                        {
                            foreach (var q in qset)
                            {
                                cra.CannedReplyQuestions.Add(new CannedReplyQuestion(cra, q));
                            }
                            context.CannedReplyAnswers.Add(cra);
                        }
                        Console.WriteLine(String.Format("{0} {1}", c.Id, Encoding.Default.GetString(Encoding.Unicode.GetBytes(c.ReplyContent))));
                        context.SaveChanges();
                    }

                }
            }
            return true;
        }
        static void Check()
        {

            using (var context = new cssmanagementdbEntities())
            {
                try
                {
                    if (context.Database.Connection.State != ConnectionState.Open)
                    {
                        context.Database.Connection.Open();
                    }
                }
                catch (Exception)
                {
                    // ignore open twice
                }
                using (var cmd = context.Database.Connection.CreateCommand())
                {
                    cmd.CommandText = "exec sp_set_session_context @key, @value";
                    cmd.Parameters.Add(new SqlParameter("key", SqlDbType.NVarChar)
                    {
                        Value = "Role"
                    });
                    cmd.Parameters.Add(new SqlParameter("value", SqlDbType.NVarChar)
                    {
                        Value = "root"
                    });

                    cmd.ExecuteNonQuery();
                }

                foreach (var c in context.CannedReplies.Where(c => c.MoreQuestionsJson != null || c.StandardQuestion != null).OrderBy(c => c.Id))
                {
                    CannedReplyAnswer cra = context.CannedReplyAnswers.SingleOrDefault(ca => ca.ReplyContent == c.ReplyContent);
                    HashSet<string> qset = new HashSet<string>(c.MoreQuestions);
                    qset.Add(c.StandardQuestion);
                    qset.Remove(null);
                    if (qset.Any())
                    {
                        if (qset.Count != cra.CannedReplyQuestions.Count) throw new Exception("not equal");
                        foreach (var crq in cra.CannedReplyQuestions)
                        {
                            if (!qset.Contains(crq.Question))
                            {
                                throw new Exception("not equal");
                            }
                        }

                    }
                    else
                    {
                        if (cra != null)
                            throw new Exception("not null");
                    }


                }
                Console.ReadKey();
            }
        }*/
    }
}
