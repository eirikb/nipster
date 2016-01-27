using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;

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