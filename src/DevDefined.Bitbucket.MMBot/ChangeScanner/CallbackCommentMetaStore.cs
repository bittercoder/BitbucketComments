using System;

namespace DevDefined.Bitbucket.MMBot.ChangeScanner
{
    public class CallbackCommentMetaStore : ICommentMetaStore
    {
        readonly Func<long, CommentMeta> _readMeta;
        readonly Action<long, CommentMeta> _writeMeta;
        static readonly object _globalLock = new object();

        public CallbackCommentMetaStore(Func<long, CommentMeta> readMeta, Action<long, CommentMeta> writeMeta)
        {
			// turns out brain out of the box doesn't deal well with concurrency... so we globally lock all reads and writes...

	        _readMeta = (id) =>
	        {
		        lock (_globalLock)
		        {
			        return readMeta(id);
		        }
	        };
            
            _writeMeta = (id, meta) =>
            {
                lock (_globalLock)
                {
                    writeMeta(id, meta);
                }
            };
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