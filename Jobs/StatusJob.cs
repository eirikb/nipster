using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace Nipster.Jobs
{
    public class StatusJob
    {
        private const int LogLineLimit = 100;

        public static async Task ProcessStatusJob(
            [TimerTrigger("00:30:00", RunOnStartup = true)] TimerInfo timerInfo,
            [Queue("github")] CloudQueue gitHubQueue,
            [Table("github")] CloudTable gitHubTable,
            [Table("npm")] CloudTable npmTable,
            [Blob("logs")] CloudBlobContainer container,
            [Blob("$root/status.txt")] TextWriter statusWriter)
        {
            gitHubQueue.FetchAttributes();

            var gitHubQueueCount = gitHubQueue.ApproximateMessageCount;
            var gitHubTableCount = await CountTable(gitHubTable, "github");
            var npmTableCount = await CountTable(npmTable, "npm");

            var lines = container.ListBlobs(null, true)
                .OfType<ICloudBlob>()
                .OrderByDescending(b => b.Properties.LastModified)
                .Take(2)
                .Reverse()
                .SelectMany(ReadAllLines)
                .Reverse()
                .Take(LogLineLimit)
                .Reverse();


            statusWriter.WriteLine($"Updated: {DateTime.UtcNow.ToString("u")}");
            statusWriter.WriteLine($"GitHub Queue count: {gitHubQueueCount}");
            statusWriter.WriteLine($"GitHub Table count: {gitHubTableCount}");
            statusWriter.WriteLine($"Npm Table count: {npmTableCount}");
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

        private static async Task<int> CountTable(CloudTable table, string partitionKey)
        {
            var query = new TableQuery().Where(
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal,
                    partitionKey)).Select(new List<string> {"PartitionKey"});

            var count = 0;
            TableContinuationToken token = null;

            do
            {
                var queryResult = await table.ExecuteQuerySegmentedAsync(query, token);
                count += queryResult.Results.Count;
                token = queryResult.ContinuationToken;
            } while (token != null);

            return count;
        }
    }
}