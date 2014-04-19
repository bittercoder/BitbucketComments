namespace DevDefined.Bitbucket.MMBot.ApprovalScanner
{
    public interface ICommitApprovalMetaStore
    {
        void Set(string hash, ApprovalMeta meta);
        ApprovalMeta Get(string hash);
    }
}