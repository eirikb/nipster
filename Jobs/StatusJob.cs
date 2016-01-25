using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Nipster.Util;

namespace Nipster.Jobs
{
    public class StatusJob
    {
        private const int LogLineLimit = 1000;
        private static readonly Log Log = new Log(typeof (StatusJob));

        public static void Process(
            [TimerTrigger("00:15:00", RunOnStartup = true)] TimerInfo timerInfo,
            [Queue("github")] CloudQueue gitHubQueue,
            [Table("github")] CloudTable gitHubTable,
            [Table("npm")] CloudTable npmTable,
            [Blob("test")] CloudBlobContainer container,
            [Blob("$root/status.txt")] TextWriter statusWriter)
        {
            Log.Info("Loading status data...");

            gitHubQueue.FetchAttributes();

            statusWriter.WriteLine($"Updated: {DateTime.UtcNow.ToString("u")}");
            statusWriter.WriteLine($"GitHub Queue count: {gitHubQueue.ApproximateMessageCount}");
            statusWriter.WriteLine($"GitHub Table count: {CountTable(gitHubTable, "github")}");
            statusWriter.WriteLine($"Npm Table count: {CountTable(npmTable, "npm")}");
            statusWriter.WriteLine("");
            statusWriter.WriteLine($"Last {LogLineLimit} lines from log:");

            var lines = container.ListBlobs(null, true)
                .OfType<ICloudBlob>()
                .OrderByDescending(b => b.Properties.LastModified)
                .Take(2)
                .Reverse()
                .SelectMany(ReadAllLines)
                .Reverse()
                .Take(LogLineLimit)
                .Reverse();

            lines.ToList().ForEach(statusWriter.WriteLine);

            Log.Info("Done");
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

        private static int CountTable(CloudTable table, string partitionKey)
        {
            return
                table.ExecuteQuery(new TableQuery().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey",
                        QueryComparisons.Equal,
                        partitionKey)).Select(new List<string> {"PartitionKey"})).Count();
        }
    }
}