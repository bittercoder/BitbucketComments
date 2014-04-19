using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DevDefined.Bitbucket.MMBot.Models;

namespace DevDefined.Bitbucket.MMBot.Client
{
    public class BitbucketApiClient : IBitbucketApiClient
    {
        readonly HttpClient client;

        public BitbucketApiClient(string username, string password)
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = BasicAuth(username, password);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<CommitsList> GetCommits(string owner, string repoSlug)
        {
            string url = string.Format("https://bitbucket.org/!api/2.0/repositories/{0}/{1}/commits", owner, repoSlug);
            HttpResponseMessage response = await client.GetAsync(url);
            return await ReadTypedResponse<CommitsList>(response);
        }

        public IEnumerable<Commit> GetAllCommits(string owner, string repoSlug)
        {
            CommitsList current= GetCommits(owner, repoSlug).Result;
            while (current != null && current.Values != null && current.Values.Length > 0)
            {
                foreach (var value in current.Values)
                {
                    yield return value;
                }
                if (current.Next == null) break;
                current = NextPage(current).Result;
            }
        }

        public async Task<CommitsList> NextPage(CommitsList current)
        {
            HttpResponseMessage response = await client.GetAsync(new Uri(current.Next));
            return await ReadTypedResponse<CommitsList>(response);
        }

        public async Task<CommentList> CommentsFor(Commit commit)
        {
            string url = commit.Links["comments"].Href;
            HttpResponseMessage response = await client.GetAsync(url);
            return await ReadTypedResponse<CommentList>(response);
        }

        public async Task<CommitDetails> GetDetailsForCommit(Commit commit)
        {
            string url = commit.Links["self"].Href;
            HttpResponseMessage response = await client.GetAsync(url);
            return await ReadTypedResponse<CommitDetails>(response);
        }

        async Task<T> ReadTypedResponse<T>(HttpResponseMessage response)
            where T : IHaveETag
        {
            response.EnsureSuccessStatusCode();
            T result = await response.Content.ReadAsAsync<T>();
            ApplyETag(result, response);
            return result;
        }

        static void ApplyETag(IHaveETag result, HttpResponseMessage response)
        {
            result.ETag = response.Headers.ETag.Tag;
        }
        
        static AuthenticationHeaderValue BasicAuth(string username, string password)
        {
            return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", username, password))));
        }
    }
}