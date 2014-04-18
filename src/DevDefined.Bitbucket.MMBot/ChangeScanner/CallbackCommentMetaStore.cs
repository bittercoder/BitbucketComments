using System;
using System.Linq;
using System.Threading;

namespace DevDefined.Bitbucket.MMBot.ChangeScanner
{
    public class CallbackCommentMetaStore : ICommentMetaStore
    {
        readonly Func<long, CommentMeta> _readMeta;
        readonly Action<long, CommentMeta> _writeMeta;
        static readonly object _globalLock = new object();

        public CallbackCommentMetaStore(Func<long, CommentMeta> readMeta, Action<long, CommentMeta> writeMeta)
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

        public StorageResult Store(long commentId, string commitHash, DateTime updatedOn)
        {
            CommentMeta meta = _readMeta(commentId);

            if (meta == null)
            {
                meta = new CommentMeta {CommitHash = commitHash, UpdatedOn = updatedOn};
                _writeMeta(commentId, meta);
                return StorageResult.New;
            }

            if (meta.UpdatedOn != updatedOn)
            {
                meta.UpdatedOn = updatedOn;
                _writeMeta(commentId, meta);
                return StorageResult.Updated;
            }

            return StorageResult.Exists;
        }
    }
}