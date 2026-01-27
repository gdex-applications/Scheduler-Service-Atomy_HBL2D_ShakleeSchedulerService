using Atomy_HBL2DBcode_ShakleeSchedulerService.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace Atomy_HBL2DBcode_ShakleeSchedulerService.Functions.AtomyHLSAPI
{
    internal class AtomyHLSAPIScheduler
    {
        private readonly IConfiguration _configuration;
        private static string HLSSendOrderURL = "";

        public AtomyHLSAPIScheduler(IConfiguration configuration)
        {
            _configuration = configuration;
            SchedulerHelpers.conn = SchedulerHelpers.extConfiguration!.ConnectionStrings.ConnectionStrings["GWAPI"].ConnectionString!;
            SchedulerHelpers.WebhookInstances = int.Parse(SchedulerHelpers.extConfiguration!.AppSettings.Settings["app:webhook_instances"].Value!);
            SchedulerHelpers.Retries = _configuration.GetValue<int>("Retries")!;


            HLSSendOrderURL = SchedulerHelpers.extConfiguration!.AppSettings.Settings["HLSURL_SO"].Value!;
        }
        public static async Task ProcessQueuedAtomyHLSAPI_Outbound(CancellationToken stoppingToken)
        {
            try
            {
                using SqlConnection con = new SqlConnection(SchedulerHelpers.conn);

                string StartPush = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string EndPush = string.Empty;
                EndPush = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                await SchedulerHelpers.LogApiRequest("AtomyQueued", "ATOMYPQueued", "Success", SchedulerHelpers.IP, StartPush, EndPush, "", "Start running Atomy Queued", "Start Select Queued", "", SchedulerHelpers.User, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), stoppingToken);


                //Get all header data that need to be push to API
                IEnumerable<Header> _ttlHeader = await con.QueryAsync<Header>(new CommandDefinition("SELECT * FROM [WHMiddleware].[dbo].[Header] with(NOLOCK) WHERE customerId = 'ATOMY' and OrderStatus = 'Queued' ", cancellationToken: stoppingToken));
                _ttlHeader = _ttlHeader.GroupBy(x => x.docNo).Select(y => y.First());
                int _processedItems = 0;

                while (_processedItems < _ttlHeader.Count())
                {
                    var _items = _ttlHeader.Skip(_processedItems).Take(SchedulerHelpers.WebhookInstances);
                    var count = _items.Count();
                    var tries = 0;
                    while (tries < SchedulerHelpers.Retries && _items.Any())
                    {
                        if (tries > 0)
                        {
                            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                        }
                        var postTasks = _items.Select(async _header => await AtomyHLSAPIAsync_Outbound(_header, stoppingToken));
                        _items = (await Task.WhenAll(postTasks)).Where(i => i != null);
                        tries++;
                    }

                    //Parallel.ForEach(_items, async _header => await AtomyHLSAPIAsync_Outbound(_header));
                    //Parallel.ForEach(_items, async _header => await GdexProcessAtomyQueued(_header));


                    _processedItems += count;
                }

                EndPush = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                await SchedulerHelpers.LogApiRequest("AtomyQueued", "ATOMYPQueued", "Success", SchedulerHelpers.IP, StartPush, EndPush, "", "Finish running Atomy Queued", "Success Select Queued", "", SchedulerHelpers.User, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), stoppingToken);



            }
            catch (Exception ex)
            {
                await SchedulerHelpers.LogApiRequest("AtomyQueued", "ATOMYPQueued", "Failed", SchedulerHelpers.IP, "", "", "", "Error-Select Atomy Queued : " + CleaningData.CutdownException(ex), "Failed to Select", "", SchedulerHelpers.User, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), stoppingToken);

                await CreateLog.UpdateErrLogAsync(ex.ToString());
            }
        }

        public static async Task<Header> AtomyHLSAPIAsync_Outbound(Header _dataHeader, CancellationToken stoppingToken)
        {
            string key = string.Empty;
            string FileName = string.Empty;
            string _responseString = string.Empty;
            string StartPush = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string EndFullTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string AtomyPaymentTerms = "NCOD";
            GenericResponse _result = new GenericResponse();
            HttpResponseMessage _response = new HttpResponseMessage();
            string jsonString = string.Empty;
            using SqlConnection _conn = new SqlConnection(SchedulerHelpers.conn);


            try
            {
                key = _dataHeader.docNo;
                FileName = _dataHeader.FileName ?? "";
                //Add invoicesitem to api content
                IEnumerable<InvoiceItem> _ttlInvoiceItem = await _conn.QueryAsync<InvoiceItem>(new CommandDefinition($"SELECT * FROM [WHMiddleware].[dbo].[InvoicesItem] with (NOLOCK) WHERE DocNo = '{_dataHeader.docNo}' ", cancellationToken: stoppingToken));



                //Add invoice to api content
                List<Invoice> _ttlInvoice = [.. await _conn.QueryAsync<Invoice>(new CommandDefinition($"SELECT * FROM [WHMiddleware].[dbo].[Invoices] with (NOLOCK) WHERE DocNo = '{_dataHeader.docNo}' ", cancellationToken: stoppingToken))];
                if (_ttlInvoiceItem.Any())
                {
                    _ttlInvoice.ForEach(s => s.invoicesItem = _ttlInvoiceItem);

                }

                //Add details to api content
                IEnumerable<Details> _ttlDetails = await _conn.QueryAsync<Details>(new CommandDefinition($"SELECT * FROM [WHMiddleware].[dbo].[Details] with (NOLOCK) WHERE DocNo = '{_dataHeader.docNo}' ", cancellationToken: stoppingToken));



                //Add header to api content
                List<Header> _header = new List<Header>()
                        {
                        new Header {
                            warehouseId             = _dataHeader.warehouseId,
                            customerId              = _dataHeader.customerId,
                            orderType               = _dataHeader.orderType,
                            docNo                   = _dataHeader.docNo,
                            soReferenceA            = _dataHeader.soReferenceA,
                            soReferenceB            = _dataHeader.soReferenceB,
                            soReferenceC            = _dataHeader.soReferenceC,
                            soReferenceD            = _dataHeader.soReferenceD,
                            orderTime               = DateTime.ParseExact(_dataHeader.orderTime, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss"),
                            expectedShipmentTime1   = _dataHeader.expectedShipmentTime1,
                            requiredDeliveryTime    = _dataHeader.requiredDeliveryTime,
                            deliveryNo              = _dataHeader.deliveryNo,
                            consigneeId             = _dataHeader.consigneeId,
                            consigneeName           = _dataHeader.consigneeName,
                            consigneeAddress1       = _dataHeader.consigneeAddress1,
                            consigneeAddress2       = _dataHeader.consigneeAddress2,
                            consigneeAddress3       = _dataHeader.consigneeAddress3,
                            consigneeCountry        = _dataHeader.consigneeCountry,
                            consigneeProvince       = _dataHeader.consigneeProvince,
                            consigneeCity           = _dataHeader.consigneeCity,
                            consigneeMail           = _dataHeader.consigneeMail,
                            consigneeTel1           = _dataHeader.consigneeTel1,
                            consigneeTel2           = _dataHeader.consigneeTel2,
                            consigneeZip            = _dataHeader.consigneeZip,
                            carrierId               = _dataHeader.carrierId,
                            carrierName             = _dataHeader.carrierName,
                            carrierFax              = _dataHeader.carrierFax,
                            carrierMail             = _dataHeader.carrierMail,
                            channel                 = _dataHeader.channel,
                            hedi01                  = _dataHeader.hedi01,
                            hedi02                  = _dataHeader.hedi02,
                            hedi03                  = _dataHeader.hedi03,
                            hedi04                  = _dataHeader.hedi04,
                            hedi05                  = _dataHeader.hedi05,
                            hedi06                  = _dataHeader.hedi06,
                            hedi07                  = _dataHeader.hedi07,
                            hedi08                  = _dataHeader.hedi08,
                            hedi09                  = _dataHeader.hedi09,
                            hedi10                  = _dataHeader.hedi10,
                            invoicePrintFlag        = _dataHeader.invoicePrintFlag,
                            route                   = _dataHeader.route,
                            stop                    = _dataHeader.stop,
                            issueParty              = _dataHeader.issueParty,
                            userDefine1             =  AtomyPaymentTerms,  //_dataHeader.userDefine1,
                            userDefine2             = _dataHeader.userDefine2,
                            userDefine3             = _dataHeader.userDefine3,
                            userDefine4             = _dataHeader.userDefine4,
                            userDefine5             = _dataHeader.userDefine5,
                            userDefine6             = _dataHeader.userDefine6,
                            notes                   = _dataHeader.notes,
                            details                 = _ttlDetails.Any() ? _ttlDetails :[],
                            invoices                = _ttlInvoice.Count > 0 ? _ttlInvoice :[]
                        }
                    };


                var _requestBodyData = new { xmldata = new { header = _header } };
                jsonString = JsonConvert.SerializeObject(_requestBodyData);

                using var httpClient = new HttpClient();
                try
                {
                    StartPush = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    //httpClient.BaseAddress = new Uri(url.ToString());
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                    HttpRequestMessage httpRequest = new(HttpMethod.Post, HLSSendOrderURL);

                    StringBuilder data = new StringBuilder();
                    data.Append(HttpUtility.UrlEncode("messageid")); data.Append('='); data.Append(HttpUtility.UrlEncode("SO"));
                    data.Append('&');
                    data.Append(HttpUtility.UrlEncode("data")); data.Append('='); data.Append(HttpUtility.UrlEncode(jsonString));

                    httpRequest.Content = new StringContent(data.ToString(), Encoding.UTF8, "application/x-www-form-urlencoded");
                    using HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequest, stoppingToken).ConfigureAwait(true);
                    //WriteToLog.Logging(httpResponse.Content.ReadAsStringAsync().Result);
                    _responseString = await httpResponse.Content.ReadAsStringAsync(stoppingToken);
                    EndFullTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    GenericResponse _responseOrder = JsonConvert.DeserializeObject<GenericResponse>(_responseString)!;

                    if (httpResponse.IsSuccessStatusCode && httpResponse.ReasonPhrase == "OK")
                    {
                        //Update [WHMiddleware].[dbo].[Header]
                        if (_responseOrder.Response.Return.returnFlag == "1")
                        {
                            string _updateSuccess = $"UPDATE [WHMiddleware].[dbo].[Header] SET OrderStatus = 'Success' WHERE DocNo = '{_dataHeader.docNo}'";
                            await _conn.ExecuteAsync(new CommandDefinition(_updateSuccess, cancellationToken: stoppingToken));

                            await SchedulerHelpers.LogApiRequest(_dataHeader.docNo, "AtomyHLSAPI", "Success", SchedulerHelpers.IP, StartPush, EndFullTime, jsonString, _responseString, "", FileName, SchedulerHelpers.User, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), stoppingToken);
                        }
                        else if (_responseOrder.Response.Return.returnFlag == "0")
                        {
                            if (_responseOrder.Response.Return.returnCode == "ED0345")
                            {
                                string _updateSuccess = $"UPDATE [WHMiddleware].[dbo].[Header] SET OrderStatus = 'Success' WHERE DocNo = '{_dataHeader.docNo}'";
                                await _conn.ExecuteAsync(new CommandDefinition(_updateSuccess, cancellationToken: stoppingToken));

                                await SchedulerHelpers.LogApiRequest(_dataHeader.docNo, "AtomyHLSAPI", "Success", SchedulerHelpers.IP, StartPush, EndFullTime, jsonString, _responseString, "", FileName, SchedulerHelpers.User, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), stoppingToken);


                            }
                            else
                            {
                                string _updateSuccess = $"UPDATE [WHMiddleware].[dbo].[Header] SET OrderStatus = 'Failed' WHERE DocNo = '{_dataHeader.docNo}'";
                                await _conn.ExecuteAsync(new CommandDefinition(_updateSuccess, cancellationToken: stoppingToken));

                                await SchedulerHelpers.LogApiRequest(_dataHeader.docNo, "AtomyHLSAPI", "Failed", SchedulerHelpers.IP, StartPush, EndFullTime, jsonString, _responseString, "", FileName, SchedulerHelpers.User, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), stoppingToken);

                            }
                        }
                    }
                    else
                    {
                        string _updateSuccess = $"UPDATE [WHMiddleware].[dbo].[Header] SET OrderStatus = 'Pending' WHERE DocNo = '{_dataHeader.docNo}'";
                        await _conn.ExecuteAsync(new CommandDefinition(_updateSuccess, cancellationToken: stoppingToken));

                        await SchedulerHelpers.LogApiRequest(_dataHeader.docNo, "AtomyHLSAPI", "Failed", SchedulerHelpers.IP, StartPush, EndFullTime, jsonString, "No StatusCode : " + httpResponse.Content.ToString(), "Pending Repush", FileName, SchedulerHelpers.User, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), stoppingToken);
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    await CreateLog.UpdateErrLogAsync(ex.ToString() + " DocNo:" + _dataHeader.docNo);
                    string _updateSuccess = $"UPDATE [WHMiddleware].[dbo].[Header] SET OrderStatus = 'Pending' WHERE DocNo = '{_dataHeader.docNo}'";
                    await _conn.ExecuteAsync(new CommandDefinition(_updateSuccess, cancellationToken: stoppingToken));

                    await SchedulerHelpers.LogApiRequest(_dataHeader.docNo, "AtomyHLSAPI", "Failed", SchedulerHelpers.IP, StartPush, EndFullTime, jsonString, "Error-CallHLSURL : " + CleaningData.CutdownException(ex) + " " + _responseString, "Pending Repush", FileName, SchedulerHelpers.User, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), stoppingToken);
                    return _dataHeader;
                }

            }
            catch (Exception ex)
            {
                await CreateLog.UpdateErrLogAsync(ex.ToString() + " DocNo:" + key);
                string _updateSuccess = $"UPDATE [WHMiddleware].[dbo].[Header] SET OrderStatus = 'Pending' WHERE DocNo = '{key}'";
                await _conn.ExecuteAsync(new CommandDefinition(_updateSuccess, cancellationToken: stoppingToken));

                await SchedulerHelpers.LogApiRequest(key, "AtomyHLSAPI", "Failed", SchedulerHelpers.IP, StartPush, EndFullTime, jsonString, "Error-Select GdexDB Table : " + CleaningData.CutdownException(ex), "Pending Repush", FileName, SchedulerHelpers.User, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), stoppingToken);
                return _dataHeader;
            }

        }

    }
}
