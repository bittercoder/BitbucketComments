using System.Collections.Generic;
using DevDefined.Bitbucket.MMBot.Models;
using Newtonsoft.Json;

namespace DevDefined.Bitbucket.MMBot.ChangeScanner
{
    public class CommentView
    {
		[JsonProperty("update")]
		public bool IsUpdate { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }
        
        [JsonProperty("links")]
        public Dictionary<string, Link> CommitLinks { get; set; }
        
        [JsonProperty("comment")]
        public Comment Comment { get; set; }
    }
}