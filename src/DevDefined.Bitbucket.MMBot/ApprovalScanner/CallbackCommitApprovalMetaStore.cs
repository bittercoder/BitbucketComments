using System;
using System.Linq;
using System.Threading;

namespace DevDefined.Bitbucket.MMBot.ApprovalScanner
{
    public class CallbackCommitApprovalMetaStore : ICommitApprovalMetaStore
    {
        public void Set(string hash, ApprovalMeta meta)
        {
            _writeMeta(hash, meta);
        }

        public ApprovalMeta Get(string hash)
        {
            return _readMeta(hash);
        }

        readonly Func<string, ApprovalMeta> _readMeta;
        readonly Action<string, ApprovalMeta> _writeMeta;
        static readonly object _globalLock = new object();

        public CallbackCommitApprovalMetaStore(Func<string, ApprovalMeta> readMeta, Action<string, ApprovalMeta> writeMeta)
        {
            Random random = new Random();

            // turns out brain out of the box doesn't deal well with concurrency... so we globally lock all reads and writes...

            _readMeta = (id) =>
            {
                Console.WriteLine("Reading id: {0}",id);
                while (true)
                {
                    try
                    {
                        lock (_globalLock)
                        {
                            return readMeta(id);
                        }
                    }
                    catch (AggregateException agEx)
                    {
                        if (agEx.InnerExceptions.Any(x => x is System.IO.IOException))
                        {
                            SleepAwhile(random);
                            continue;
                        }

                        throw;
                    }
                }
            };
            
            _writeMeta = (id, meta) =>
            {
                Console.WriteLine("Writing id: {0}", id);
                while (true)
                {
                    try
                    {
                        lock (_globalLock)
                        {
                            writeMeta(id, meta);
                            return;
                        }
                    }
                    catch (AggregateException agEx)
                    {
                        if (agEx.InnerExceptions.Any(x=>x is System.IO.IOException))
                        {
                            SleepAwhile(random);
                            continue;
                        }

                        throw;
                    }
                }
            };
        }

        static void SleepAwhile(Random random)
        {
            Thread.Sleep(10 + (int) (random.NextDouble()*50));
        }
    }
}