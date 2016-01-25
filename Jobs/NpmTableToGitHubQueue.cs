using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Nipster.Util;

namespace Nipster.Jobs
{
    public class NpmTableToGitHubQueue
    {
        private static readonly Log Log = new Log(typeof (NpmTableToGitHubQueue));

        public static async Task Process(
            [TimerTrigger("00:10:00", RunOnStartup = true)] TimerInfo timerInfo,
            [Table("npm")] CloudTable table,
            [Queue("github")] ICollector<string> outputQueueMessage, [Queue("github")] CloudQueue queue)
        {
            queue.FetchAttributes();
            if (queue.ApproximateMessageCount > 0)
            {
                Log.Info($"GitHubQueue has approcimately {queue.ApproximateMessageCount} items, quitting..");
                return;
            }

            var tableQuery = new TableQuery<NpmEntity>();

            TableContinuationToken continuationToken = null;
            do
            {
                var tableQueryResult = await table.ExecuteQuerySegmentedAsync(tableQuery, continuationToken);
                continuationToken = tableQueryResult.ContinuationToken;
                Log.Info($"Rows retrieved {tableQueryResult.Results.Count}");

                var repoUrls = tableQueryResult.Results.Select(npmEntity => npmEntity.GitHubRepoUrl)
                    .Where(repoUrl => !string.IsNullOrEmpty(repoUrl))
                    .Distinct()
                    .ToList();

                Log.Info($"Adding {repoUrls.Count} repoUrls to GitHub Queue...");
                repoUrls.ForEach(outputQueueMessage.Add);
            } while (continuationToken != null);
        }
    }
}