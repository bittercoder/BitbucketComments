using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevDefined.Bitbucket.MMBot.ApprovalScanner;
using DevDefined.Bitbucket.MMBot.ChangeScanner;
using DevDefined.Bitbucket.MMBot.Client;
using DevDefined.Bitbucket.MMBot.Models;
using MMBot;
using MMBot.Scripts;
using User = DevDefined.Bitbucket.MMBot.Models.User;

namespace DevDefined.Bitbucket.MMBot.Scripts
{
    public class BitbucketComments : IMMBotScript
    {
        public void Register(Robot robot)
        {
            bool useFormatting = (robot.GetConfigVariable("MMBOT_BITBUCKET_USEHIPCHATFORMATTING") ?? "False").Equals("true", StringComparison.OrdinalIgnoreCase);
            bool useHipchatFormatSpecifiers = (robot.GetAdapter("HipChatAdapter") != null && useFormatting);

            robot.Respond("bitbucket comments check (.*) (.*)", msg =>
            {
                var bucketClient = CreateClient(robot, msg);
                if (bucketClient == null) return;

                string owner = msg.Match[1];
                string repoSlug = msg.Match[2];
                
                int daysToCheckForComments = Convert.ToInt32(robot.GetConfigVariable("MMBOT_BITBUCKET_DAYSFORCOMMENTS") ?? "14");
                
                var metaStore = CreateCommentMetaStore(robot, owner, repoSlug);
                
                var scanner = new CommentChangeScanner(bucketClient, metaStore);

                IEnumerable<CommentView> comments = scanner.ScanForNewComments(TimeSpan.FromDays(daysToCheckForComments), owner, repoSlug);

                foreach (var message in comments.OrderByDescending(x => x.Comment.UpdatedOn).Select(c => RenderCommentMessage(c, repoSlug, useHipchatFormatSpecifiers)))
                {
                    msg.Send(message);
                }
            });

            robot.Respond("bitbucket approvals new (.*) (.*)", msg =>
            {
                var bucketClient = CreateClient(robot, msg);
                if (bucketClient == null) return;

                string owner = msg.Match[1];
                string repoSlug = msg.Match[2];

                int daysToCheckForApprovals = Convert.ToInt32(robot.GetConfigVariable("MMBOT_BITBUCKET_DAYSFORAPPROVALS") ?? "14");

                var metaStore = CreateCommitApprovalMetaStore(robot, owner, repoSlug);

                var scanner = new CommitApprovalScanner(bucketClient, metaStore);

                var changes = scanner.Scan(TimeSpan.FromDays(daysToCheckForApprovals), owner, repoSlug);

                foreach (var message in changes.ApprovedCommits.SelectMany(c => RenderApprovedCommitMessages(c, repoSlug, useHipchatFormatSpecifiers)))
                {
                    msg.Send(message);
                }
            });

            robot.Respond("bitbucket approvals awaiting (.*) (.*)", msg =>
            {
                var bucketClient = CreateClient(robot, msg);
                if (bucketClient == null) return;

                string owner = msg.Match[1];
                string repoSlug = msg.Match[2];

                int daysToCheckForApprovals = Convert.ToInt32(robot.GetConfigVariable("MMBOT_BITBUCKET_DAYSFORAWAITINGAPPROVALS") ?? "7");

                var metaStore = CreateCommitApprovalMetaStore(robot, owner, repoSlug);

                var scanner = new CommitApprovalScanner(bucketClient, metaStore);

                var changes = scanner.Scan(TimeSpan.FromDays(daysToCheckForApprovals), owner, repoSlug);

                foreach (var message in changes.CommitsAwaitingApproval.Select(c => RenderCommitAwaitingApproval(c, repoSlug, useHipchatFormatSpecifiers)))
                {
                    msg.Send(message);
                }
            });

            robot.Respond("bitbucket approvals total (.*) (.*)", msg =>
            {
                var bucketClient = CreateClient(robot, msg);
                if (bucketClient == null) return;

                string owner = msg.Match[1];
                string repoSlug = msg.Match[2];

                int daysToDoSummaryFor = Convert.ToInt32(robot.GetConfigVariable("MMBOT_BITBUCKET_DAYSFORAPPROVALSUMMARY") ?? "30");

                var metaStore = CreateCommitApprovalMetaStore(robot, owner, repoSlug);

                var scanner = new CommitApprovalScanner(bucketClient, metaStore);

                var timespan = TimeSpan.FromDays(daysToDoSummaryFor);

                var statistics = scanner.CalculateStatistics(timespan, owner, repoSlug);

                var message = RenderApprovalStatistics(statistics, repoSlug, timespan, useHipchatFormatSpecifiers);
                
                msg.Send(message);
            });
        }
        
        static ICommentMetaStore CreateCommentMetaStore(Robot robot, string owner, string repoSlug)
        {
            Func<long, string> keyFunc = id => string.Format("bitbucket.{0}.{1}.comment.{2}", owner, repoSlug, id);
            var metaStore = new CallbackCommentMetaStore(id => robot.Brain.Get<CommentMeta>(keyFunc(id)).Result, (id, meta) => robot.Brain.Set(keyFunc(id), meta).Wait());
            return metaStore;
        }

        static ICommitApprovalMetaStore CreateCommitApprovalMetaStore(Robot robot, string owner, string repoSlug)
        {
            Func<string, string> keyFunc = hash => string.Format("bitbucket.{0}.{1}.commit.{2}.approvals", owner, repoSlug, hash);
            var metaStore = new CallbackCommitApprovalMetaStore(hash => robot.Brain.Get<ApprovalMeta>(keyFunc(hash)).Result, (hash, meta) => robot.Brain.Set(keyFunc(hash), meta).Wait());
            return metaStore;
        }
        static IBitbucketApiClient CreateClient(Robot robot, IResponse<TextMessage> msg)
        {
            string username = robot.GetConfigVariable("MMBOT_BITBUCKET_USERNAME");
            string password = robot.GetConfigVariable("MMBOT_BITBUCKET_PASSWORD");
            
            if (username == null || password == null)
            {
                msg.Send("MMBOT_BITBUCKET_USERNAME or MMBOT_BITBUCKET_PASSWORD is not currently set in the mmbot .ini file");                
            }

            return new BitbucketApiClient(username, password);
        }

        public IEnumerable<string> GetHelp()
        {
            yield return "mmbot bitbucket comments check <owner> <repoSlug> - Will check for new and updated comments in bitbucket and display links to them.";
            yield return "mmbot bitbucket approvals new <owner> <repoSlug> - Will return commits which have recently been approved by a user.";
            yield return "mmbot bitbucket approvals awaiting <owner> <repoSlug> - Will return a list of commits still requiring approval/review.";
            yield return "mmbot bitbucket approvals total <owner> <repoSlug> - Will return a list of how many approvals each user has made.";
        }

        IEnumerable<string> RenderApprovedCommitMessages(Tuple<User[], CommitDetails> tuple, string repoSlug, bool useHipchatFormatSpecifiers)
        {
            var commit = tuple.Item2;
            foreach (var user in tuple.Item1)
            {
                string formatSpecifiers = useHipchatFormatSpecifiers ? "::green ::notify ::from " + user.DisplayName : "";
                yield return string.Format("{0}@{1} has approved (bitbucket) commit #{2} (successful)\r\n\r\n{3}\r\n\r\n{4}\r\n\r\ncommit author:{4}\r\nrepository: {5}", formatSpecifiers, user.UserName, commit.Hash,
                   commit.Message, commit.Links["html"].Href, commit.Author.User.UserName, repoSlug);
            }
        }

        string RenderCommentMessage(CommentView commentView, string repoSlug, bool useHipchatFormatSpecifiers)
        {
            string sentiment = "has commented on";
            
            if (commentView.IsUpdate)
            {
                sentiment = "has updated their comment on";                
            }

            string formatSpecifiers = (useHipchatFormatSpecifiers ? (commentView.IsUpdate ? "::purple ::notify ::from bitbucket " : "::green ::notify ::from bitbucket ") : "");

            return string.Format("{6}@{0} {5} (bitbucket) commit #{1}\r\n\r\n{2}\r\n\r\n{3}\r\n\r\n\r\n\r\ncommit author: @{7}\r\nrepository: {4}",
                commentView.Comment.User.UserName, commentView.Hash.Substring(0,10), commentView.Comment.Content.Raw.TruncateWithEllipsis(256),
                commentView.Comment.Links["html"].Href, repoSlug, sentiment, formatSpecifiers,commentView.CommitAuthor);
        }
        
        string RenderApprovalStatistics(CommitStatistics statistics, string repoSlug, TimeSpan span, bool useHipchatFormatSpecifiers)
        {
            StringBuilder builder = new StringBuilder();

            if (useHipchatFormatSpecifiers) builder.Append("::html ::yellow ");

            foreach (var stat in statistics.TotalApprovalsByUser.OrderByDescending(x => x.Item2))
            {
                builder.AppendFormat("\r\n@{0}: approved {1} commits in the last {2} days\r\n", stat.Item1.UserName, stat.Item2, (int)span.TotalDays);                
            }

            builder.AppendFormat("\r\n\r\repository: {0}", repoSlug);

            return builder.ToString();
        }

        string RenderCommitAwaitingApproval(CommitDetails commit, string repoSlug, bool useHipchatFormatSpecifiers)
        {
            string formatSpecifiers = useHipchatFormatSpecifiers ? "::red ::notify ::from bitbucket " : "";

            return string.Format("{0}(bitbucket) commit #{1} has not been approved yet (philosoraptor)\r\n\r\n{2}\r\n\r\n{3}\r\n\r\ncommit author: @{4}\r\nrepository: {5}", formatSpecifiers, commit.Hash, commit.Message.TruncateWithEllipsis(256), commit.Links["html"].Href, 
                commit.Author.User.UserName, repoSlug);
        }
    }
}