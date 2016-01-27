using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Nipster.Clients;
using Nipster.Util;

namespace Nipster.Jobs
{
    public class GitHubQueueToGitHubTable
    {
        private static readonly Log Log = new Log(typeof (GitHubQueueToGitHubTable));

        public static async Task ProcessGitHubQueueToGitHubTable(
            [QueueTrigger("github")] string message,
            [Table("github")] CloudTable table,
            [Queue("github")] CloudQueue queue,
            [Blob("test/token.txt")] string token)
        {
            var rowKey = message.ToRowKey();

            var retrieveOperation = TableOperation.Retrieve<GitHubEntity>("github", rowKey);
            var result = table.Execute(retrieveOperation);
            var eTag = (result.Result as GitHubEntity)?.GitHubETag;
            Log.Info($"Fetching repo {message} from GitHub with ETag {eTag}...");
            var repo = await GetRepo(token, message, eTag);
            if (repo != null)
            {
                table.Execute(TableOperation.InsertOrReplace(repo));
                Log.Info($"Done with {message}");
            }
            else
            {
                Log.Info($"Repo {message} unchanged");
            }
        }

        private static async Task<GitHubEntity> GetRepo(string token, string repoUrl, string eTag)
        {
            try
            {
                return await GitHubClient.GetRepo(token, repoUrl, eTag);
            }
            catch (RepoNotFoundException)
            {
                return new GitHubEntity(repoUrl) {Found = false};
            }
            catch (GitHubLimitException)
            {
                Log.Info($"Limit hit for {repoUrl}");
                await Task.Delay(TimeSpan.FromHours(1));
                return await GetRepo(token, repoUrl, eTag);
            }
        }
    }
}