using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Table;
using Nipster.Clients;
using Nipster.Util;

namespace Nipster.Jobs
{
    public class NpmRegistryToNpmTable
    {
        private static readonly Log Log = new Log(typeof (NpmRegistryToNpmTable));

        public static async void Process(
            [TimerTrigger("12:00:00", RunOnStartup = true)] TimerInfo timerInfo,
            [Table("npm")] CloudTable table)
        {
            Log.Info("Quering packages...");
            var packages = await NpmClient.GetPackagesForToday();

            var chunks = packages.Chunk(100);
            Log.Info("Processing chunks...");

            var count = 0;
            Parallel.ForEach(chunks, chunk =>
            {
                Log.Info($"Processing cunk {count++}");
                var tableBatchOperation = new TableBatchOperation();
                chunk.ToList().ForEach(npmEntity => tableBatchOperation.InsertOrReplace(npmEntity));
                table.ExecuteBatch(tableBatchOperation);
            });

            Log.Info("Done");
        }
    }
}