namespace Mss.WorkForce.Code.WMSSimulator.Helper.Methods
{
    public static class SGALogger
    {
        public static void LogError(string message, string source = "Unknown")
        {
            Console.Error.WriteLine($"[ERROR] [{source}]: {message}");
        }

        public static void LogWarning(string message, string source = "Unknown")
        {
            Console.Error.WriteLine($"[WARNING] [{source}]: {message}");
        }

        public static void LogInfo(string message, string source = "Unknown")
        {
            Console.WriteLine($"[INFO] [{source}]: {message}");
        }
    }

}
