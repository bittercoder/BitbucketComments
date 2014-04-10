using System;
using System.Collections.Generic;
using System.Linq;
using DevDefined.Bitbucket.MMBot.ChangeScanner;
using DevDefined.Bitbucket.MMBot.Client;
using MMBot;
using MMBot.Scripts;

namespace DevDefined.Bitbucket.MMBot.Scripts
{
    public class BitbucketComments : IMMBotScript
    {
        public void Register(Robot robot)
        {
            robot.Respond("bitbucket comments check (.*) (.*)", msg =>
            {
                string owner = msg.Match[1];
                string repoSlug = msg.Match[2];
                string username = robot.GetConfigVariable("MMBOT_BITBUCKET_USERNAME");
                string password = robot.GetConfigVariable("MMBOT_BITBUCKET_PASSWORD");
                
                if (username == null || password == null)
                {
                    msg.Send("MMBOT_BITBUCKET_USERNAME or MMBOT_BITBUCKET_PASSWORD is not currently set in the mmbot .ini file");
                    return;
                }

                Func<long, string> keyFunc = id => string.Format("bitbucket.{0}.{1}.comment.{2}", owner, repoSlug, id);

                var metaStore = new CallbackCommentMetaStore(id => robot.Brain.Get<CommentMeta>(keyFunc(id)).Result, (id, meta) => robot.Brain.Set(keyFunc(id), meta).Wait());

                var bucketClient = new BitbucketApiClient(username, password);

                var scanner = new CommentChangeScanner(bucketClient, metaStore);

                IEnumerable<CommentView> comments = scanner.ScanForNewComments(owner, repoSlug);

                foreach (var message in comments.OrderByDescending(x => x.Comment.UpdatedOn).Select(c=>RenderCommentMessage(c,repoSlug)))
                {
                    msg.Send(message);
                }
            });
        }

        public IEnumerable<string> GetHelp()
        {
            yield return "mmbot bitbucket comments check <owner> <repoSlug> - Will check for new and updated comments in bitbucket and display links to them.";
        }

        string RenderCommentMessage(CommentView commentView, string repoSlug)
        {
            string sentiment = "has commented on";
            
            if (commentView.IsUpdate)
            {
                sentiment = "has updated their comment on";                
            }

            return string.Format("{0} {5} (bitbucket) commit #{1}\r\n\r\n{2}\r\n\r\n{3}\r\n\r\nrepository: {4}",
                commentView.Comment.User.DisplayName, commentView.Hash.Substring(0,10), commentView.Comment.Content.Raw.TruncateWithEllipsis(256),
                commentView.Comment.Links["html"].Href, repoSlug, sentiment);
        }
    }
}