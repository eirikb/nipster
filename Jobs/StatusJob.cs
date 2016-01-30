using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Nipster.Util;

namespace Nipster.Jobs
{
    public class StatusJob
    {
        private const int LogLineLimit = 500;

        public static async Task ProcessStatusJob2(
            [TimerTrigger("00:30:00", RunOnStartup = true)] TimerInfo timerInfo,
            [Queue("github")] CloudQueue gitHubQueue,
            [Table("github")] CloudTable gitHubTable,
            [Table("npm")] CloudTable npmTable,
            [Blob("logs")] CloudBlobContainer container,
            [Blob("$root/status.txt")] TextWriter statusWriter)
        {
            gitHubQueue.FetchAttributes();

            var gitHubQueueCount = gitHubQueue.ApproximateMessageCount;
            var npmTableCount = 0;
            await npmTable.QueryAsync(new TableQuery<TableEntity>().Select(new List<string> {"PartitionKey"}),
                list => npmTableCount += list.Count);

            var lines = container.ListBlobs(null, true)
                .OfType<ICloudBlob>()
                .OrderByDescending(b => b.Properties.LastModified)
                .Take(2)
                .Reverse()
                .SelectMany(ReadAllLines)
                .Reverse()
                .Take(LogLineLimit)
                .Reverse();

            var githubStatus = new Dictionary<string, int>();

            await gitHubTable.QueryAsync(new TableQuery<GitHubEntity>().Select(new List<string> {"StatusCode"}),
                list => list.GroupBy(e => e.StatusCode).ToList().ForEach(e =>
                {
                    int current;
                    var key = e.Key ?? "OK";
                    current = githubStatus.TryGetValue(key, out current) ? current : 0;
                    githubStatus[key] = current + e.Count();
                }));

            var gitHubTableCount = githubStatus.Values.Sum();

            statusWriter.WriteLine($"Updated: {DateTime.UtcNow.ToString("u")}");
            statusWriter.WriteLine($"GitHub Queue count: {gitHubQueueCount}");
            statusWriter.WriteLine($"GitHub Table count: {gitHubTableCount}");
            statusWriter.WriteLine($"Npm Table count: {npmTableCount}");
            statusWriter.WriteLine("GitHub statuses:");
            githubStatus.ToList().ForEach(pair => { statusWriter.WriteLine($"  {pair.Key}: {pair.Value}"); });

            statusWriter.WriteLine("");
            statusWriter.WriteLine($"Last {LogLineLimit} lines from log:");
            lines.ToList().ForEach(statusWriter.WriteLine);
        }

        private static List<string> ReadAllLines(ICloudBlob blob)
        {
            var lines = new List<string>();
            using (var stream = blob.OpenRead())
            {
                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        lines.Add(reader.ReadLine());
                    }
                }
            }
            return lines;
        }
    }
}