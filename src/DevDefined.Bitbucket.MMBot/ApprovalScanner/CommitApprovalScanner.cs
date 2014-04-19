using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevDefined.Bitbucket.MMBot.Client;
using DevDefined.Bitbucket.MMBot.Models;

namespace DevDefined.Bitbucket.MMBot.ApprovalScanner
{
    public class CommitApprovalScanner
    {
        readonly ICommitApprovalMetaStore _approvalMetaStore;
        readonly IBitbucketApiClient _client;

        public CommitApprovalScanner(IBitbucketApiClient client, ICommitApprovalMetaStore approvalMetaStore)
        {
            _client = client;
            _approvalMetaStore = approvalMetaStore;
        }

        public CommitInfo Scan(TimeSpan timespan, string owner, string repoSlug)
        {
            ConcurrentQueue<CommitDetails> commitDetails = ReadCommitDetails(timespan, owner, repoSlug);

            return new CommitInfo
            {
                CommitsAwaitingApproval = commitDetails.Where(x => x.Participants.All(p => p.Approved != true)).ToList(),
                ApprovedCommits = commitDetails.Select(CheckForNewApprovers).Where(x => x.Item1.Length > 0).ToList(),
            };
        }

        public CommitStatistics CalculateStatistics(TimeSpan timespan, string owner, string repoSlug)
        {
            ConcurrentQueue<CommitDetails> commitDetails = ReadCommitDetails(timespan, owner, repoSlug);

            User[] users = commitDetails.SelectMany(x => x.Participants.Select(p => p.User)).ToArray();

            User[] distinctUsers = users.GroupBy(x => x.UserName).Select(x => x.First()).ToArray();

            return new CommitStatistics
            {
                TotalApprovalsByUser = distinctUsers.Select(x => Tuple.Create(x, users.Count(u => u.UserName == x.UserName))).OrderBy(x => x.Item2).ToList()
            };
        }

        ConcurrentQueue<CommitDetails> ReadCommitDetails(TimeSpan timespan, string owner, string repoSlug)
        {
            IEnumerable<Commit> commits = _client.GetAllCommits(owner, repoSlug).InTheLast(timespan);

            var commitDetails = new ConcurrentQueue<CommitDetails>();

            Task.WaitAll(commits.Select(commit => _client.GetDetailsForCommit(commit).ContinueWith(response => commitDetails.Enqueue(response.Result))).ToArray());

            return commitDetails;
        }

        Tuple<User[], CommitDetails> CheckForNewApprovers(CommitDetails details)
        {
            ApprovalMeta previous = _approvalMetaStore.Get(details.Hash) ?? new ApprovalMeta {Approvers = new string[] {}};

            var current = new ApprovalMeta {Approvers = details.Participants.Where(x => x.Approved == true).Select(x => x.User.UserName).ToArray()};

            if (previous.Equals(current))
            {
                return new Tuple<User[], CommitDetails>(new User[] {}, details);
            }

            _approvalMetaStore.Set(details.Hash, current);

            User[] newApprovers = details.Participants.Where(x => x.Approved == true && previous.Approvers.All(u => u != x.User.UserName)).Select(x => x.User).ToArray();

            return new Tuple<User[], CommitDetails>(newApprovers, details);
        }
    }
}