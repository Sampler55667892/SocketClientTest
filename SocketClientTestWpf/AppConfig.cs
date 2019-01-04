using System.Configuration;

namespace SocketClientTestWpf
{
    public static class AppConfig
    {
        public static string ServerHost => ConfigurationManager.AppSettings["ServerHost"];
        public static int ServerPort => System.Convert.ToInt32(ConfigurationManager.AppSettings["ServerPort"]);
    }
}
