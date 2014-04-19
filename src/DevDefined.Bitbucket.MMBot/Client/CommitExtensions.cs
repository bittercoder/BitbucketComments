using System;
using System.Collections.Generic;
using DevDefined.Bitbucket.MMBot.Models;

namespace DevDefined.Bitbucket.MMBot.Client
{
    public static class CommitExtensions
    {
        public static IEnumerable<Commit> InTheLast(this IEnumerable<Commit> commits, TimeSpan span)
        {
            DateTime after = DateTime.UtcNow.Subtract(span);

            foreach (var commit in commits)
            {
                if (commit.Date >= after)
                {
                    yield return commit;
                }
                else
                {
                    break;
                }
            }
        }
    }
}