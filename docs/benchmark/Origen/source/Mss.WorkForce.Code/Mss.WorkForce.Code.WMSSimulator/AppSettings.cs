namespace Mss.WorkForce.Code.WMSSimulator

{

    public class AppSettings
    {
        public ConnectionConfig ConnectionStrings { get; set; } = new();
        public ExternalServicesConfig ExternalServices { get; set; } = new();
        public SimulationsConfig Simulations { get; set; } = new();
    }

    public class ConnectionConfig
    {
        public string WFMConnection { get; set; } = string.Empty;
        public string WMSConnection { get; set; } = string.Empty;
    }


    public class ExternalServicesConfig
    {
        public string ApiServiceAPI { get; set; } = string.Empty;
    }
    public class SimulationsConfig
    {
        public int MinutesPeriod { get; set; } = 60;
    }
}
