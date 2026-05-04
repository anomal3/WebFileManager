using System.Reflection;
using System.Runtime.InteropServices;

namespace CompanyFileManager.Data
{
    public static class Data
    {
        public static string path { get; set; }

        public static void InitDirectory()
        {
            //if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            //{
            //    Console.WriteLine("WINDA");
            //}
            //else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            //{
            //    Console.WriteLine("LINUXA");
            //}
            var execute = AppContext.BaseDirectory;
            try { path = File.ReadAllText(Path.Combine(execute, "sharedirectory.txt")); }
            catch (Exception ex) { Console.WriteLine(ex.Message); path = "C:\\"; }
        }
    }
}
