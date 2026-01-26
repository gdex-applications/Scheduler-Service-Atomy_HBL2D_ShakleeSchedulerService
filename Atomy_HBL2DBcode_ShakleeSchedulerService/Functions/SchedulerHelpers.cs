using Atomy_HBL2DBcode_ShakleeSchedulerService.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Configuration;
using System.Net;

namespace Atomy_HBL2DBcode_ShakleeSchedulerService.Functions
{
    internal static class SchedulerHelpers
    {
        internal static readonly string User = "SYSTEM";
        internal static string conn = "";

        internal static int WebhookInstances = 2;
        internal static int Retries = 3;
        internal static string IP = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1].ToString();
        internal static string filePath = "";
        internal static ExeConfigurationFileMap? fileMap;
        internal static Configuration? extConfiguration;


        public static async Task LogApiRequest(string key, string path, string status, string IP, string start, string end, string request, string response, string error, string filename, string createby, string createdate, CancellationToken stoppingToken)
        {
            try
            {
                using (SqlConnection _conn = new SqlConnection(conn))
                {
                    WebAPI_Log _request = new WebAPI_Log();

                    _request.Key = key;
                    _request.Path = path;
                    _request.Status = status;
                    _request.IPAddress = IP;
                    _request.StartTime = start;
                    _request.EndTime = end;
                    _request.RequestString = request;
                    _request.ResponseString = response;
                    _request.Error = error;
                    _request.FileName = filename;
                    _request.CreateBy = createby;
                    _request.CreateDate = createdate;

                    await _conn.ExecuteAsync(new CommandDefinition(@"INSERT INTO [WHMiddleware].[dbo].[WebApi_Log] ([Key], [Path], [Status], [IPAddress], [RequestString], [ResponseString], [Error], [StartTime], [EndTime], [FileName], [CreateBy],[CreateDate]) " +
                        "VALUES (@Key, @Path, @Status, @IPAddress, @RequestString, @ResponseString, @Error, @StartTime, @EndTime, @FileName,@CreateBy,@CreateDate)", _request, cancellationToken: stoppingToken));
                }
            }
            catch (Exception ex)
            {
                await CreateLog.UpdateErrLogAsync(ex.ToString());
                using (SqlConnection _conn = new SqlConnection(conn))
                {

                    WebAPI_Log _request = new WebAPI_Log();

                    _request.Key = key;
                    _request.Path = path;
                    _request.Status = status;
                    _request.IPAddress = IP;
                    _request.StartTime = start;
                    _request.EndTime = end;
                    _request.RequestString = request;
                    _request.ResponseString = "Failed Insert WebLog";
                    _request.Error = CleaningData.CutdownException(ex);
                    _request.FileName = filename;
                    _request.CreateBy = createby;
                    _request.CreateDate = createdate;

                    await _conn.ExecuteAsync(new CommandDefinition(@"INSERT INTO [WHMiddleware].[dbo].[WebApi_Log] ([Key], [Path], [Status], [IPAddress], [RequestString], [ResponseString], [Error], [StartTime], [EndTime], [FileName], [CreateBy],[CreateDate]) " +
                        "VALUES (@Key, @Path, @Status, @IPAddress, @RequestString, @ResponseString, @Error, @StartTime, @EndTime, @FileName,@CreateBy,@CreateDate)", _request, cancellationToken: stoppingToken));
                }

            }
        }
    }
}