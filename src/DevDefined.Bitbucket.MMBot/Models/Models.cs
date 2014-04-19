using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DevDefined.Bitbucket.MMBot.Models
{
    public interface IHaveETag
    {
        [JsonIgnore]
        string ETag { get; set; }
    }

    public class Link
    {
        [JsonProperty("href")]
        public string Href { get; set; }
    }

    public abstract class Resource : IHaveETag
    {
        protected Resource()
        {
            Links = new Dictionary<string, Link>();
        }

        [JsonProperty("links")]
        public Dictionary<string, Link> Links { get; set; }

        [JsonProperty("parents")]
        public Parent[] Parents { get; set; }

        public string ETag { get; set; }
    }

    public abstract class ListResource<TItem> : IHaveETag
    {
        [JsonProperty("pagelen", NullValueHandling = NullValueHandling.Ignore)]
        public int? PageSize { get; set; }

        [JsonProperty("values")]
        public TItem[] Values { get; set; }

        [JsonProperty("page", NullValueHandling = NullValueHandling.Ignore)]
        public int? Page { get; set; }

        [JsonProperty("next", NullValueHandling = NullValueHandling.Ignore)]
        public string Next { get; set; }

        [JsonProperty("prev", NullValueHandling = NullValueHandling.Ignore)]
        public string Previous { get; set; }

        public string ETag { get; set; }
    }

    public class Comment : Resource
    {
        [JsonProperty("content")]
        public CommentContent Content { get; set; }

        [JsonProperty("created_on")]
        public DateTime CreatedOn { get; set; }

        [JsonProperty("updated_on")]
        public DateTime UpdatedOn { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }
    }

    public class CommentList : ListResource<Comment>
    {
    }

    public class CommentContent
    {
        [JsonProperty("raw")]
        public string Raw { get; set; }

        [JsonProperty("markup")]
        public string Markup { get; set; }

        [JsonProperty("html")]
        public string Html { get; set; }
    }

    public class CommitsList : ListResource<Commit>
    {
    }
    
    public class Commit : Resource
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("repository")]
        public Repository Repository { get; set; }

        [JsonProperty("author")]
        public Author Author { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public class Repository : Resource
    {
        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class User : Resource
    {
        [JsonProperty("username")]
        public string UserName { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
    }

    public class Author
    {
        [JsonProperty("raw")]
        public string Raw { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }
    }

    public class Parent : Resource
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }
    }

    public class Participant
    {
        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }
        
        [JsonProperty("approved")]
        public bool? Approved { get; set; }
    }

    public class CommitDetails : Commit
    {
        [JsonProperty("participants")]
        public Participant[] Participants { get; set; }
    }
}