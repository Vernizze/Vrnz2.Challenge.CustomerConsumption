using Vrnz2.Challenge.ServiceContracts.Settings;

namespace Vrnz2.Challenge.CustomerConsumption.Shared.Settings
{
    public class AppSettings
        : BaseAppSettings
    {
        public ConnectionStringsSettings ConnectionStrings { get; set; }
        public AwsSettings AwsSettings { get; set; }
        public QueuesSettings QueuesSettings { get; set; }
    }
}
