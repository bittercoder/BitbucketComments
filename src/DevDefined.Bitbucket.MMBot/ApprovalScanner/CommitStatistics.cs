using System;
using System.Collections.Generic;
using DevDefined.Bitbucket.MMBot.Models;

namespace DevDefined.Bitbucket.MMBot.ApprovalScanner
{
    public class CommitStatistics
    {
        public List<Tuple<User, int>> TotalApprovalsByUser { get; set; }
    }
}