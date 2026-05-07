using ATY_HBL2D_SHKSchedulerService;
using ATY_HBL2D_SHKSchedulerService.Functions;
using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using WinSCP;

namespace ATY_HBL2D_SHKSchedulerService.Functions.HerbalifeProcess2DBarcode
{
    internal class BranchCodeTaskScheduler
    {
        private readonly IConfiguration _configuration;
        private static string HLSURL_BranchCode = "";



        public BranchCodeTaskScheduler(IConfiguration configuration)
        {
            _configuration = configuration;

            SchedulerHelpers.conn = SchedulerHelpers.extConfiguration!.ConnectionStrings.ConnectionStrings["GWAPI"].ConnectionString!;
            SchedulerHelpers.WebhookInstances = int.Parse(SchedulerHelpers.extConfiguration!.AppSettings.Settings["app:webhook_instances"].Value!);
            HLSURL_BranchCode = SchedulerHelpers.extConfiguration!.AppSettings.Settings["HLSURL_BranchCode"].Value!;


        }
        public static async Task<bool> UpdateLatestBranchCodeAsyncHLS(CancellationToken stoppingToken)
        {
            try
            {
                using (SqlConnection _conn = new SqlConnection(SchedulerHelpers.conn))
                {
                    //Delete previous BranchCode
                    await _conn.ExecuteAsync("delete [WHMiddleware].[dbo].[WH_BranchCode]");


                    //Get all branch code that need to be Updated
                    IEnumerable<SARBranchCode> _ttlSarBranchCode = await _conn.QueryAsync<SARBranchCode>(new CommandDefinition(" exec manifestdb.dbo.mms_select @ptype='PQ_SV_POSTCODE_WHS' ", commandType: System.Data.CommandType.StoredProcedure, cancellationToken: stoppingToken));


                    ////Latest code change to use SP
                    //List<SARBranchCode> _ttlSarBranchCode = (await _conn.QueryAsync<SARBranchCode>(" SELECT DISTINCT p1.postcode, left(D.stationcode, len(D.stationcode) - 1) stationCode, '0' as nFreq, getdate() dUp "
                    //                                                                              + " FROM gdexpdb.dbo.stationSAR p1 "
                    //                                                                              + " CROSS APPLY(SELECT "
                    //                                                                              + " p2.stationcode + ',' "
                    //                                                                              + " FROM(select "
                    //                                                                              + " r.postcode, r.stationCode, count(*) nFreq "
                    //                                                                              + " from( "
                    //                                                                              + " select "
                    //                                                                              + " a.postcode "
                    //                                                                              + " , isnull(case  when s.origin_code = 'KUL' then 'K' "
                    //                                                                              + " when s.origin_code = 'PJY' then 'P' "
                    //                                                                              + " when upper(s.region) like 'KLV%' OR upper(s.region) = 'KUL' then s.origin_code "
                    //                                                                              + " when upper(s.region) = 'SABAH' then 'B-' + a.delcode "
                    //                                                                              + " when upper(s.region) like 'SARAWAK%' then 'R-' + a.delcode "
                    //                                                                              + " else left(s.region, 1) + '-' + a.delcode end,a.stationcode) stationCode "
                    //                                                                              + "      from gdexpdb.dbo.stationSAR a(nolock) "
                    //                                                                              + " left join gdexpdb.dbo.station s(nolock) on a.stationcode = s.origin_code "
                    //                                                                              + " where a.cStatus = 'Y' "
                    //                                                                              + " ) r "
                    //                                                                              + " group by r.postcode,r.stationcode) p2 "
                    //                                                                              + " WHERE p2.postcode = p1.postcode "
                    //                                                                              + " ORDER BY P2.nFreq desc "
                    //                                                                              + " FOR XML PATH('') "
                    //                                                                              + " )  D(stationcode) "
                    //                                                                              + " where p1.cStatus = 'Y' and p1.country = 'MY' "
                    //                                                                              + " order by p1.postcode "
                    //                                                                              + "  ")).ToList();


                    if (_ttlSarBranchCode is not null)
                    {
                        //Insert latest CN
                        foreach (SARBranchCode eachSARBranchCode in _ttlSarBranchCode)
                        {

                            await _conn.ExecuteAsync(new CommandDefinition("INSERT INTO [WHMiddleware].[dbo].[WH_BranchCode] (Postcode,StationCode,nFreq,dup,CreatedDateTime) "
                                                    + "Values('" + eachSARBranchCode.postcode + "','" + eachSARBranchCode.stationCode + "','" + eachSARBranchCode.nFreq + "','" + eachSARBranchCode.dUp + "',Getdate())", cancellationToken: stoppingToken));

                        }
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                await CreateLog.UpdateErrLogAsync(ex.ToString());
                return false;
            }
        }
        public static async Task<bool> ProcessBranchCodeAsyncHLS(CancellationToken stoppingToken)
        {
            try
            {
                using (SqlConnection _conn = new SqlConnection(SchedulerHelpers.conn))
                {
                    //Get all branch code that need to be push to API
                    IEnumerable<BranchCode> _ttlBranchCode = await _conn.QueryAsync<BranchCode>(new CommandDefinition("SELECT postcode,stationcode,convert(int,nfreq) as nfreq,CONVERT(VARCHAR(20), dup, 120)  as dup  FROM [WHMiddleware].[dbo].[WH_BranchCode](NOLOCK) WHERE UpdateDateTime is null  ", cancellationToken: stoppingToken));
                    _ttlBranchCode = _ttlBranchCode.GroupBy(x => x.postcode).Select(y => y.First());

                    if (_ttlBranchCode is not null)
                    {
                        await PushBranchCode_TOHLS(_ttlBranchCode, stoppingToken);
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                await CreateLog.UpdateErrLogAsync(ex.ToString());
                return false;
            }
        }
        public static async Task<string> PushBranchCode_TOHLS(IEnumerable<BranchCode> branchCodes, CancellationToken stoppingToken)
        {
            string FunctionName = "BranchCode";
            string Function = "PushBranchCode";
            string _responseString = string.Empty;
            string HLSURL = HLSURL_BranchCode;
            HLS_Response.Response hLS_Response = new HLS_Response.Response();
            try
            {



                string JSONBranchCode = JsonConvert.SerializeObject(branchCodes);

                ///Send to HLSAPI BranchCode API

                using (var httpClient = new HttpClient())
                {

                    using var content = new StringContent(JSONBranchCode, Encoding.UTF8, "application/json");

                    ////servicepointmanager to access https
                    ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12| SecurityProtocolType.Tls13;

                    string StartPushDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    var httpResponse = await httpClient.PostAsync(HLSURL, content, stoppingToken);
                    string EndPushDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    if (httpResponse.IsSuccessStatusCode && httpResponse.ReasonPhrase == "OK")
                    {
                        var ResponseContent = await httpResponse.Content.ReadAsStringAsync(stoppingToken);

                        //Log the Success Response from HLS
                        await SchedulerHelpers.LogApiRequest(FunctionName, Function, "Success", SchedulerHelpers.IP, StartPushDateTime, EndPushDateTime, JSONBranchCode, ResponseContent, "", "", SchedulerHelpers.User, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), stoppingToken);

                    }
                    else
                    {
                        var ResponseContent = await httpResponse.Content.ReadAsStringAsync(stoppingToken);

                        //Log the Failed Response from HLS
                        await SchedulerHelpers.LogApiRequest(FunctionName, Function, "Failed", SchedulerHelpers.IP, StartPushDateTime, EndPushDateTime, JSONBranchCode, ResponseContent, "", "", SchedulerHelpers.User, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), stoppingToken);
                    }


                }
                return _responseString;
            }
            catch (Exception ex)
            {
                await CreateLog.UpdateErrLogAsync(ex.ToString());
                return _responseString;
            }
        }

        public class SARBranchCode
        {
            public string postcode { get; set; }
            public string stationCode { get; set; }
            public int nFreq { get; set; }
            public string dUp { get; set; }

        }
        public class BranchCode
        {
            public string postcode { get; set; }
            public string stationCode { get; set; }
            public int nFreq { get; set; }
            public string dUp { get; set; }

        }

        public class HLS_Response
        {
            public class Rootobject
            {
                public Response Response { get; set; }
            }

            public class Response
            {
                public Return _return { get; set; }
            }

            public class Return
            {
                public string returnFlag { get; set; }
                public string returnCode { get; set; }
                public string returnDesc { get; set; }
                public object[] resultInfo { get; set; }
            }

        }

    }
}
