using System;
using System.Collections.Generic;
using DevDefined.Bitbucket.MMBot.Models;

namespace DevDefined.Bitbucket.MMBot.ApprovalScanner
{
    public class CommitInfo
    {
        public List<CommitDetails> CommitsAwaitingApproval { get; set; }
        public List<Tuple<User[], CommitDetails>> ApprovedCommits { get; set; }        
    }
}