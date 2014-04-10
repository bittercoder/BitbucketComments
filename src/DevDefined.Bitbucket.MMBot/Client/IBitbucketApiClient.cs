using System.Collections.Generic;
using System.Threading.Tasks;
using DevDefined.Bitbucket.MMBot.Models;

namespace DevDefined.Bitbucket.MMBot.Client
{
    public interface IBitbucketApiClient
    {
        Task<CommitsList> GetCommits(string owner, string repoSlug);
        IEnumerable<Commit> GetAllCommits(string owner, string repoSlug);
        Task<CommitsList> NextPage(CommitsList current);
        Task<CommentList> CommentsFor(Commit commit);
    }
}