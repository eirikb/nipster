using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nipster.Util.Nipster;

namespace Nipster.Clients
{
    public static class NpmClient
    {
        private const string NpmUrl = "https://registry.npmjs.org/-/all/static/today.json";

        public static async Task<IEnumerable<NpmEntity>> GetPackagesForToday()
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 10, 0);
                var response = await client.GetAsync(NpmUrl);
                var data = await response.Content.ReadAsAsync<List<dynamic>>(new[]
                {
                    new JsonMediaTypeFormatter
                    {
                        SupportedMediaTypes = {new MediaTypeHeaderValue("text/plain")}
                    }
                });

                var npmPackages = data.Select(v =>
                {
                    var url = GitHubUrl(() => v.repository.url)
                              ?? GitHubUrl(() => v.repository[0].git)
                              ?? GitHubUrl(() => v.repository[0].url)
                              ?? GitHubUrl(() => v.repository.git)
                              ?? GitHubUrl(() => v.repository)
                              ?? GitHubUrl(() => v.homepage)
                              ?? GitHubUrl(() => v.bugs.url);
                    var gitHubName = GitHubName("" + url);

                    var authorUrl = NoThrow.String(() => v.author.url) ?? NoThrow.String(() => v.author.email);

                    var name = NoThrow.String(() => v.name);
                    if (string.IsNullOrEmpty(name)) return null;

                    return new NpmEntity(name)
                    {
                        Description = NoThrow.String(() => v.description),
                        Name = name,
                        Keywords = string.Join(" ", NoThrow.StringArray(() => v.keywords) ?? new string[] {}),
                        Modified = NoThrow.DateTime(() => (DateTime) v.time.modified),
                        Author = NoThrow.String(() => v.author.name),
                        AuthorUrl = authorUrl,
                        GitHubRepoUrl = gitHubName
                    };
                });

                return npmPackages.Where(package => !string.IsNullOrEmpty(package?.Name));
            }
        }

        private static string GitHubUrl(Func<dynamic> func)
        {
            var u = "" + NoThrow.String(func);
            return Regex.IsMatch(u, "github.com", RegexOptions.IgnoreCase) ? u : null;
        }

        public static string GitHubName(string url)
        {
            url = Regex.Replace(url, @".*github.com.", "", RegexOptions.IgnoreCase);
            url = Regex.Replace(url, @"\.git$", "", RegexOptions.IgnoreCase);
            url = Regex.Replace(url, @"(.*\/.*)\/issues", "", RegexOptions.IgnoreCase);
            url = Regex.Replace(url, @"^\/*", "");
            return url.Trim().ToLower();
        }
    }
}