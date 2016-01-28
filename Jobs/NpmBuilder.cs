using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Nipster.Util;

namespace Nipster.Jobs
{
    public static class NpmBuilder
    {
        private static readonly Log Log = new Log(typeof (NpmBuilder));

        public static async Task ProcessNpmBuilder(
            [TimerTrigger("24:00:00", RunOnStartup = true)] TimerInfo timerInfo,
            [Table("npm")] CloudTable npmTable,
            [Table("github")] CloudTable gitHubTable,
            [Blob("$root/npm-datatables.json", FileAccess.Write)] Stream output)
        {
            var githubs = new Dictionary<string, GitHubEntity>();
            await gitHubTable.QueryAsync<GitHubEntity>(list => list.ForEach(gh => githubs.Add(gh.Name, gh)));
            Log.Info($"Got {githubs.Count} github packages");

            var data = new List<object[]>();
            await npmTable.QueryAsync<NpmEntity>(list => list.ForEach(npm =>
            {
                GitHubEntity gitHubRepo;
                gitHubRepo = githubs.TryGetValue(npm.GitHubRepoUrl, out gitHubRepo)
                    ? gitHubRepo
                    : new GitHubEntity {Found = false};
                var date = npm.Modified ?? DateTime.MinValue;

                data.Add(new object[]
                {
                    npm.Name,
                    gitHubRepo.Name,
                    npm.Description,
                    npm.AuthorUrl + ";" + npm.Author,
                    date.ToString("yyyy-MM-dd"),
                    gitHubRepo.Forks,
                    gitHubRepo.Stargazers,
                    gitHubRepo.Watchers,
                    npm.Keywords
                });
            }));
            var res = new
            {
                lastUpdate = DateTime.UtcNow.ToString("u"),
                aaData = data.ToArray()
            };

            Log.Info($"Writing {data.Count} packages to blob...");
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(res));
            using (var memoryStream = new MemoryStream(bytes))
            {
                using (var gzipStream = new GZipStream(output, CompressionMode.Compress))
                {
                    await memoryStream.CopyToAsync(gzipStream);
                }
            }
        }
    }
}