namespace ATY_HBL2D_SHKSchedulerService
{
    internal class CreateLog
    {
        private static readonly SemaphoreSlim _fileLockErr = new(1);
        private static readonly SemaphoreSlim _fileLockInf = new(1);
        public static void CheckEerrLog()
        {

            if (!Directory.Exists($"{AppContext.BaseDirectory}/logs"))
            {
                Directory.CreateDirectory($"{AppContext.BaseDirectory}/logs");
            }
            if (File.Exists($"{AppContext.BaseDirectory}/logs/Applog.txt"))
            {
                FileInfo fi = new($"{AppContext.BaseDirectory}/logs/Applog.txt");
                if (fi.Length > 10000000)
                //if (fi.Length > 10000)
                {
                    File.Move($"{AppContext.BaseDirectory}/logs/Applog.txt", $"{AppContext.BaseDirectory}/logs/Applog{DateTime.Now:yyyyMMddHHmmssffff}.txt");
                }
            }

        }
        public static void CheckInfLog()
        {

            if (!Directory.Exists($"{AppContext.BaseDirectory}/logs"))
            {
                Directory.CreateDirectory($"{AppContext.BaseDirectory}/logs");
            }
            if (File.Exists($"{AppContext.BaseDirectory}/logs/Infolog.txt"))
            {
                FileInfo fi = new($"{AppContext.BaseDirectory}/logs/Infolog.txt");
                if (fi.Length > 10000000)
                //if (fi.Length > 10000)
                {
                    File.Move($"{AppContext.BaseDirectory}/logs/Infolog.txt", $"{AppContext.BaseDirectory}/logs/Infolog{DateTime.Now:yyyyMMddHHmmssffff}.txt");
                }
            }

        }
        public static async Task UpdateErrLogAsync(string log)
        {
            await UpdateInfoAsync(log, "ERROR");
            try
            {
                await _fileLockErr.WaitAsync();
                using StreamWriter streamWriter = new($"{AppContext.BaseDirectory}/logs/Applog.txt", true);
                await streamWriter.WriteLineAsync($"{DateTimeOffset.Now:dd/MM/yyyy hh:mm:ss tt zzz}: {log}");

            }
            finally
            {
                CheckEerrLog();
                _fileLockErr.Release();
            }

        }
        public static async Task UpdateInfoAsync(string log, string inf = "INFO")
        {
            try
            {
                await _fileLockInf.WaitAsync();
                using StreamWriter streamWriter = new($"{AppContext.BaseDirectory}/logs/Infolog.txt", true);
                await streamWriter.WriteLineAsync($"{DateTimeOffset.Now:dd/MM/yyyy hh:mm:ss tt zzz}: {inf} {log}");
            }
            finally
            {
                CheckInfLog();
                _fileLockInf.Release();
            }
        }
    }
}
