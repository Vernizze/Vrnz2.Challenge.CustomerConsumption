using System.Threading;

namespace Vrnz2.Challenge.CustomerConsumption
{
    class Program
    {
        static void Main(string[] args)
        {
            Startup.ConfigureServices();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
