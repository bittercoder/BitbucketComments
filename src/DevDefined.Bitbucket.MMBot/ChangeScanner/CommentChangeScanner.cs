using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevDefined.Bitbucket.MMBot.Client;
using DevDefined.Bitbucket.MMBot.Models;

namespace DevDefined.Bitbucket.MMBot.ChangeScanner
{
    public class CommentChangeScanner
    {
        readonly IBitbucketApiClient _client;
        readonly ICommentMetaStore _store;

        public CommentChangeScanner(IBitbucketApiClient client, ICommentMetaStore store)
        {
            _client = client;
            _store = store;
        }

        public IEnumerable<CommentView> ScanForNewComments(TimeSpan timespan, string owner, string repoSlug)
        {
            List<Commit> recentCommits = _client.GetAllCommits(owner, repoSlug).InTheLast(timespan).ToList();

            var queue = new ConcurrentQueue<CommentView>();

            Task.WaitAll(recentCommits.Select(commit =>
            {
                return _client.CommentsFor(commit).ContinueWith(response =>
                {
                    foreach (Comment comment in response.Result.Values)
                    {
                        StorageResult result = ChangedOrNew(comment, commit);
                        if (result != StorageResult.Exists)
                        {
                            queue.Enqueue(new CommentView {CommitAuthor = commit.Author.User.UserName, Comment = comment, CommitLinks = commit.Links, Hash = commit.Hash, IsUpdate = (result == StorageResult.Updated)});
                        }
                    }
                });
            }).ToArray());

            return queue;
        }

        StorageResult ChangedOrNew(Comment comment, Commit commit)
        {
            return _store.Store(comment.Id, commit.Hash, comment.UpdatedOn);
        }
    }
}