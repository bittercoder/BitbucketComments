using System;

namespace DevDefined.Bitbucket.MMBot.ChangeScanner
{
    public interface ICommentMetaStore
    {
        StorageResult Store(long commentId, string commitHash, DateTime updatedOn);
    }
}