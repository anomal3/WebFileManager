namespace CompanyFileManager.Models
{
    public class AppSettings
    {
        public string SharedDirectory { get; set; } = "C:\\";
        public bool AllowUpload { get; set; } = false;
        public int Port { get; set; } = 5000;
        public bool EnableUpnp { get; set; } = true;
        public string DuckDnsToken { get; set; } = "";
        public string DuckDnsDomain { get; set; } = "";
        public bool IsConfigured { get; set; } = false;
        public bool AllowRemoteControl { get; set; } = false;
        public string RemoteControlPassword { get; set; } = "";
    }
}
