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

            statusWriter.WriteLine($"Updated: {DateTime.UtcNow}");
            statusWriter.WriteLine($"GitHub Queue count: {gitHubQueue.ApproximateMessageCount}");
            statusWriter.WriteLine($"GitHub Table count: {CountTable(gitHubTable, "github")}");
            statusWriter.WriteLine($"Npm Table count: {CountTable(npmTable, "npm")}");
            statusWriter.WriteLine("Log snippet:");

            var blob = container.ListBlobs(null, true)
                .OfType<ICloudBlob>()
                .OrderByDescending(b => b.Properties.LastModified)
                .FirstOrDefault();

            if (blob != null)
            {
                using (var stream = blob.OpenRead())
                {
                    var i = stream.Length;
                    if (i > 20000)
                    {
                        stream.Seek(-20000, SeekOrigin.End);
                    }
                    using (var reader = new StreamReader(stream))
                    {
                        statusWriter.Write(reader.ReadToEnd());
                    }
                }
            }

            Log.Info("Done");
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