using Dapper;
using Microsoft.Data.SqlClient;
using WinSCP;

namespace Atomy_HBL2DBcode_ShakleeSchedulerService.Functions.HerbalifeProcess2DBarcode
{
    internal class HerbalifeProcess2DBarcodeScheduler
    {
        private readonly IConfiguration _configuration;
        private static string SaveTo_LocalPath_Barcode = "";
        private static string SaveTo_LocalPath_Barcode_Backup = "";

        private static string RemotePath_Barcode = "";
        private static SessionOptions sessionOptions = new() { Protocol = Protocol.Sftp };

        public HerbalifeProcess2DBarcodeScheduler(IConfiguration configuration)
        {
            _configuration = configuration;

            SchedulerHelpers.conn = SchedulerHelpers.extConfiguration!.ConnectionStrings.ConnectionStrings["GWAPI"].ConnectionString!;
            SchedulerHelpers.WebhookInstances = int.Parse(SchedulerHelpers.extConfiguration!.AppSettings.Settings["app:webhook_instances"].Value!);
            SaveTo_LocalPath_Barcode = SchedulerHelpers.extConfiguration!.AppSettings.Settings["HerbalifeProcess_Path_Barcode"].Value!; 
            SaveTo_LocalPath_Barcode_Backup = SchedulerHelpers.extConfiguration!.AppSettings.Settings["HerbalifeProcess_Path_Barcode_Backup"].Value!;
            RemotePath_Barcode = SchedulerHelpers.extConfiguration!.AppSettings.Settings["SFTPHL_Barcode"].Value!;

            sessionOptions.HostName = SchedulerHelpers.extConfiguration!.AppSettings.Settings["SFTPHL_Hostname"].Value!;
            sessionOptions.UserName = SchedulerHelpers.extConfiguration!.AppSettings.Settings["SFTPHL_UserName"].Value!;
            sessionOptions.Password = SchedulerHelpers.extConfiguration!.AppSettings.Settings["SFTPHL_Password"].Value!;
            sessionOptions.PortNumber = int.Parse(SchedulerHelpers.extConfiguration!.AppSettings.Settings["SFTPHL_Port"].Value!);
            sessionOptions.SshHostKeyFingerprint = SchedulerHelpers.extConfiguration!.AppSettings.Settings["SFTPHL_SshHostKeyFingerprint"].Value!;
            sessionOptions.SshPrivateKeyPath = SchedulerHelpers.extConfiguration!.AppSettings.Settings["SFTPHL_PrivateKeyPath"].Value!;
        }
        public static async Task HerbalifeProcessBarcode(CancellationToken stoppingToken)
        {
            string FTPDownloadResult = string.Empty;

            string StartProcess = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string EndProcess = string.Empty;

            int batchSize = 2000;

            try
            {
                using (SqlConnection _conn = new SqlConnection(SchedulerHelpers.conn))
                {

                    string select2D = """
                        SELECT A.SerialNo, A.SoReference1 +'|'+ A.SerialNo as [Barcode] FROM WHMiddleware.dbo.WH_OrderSerialNo A with(nolock)
                        LEFT JOIN WHMiddleware.dbo.WebApi_Log B with(nolock) ON (A.SerialNo = B.[Key]) WHERE isnull( B.[Key],'')='' and A.CustomerId = 'HBL'  
                        and CreateDateTime >CONVERT(date, DATEADD(Month,-1,  DATEADD(DAY,1,EOMONTH(getdate(),-1)))) 
                        """;
                    //string select2D = """
                    //    SELECT top 3000 A.SerialNo, A.SoReference1 +'|'+ A.SerialNo as [Barcode] FROM WHMiddleware.dbo.WH_OrderSerialNo A with(nolock)
                    //    LEFT JOIN WHMiddleware.dbo.WebApi_Log B with(nolock) ON (A.SerialNo = B.[Key]) WHERE isnull( B.[Key],'')='' and A.CustomerId = 'HBL'  
                    //    --and CreateDateTime >CONVERT(date, DATEADD(Month,-1, getdate())) 
                    //    """;

                    IEnumerable<HBLBarcode> hBLBarcodes = await _conn.QueryAsync<HBLBarcode>(new CommandDefinition(select2D, cancellationToken: stoppingToken));

                    //Generate the textfile
                    //If more than 2000 split to 2 file
                    //Sample filename MYC202211211413.DAT
                    //Prefix MYCyyyyMMddHHmm.DAT
                    //Store in new folder HERBALIFE_PROCESS_BARCODE_SFTP
                    //After genereate send to SFTP then move in backup \HERBALIFE_PROCESS_BARCODE_SFTP\Backup
                    var hBLBarcodesChunk = hBLBarcodes.Chunk(batchSize);

                    if (hBLBarcodesChunk.Any())
                    {
                        foreach (var item in hBLBarcodesChunk)
                        {
                            var saveTime = DateTime.Now;
                            string localPath = SaveTo_LocalPath_Barcode + "MYC" + saveTime.ToString("yyyyMMddHHmm") + ".DAT";
                            while (File.Exists(localPath))
                            {
                                localPath = SaveTo_LocalPath_Barcode + "MYC" + saveTime.AddMinutes(1).ToString("yyyyMMddHHmm") + ".DAT";
                            }

                            using (StreamWriter streamWriterBarcode = new StreamWriter(localPath))
                            {
                                foreach (var barcode in item)
                                {
                                    streamWriterBarcode.WriteLine(barcode.Barcode);
                                    EndProcess = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    string insertBarcodeLog = "Insert into WHMiddleware.dbo.WebApi_Log ([Key], [Path], [Status], [IPAddress], [StartTime], [EndTime], [CreateBy],[CreateDate], [FileName]) "
                                                    + "Values ('" + barcode.SerialNo + "','HerbalifeProcess_Barcode','Success','" + SchedulerHelpers.IP + "','" + StartProcess + "','" + EndProcess + "'," +
                                                    "'" + SchedulerHelpers.User + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + Path.GetFileName(localPath) + "')";

                                    await _conn.ExecuteAsync(new CommandDefinition(insertBarcodeLog, cancellationToken: stoppingToken));
                                }
                            }
                        }
                    }

                    ////code to sent to SFTP
                    var Dir = Directory.EnumerateFiles(SaveTo_LocalPath_Barcode);
                    foreach (string filePath in Dir)
                    {
                        string filename = Path.GetFileName(filePath);
                        string desPath = Path.Combine(SaveTo_LocalPath_Barcode_Backup, filename);
                        //Check if exist same filename in Backup folder
                        if (!File.Exists(desPath))
                        {
                            File.Copy(filePath, desPath);
                            bool status = await UploadBarcodeFileSftp(filePath, stoppingToken);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await CreateLog.UpdateErrLogAsync(ex.ToString());
                EndProcess = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                await SchedulerHelpers.LogApiRequest("AppManager", "HerbalifeProcess2DBarcode", "Failed", SchedulerHelpers.IP, StartProcess, EndProcess, "", "", CleaningData.CutdownException(ex), "", SchedulerHelpers.User, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), stoppingToken);

            }
        }

        public static async Task<bool> UploadBarcodeFileSftp(string filePath, CancellationToken stoppingToken)
        {
            string StartProcessFTP = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            try
            {

                string filename = Path.GetFileName(filePath);
                string status = await sftpuploadAsync(sessionOptions, filePath, RemotePath_Barcode + filename, "SFTP_Herbalife_UploadBarcode", stoppingToken);

            }
            catch (Exception ex)
            {
                await CreateLog.UpdateErrLogAsync(ex.ToString());
                string EndProcessFTP = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                await SchedulerHelpers.LogApiRequest("HERBALIFE_SFTP", "Process_UploadBarcode", "Failed", SchedulerHelpers.IP, StartProcessFTP, EndProcessFTP, "", "", CleaningData.CutdownException(ex), "", SchedulerHelpers.User, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), stoppingToken);
                //await LogApiRequest
            }

            return true;
        }
        public static async Task<string> sftpuploadAsync(SessionOptions sessionOptions, string SendFrom_LocalPath, string SaveTo_RemotePath, string LogName, CancellationToken stoppingToken)
        {
            string result = string.Empty;
            string StartProcessFTP = string.Empty;
            string EndProcessFTP = string.Empty;
            string EachFilename = string.Empty;
            try
            {
                //Example
                //string SendFrom_LocalPath = "C:\\Users\\64616\\Documents\\New folder\\P122796_KQ.DAT";

                using (Session session = new Session())
                {
                    // Connect
                    session.Open(sessionOptions);

                    StartProcessFTP = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    EachFilename = Path.GetFileName(SendFrom_LocalPath);

                    bool upload;
                    if (!session.FileExists(SaveTo_RemotePath))
                    {
                        upload = true;
                    }
                    else
                    {
                        DateTime remoteWriteTime = session.GetFileInfo(SaveTo_RemotePath).LastWriteTime;
                        DateTime localWriteTime = File.GetLastWriteTime(SendFrom_LocalPath);

                        if (localWriteTime > remoteWriteTime)
                        {
                            upload = true;
                        }
                        else
                        {
                            upload = false;
                        }
                    }
                    if (upload)
                    {
                        session.PutFiles(SendFrom_LocalPath, SaveTo_RemotePath, true).Check();
                        //session.RemoveFiles(RemotePath).Check();
                    }

                }
                result = "Success";
                EndProcessFTP = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


                await SchedulerHelpers.LogApiRequest("HERBALIFE_SFTP", LogName, "Success", SchedulerHelpers.IP, StartProcessFTP, EndProcessFTP, SendFrom_LocalPath, "", "", EachFilename, SchedulerHelpers.User, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), stoppingToken);

            }
            catch (Exception ex)
            {
                await CreateLog.UpdateErrLogAsync(ex.ToString());
                EndProcessFTP = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                await SchedulerHelpers.LogApiRequest("HERBALIFE_SFTP", LogName, "Failed", SchedulerHelpers.IP, StartProcessFTP, EndProcessFTP, "", "", CleaningData.CutdownException(ex), EachFilename, SchedulerHelpers.User, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), stoppingToken);
                return "Failed";
            }
            return result;
            //return 0;
        }

        public class HBLBarcode
        {
            public string SerialNo { set; get; }
            public string Barcode { set; get; } //2D Barcode
        }
    }
}
