using Microsoft.Azure.WebJobs;

namespace Nipster
{
    internal class Program
    {
        private static void Main()
        {
            var config = new JobHostConfiguration();
            config.UseTimers();
            var host = new JobHost(config);

            host.RunAndBlock();
        }
    }
}