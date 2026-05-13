using AtmyHbl2dShkleBrnCdSchedulerService;
using AtmyHbl2dShkleBrnCdSchedulerService.Functions;
using AtmyHbl2dShkleBrnCdSchedulerService.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Text;

namespace AtmyHbl2dShkleBrnCdSchedulerService.Functions.ProcessSendReciptASN
{
    internal class ProcessSendReciptASNScheduler
    {
        private readonly IConfiguration _configuration;
        private static string Token_ReceiptASN = "";

        public ProcessSendReciptASNScheduler(IConfiguration configuration)
        {
            _configuration = configuration;
            SchedulerHelpers.conn = SchedulerHelpers.extConfiguration!.ConnectionStrings.ConnectionStrings["GWAPI"].ConnectionString!;
            Token_ReceiptASN = SchedulerHelpers.extConfiguration!.AppSettings.Settings["Token_ReceiptASN"].Value!;
        }


        public static async Task ProcessSendReciptASN(CancellationToken stoppingToken)
        {
            var key = string.Empty;
            var path = "ProcessSendReciptASN";
            string ResultASN = string.Empty;
            var StartDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var EndDateTime = string.Empty;
            var query1 = """
                select a.[url],b.[Id] ,b.[warehouseId],b.[CustomerId],b.[DocNo] ,b.[docType] ,
                format(ReceivedTime,'yyyy-MM-dd HH:mm:ss') ReceivedTime,b.[userDefine1] ,b.[userDefine2],
                [userDefine3],b.[userDefine4],b.[userDefine5],b.[userDefine6],b.[Status]
                from[WHMiddleware].[dbo].[WH_ReceiptASNURL] A with(nolock)
                left join[WHMiddleware].[dbo].[WH_ReceiptASN]B with(nolock)  on a.CustomerId = b.CustomerId 
                where b.DocNo is not null and b.[Status] = '0' and a.[Status] = 1
                """;
            var query2 = "select * from [WHMiddleware].[dbo].WH_ReceiptASN_Details with(nolock) where DocNo=convert(varchar,@DocNo)";
            try
            {
                using (SqlConnection _conn = new SqlConnection(SchedulerHelpers.conn))
                {

                    //Get all header data that need to be push to API
                    IEnumerable<WH_ASN.MainASNOut> _mainData = await _conn.QueryAsync<WH_ASN.MainASNOut>(new CommandDefinition(query1, cancellationToken: stoppingToken));


                    foreach (var item in _mainData)
                    {

                        (ResultASN, EndDateTime) = await SendToApi(path, ResultASN, StartDateTime, EndDateTime, query2, _conn, item, stoppingToken);
                        //outside for fp
                        if (ResultASN == "Success")
                        {
                            string _UpdateQuery = "UPDATE [WHMiddleware].[dbo].[WH_ReceiptASN] " +
                            "SET [Status]=1 " +
                            "WHERE  [DocNo] = convert(varchar,@DocNo)";
                            try
                            {
                                await _conn.ExecuteAsync(_UpdateQuery, new
                                {
                                    DocNo = item.docNo
                                });

                                EndDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                await SchedulerHelpers.LogApiRequest(item.docNo, path, ResultASN, SchedulerHelpers.IP, StartDateTime, EndDateTime, _UpdateQuery.Replace("@DocNo", item.docNo), "", "", "", SchedulerHelpers.User, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), stoppingToken);
                            }

                            catch (Exception ex)
                            {
                                EndDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                await SchedulerHelpers.LogApiRequest(item.docNo, path, ResultASN, SchedulerHelpers.IP, StartDateTime, EndDateTime, _UpdateQuery.Replace("@DocNo", item.docNo), CleaningData.CutdownException(ex), "", "", SchedulerHelpers.User, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), stoppingToken);
                                await CreateLog.UpdateErrLogAsync(ex.ToString() + " DocNo:" + item.docNo);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                ResultASN = "Failed";
                EndDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                await SchedulerHelpers.LogApiRequest(key, path, ResultASN, SchedulerHelpers.IP, StartDateTime, EndDateTime, query1, CleaningData.CutdownException(ex), "", "", SchedulerHelpers.User, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), stoppingToken);
                await CreateLog.UpdateErrLogAsync(ex.ToString() + " DocNo:" + key);
            }
        }

        private static async Task<(string ResultASN, string EndDateTime)> SendToApi(string path, string ResultASN, string StartDateTime, string EndDateTime, string query2, SqlConnection _conn, WH_ASN.MainASNOut item, CancellationToken stoppingToken)
        {
            try
            {

                IEnumerable<WH_ASN.Detail> details = await _conn.QueryAsync<WH_ASN.Detail>(new CommandDefinition(query2, new { DocNo = item.docNo }, cancellationToken: stoppingToken));
                WH_ASN.MainASN _main = new WH_ASN.MainASN(item);

                _main.details = details;
                //var temp = Convert.ToDateTime(_main.receivedTime);
                //_main.receivedTime = temp.ToString("yyyy-MM-dd HH:mm:ss");

                ///for if the receipt need to be seperated by details
                //foreach (var data in _detailData)
                //{
                //        WH_ASN.ReciptASNOut _recipt = new WH_ASN.ReciptASNOut
                //        {
                //            PONumber = item.userDefine1,
                //            LineNumber = data.lineNo,
                //            ItemNumber = data.sku,
                //            LotNumber = data.lotAtt04,
                //            MfgDate = data.lotAtt01,
                //            ExpirationDate = data.lotAtt02,
                //            InvoiceNumber = data.userDefine2,
                //            Container = data.userDefine3,
                //            ArrivalDate = item.receivedTime,
                //            ReceivedQty = data.receivedQty,
                //            Condition = data.lotAtt08,
                //            ReceiptDate = data.lotAtt03
                //        };


                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(item.url);
                    httpClient.DefaultRequestHeaders.Add("Token", Token_ReceiptASN);



                    //HTTP POST
                    // string lastPart = item.url.Split('/').Last();
                    var x = JsonConvert.SerializeObject(_main);
                    var content = new StringContent(x, Encoding.UTF8, "appilcation/json");
                    //ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    var postTask = await httpClient.PostAsync(item.url, content, stoppingToken);

                    //var httpResponse = await httpClient.SendAsync(httpRequest).ConfigureAwait(true);
                    //var _responseString = httpResponse.Content.ReadAsStringAsync().Result; //To get the response
                    var result = postTask;
                    var data = result.Content.ReadAsStringAsync(stoppingToken);
                    string file = await data/*.Substring(1, data.Result.Length - 2)*/;
                    if (result.IsSuccessStatusCode)
                    {
                        ResultASN = "Success";
                        EndDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        await SchedulerHelpers.LogApiRequest(item.docNo, path, ResultASN, SchedulerHelpers.IP, StartDateTime, EndDateTime, JsonConvert.SerializeObject(_main), file, "", "", SchedulerHelpers.User, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), stoppingToken);

                    }
                    else
                    {
                        ResultASN = "Failed";
                        EndDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        await SchedulerHelpers.LogApiRequest(item.docNo, path, ResultASN, SchedulerHelpers.IP, StartDateTime, EndDateTime, JsonConvert.SerializeObject(_main), file, "Client status: " + result.StatusCode.ToString(), "", SchedulerHelpers.User, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), stoppingToken);
                    }
                    //}
                }
            }

            catch (Exception ex)
            {
                ResultASN = "Failed";
                EndDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                await SchedulerHelpers.LogApiRequest(item.docNo, path, ResultASN, SchedulerHelpers.IP, StartDateTime, EndDateTime, query2.Replace("@DocNo", item.docNo), CleaningData.CutdownException(ex), "", "", SchedulerHelpers.User, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), stoppingToken);
                await CreateLog.UpdateErrLogAsync(ex.ToString() + " DocNo:" + item.docNo);

            }

            return (ResultASN, EndDateTime);
        }
    }
}
