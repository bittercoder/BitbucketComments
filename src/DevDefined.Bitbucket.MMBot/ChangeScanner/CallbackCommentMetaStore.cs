using System;

namespace DevDefined.Bitbucket.MMBot.ChangeScanner
{
    public class CallbackCommentMetaStore : ICommentMetaStore
    {
        readonly Func<long, CommentMeta> _readMeta;
        readonly Action<long, CommentMeta> _writeMeta;
        object _globalLock = new object();

        public CallbackCommentMetaStore(Func<long, CommentMeta> readMeta, Action<long, CommentMeta> writeMeta)
        {
            _readMeta = readMeta;
            
            _writeMeta = (id, meta) =>
            {
                // turns out brain out of the box doesn't deal well with concurrency...
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