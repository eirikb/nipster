using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Nipster.Util;

namespace Nipster.Clients
{
    public static class GitHubClient
    {
        private const string RateLimitHeader = "X-RateLimit-Remaining";
        private static readonly Uri GitHubRepoUrl = new Uri("https://api.github.com/repos/");
        private static readonly Log Log = new Log(typeof (GitHubClient));

        public static async Task<GitHubEntity> GetRepo(string token, string repoUrl, string eTag)
        {
            using (var client = new HttpClient(new HttpClientHandler {AllowAutoRedirect = false}))
            {
                var url = new Uri(GitHubRepoUrl, repoUrl);

                client.DefaultRequestHeaders.Add("User-Agent", "nipster");
                client.DefaultRequestHeaders.Add("Authorization", "token " + token);
                if (eTag != null)
                {
                    client.DefaultRequestHeaders.Add("If-None-Match", eTag);
                }
                var response = await client.GetAsync(url);

                if (response.StatusCode == HttpStatusCode.NotModified) return null;

                var repo = new GitHubEntity("" + repoUrl)
                {
                    StatusCode = "" + response.StatusCode
                };

                var limit = TryGetHeaderInt(response.Headers, RateLimitHeader);
                if (limit.HasValue)
                {
                    Log.Info($"GitHub limit: {limit}   ({repoUrl})");
                    if (limit.Value <= 0) throw new GitHubLimitException();
                }

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Log.Info($"Repo {repoUrl} failed: {response.StatusCode}");
                    return repo;
                }

                Log.Info($"{repoUrl} - {response.StatusCode}");

                var res = await response.Content.ReadAsAsync<dynamic>();

                return new GitHubEntity("" + res.full_name)
                {
                    GitHubETag = response.Headers.ETag.Tag,
                    Found = true,
                    Forks = res.forks_count,
                    Stargazers = res.stargazers_count,
                    Watchers = res.subscribers_count
                };
            }
        }

        private static int? TryGetHeaderInt(HttpHeaders headers, string header)
        {
            IEnumerable<string> values;
            if (!headers.TryGetValues(header, out values)) return null;
            var value = values.FirstOrDefault();
            if (value == null) return null;
            int i;
            return int.TryParse(value, out i) ? i : (int?) null;
        }
    }

    public class GitHubLimitException : Exception
    {
    }

    public class RepoNotFoundException : Exception
    {
        public RepoNotFoundException(string message) : base(message)
        {
        }
    }
}