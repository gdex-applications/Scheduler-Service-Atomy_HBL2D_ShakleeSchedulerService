namespace AtmyHbl2dShkleBrnCdSchedulerService.Models
{
    public class WH_ASN
    {

        public class MainASN
        {
            public MainASN()
            {
            }

            public MainASN(MainASN main)
            {
                warehouseId = main.warehouseId;
                customerId = main.customerId;
                docNo = main.docNo;
                docType = main.docType;
                receivedTime = main.receivedTime;
                userDefine1 = main.userDefine1;
                userDefine2 = main.userDefine2;
                userDefine3 = main.userDefine3;
                userDefine4 = main.userDefine4;
                userDefine5 = main.userDefine5;
                userDefine6 = main.userDefine6;
                details = main.details;
            }

            public string warehouseId { get; set; } = "";
            public string customerId { get; set; } = "";
            public string docNo { get; set; } = "";
            public string docType { get; set; } = "";
            public string receivedTime { get; set; } = "";
            public string userDefine1 { get; set; } = "";
            public string userDefine2 { get; set; } = "";
            public string userDefine3 { get; set; } = "";
            public string userDefine4 { get; set; } = "";
            public string userDefine5 { get; set; } = "";
            public string userDefine6 { get; set; } = "";
            public IEnumerable<Detail> details { get; set; }

        }

        public class Detail
        {
            public Detail()
            {
            }

            public Detail(Detail detail)
            {
                lineNo = detail.lineNo;
                sku = detail.sku;
                receivedQty = detail.receivedQty;
                lotAtt01 = detail.lotAtt01;
                lotAtt02 = detail.lotAtt02;
                lotAtt03 = detail.lotAtt03;
                lotAtt04 = detail.lotAtt04;
                lotAtt05 = detail.lotAtt05;
                lotAtt06 = detail.lotAtt06;
                lotAtt07 = detail.lotAtt07;
                lotAtt08 = detail.lotAtt08;
                lotAtt09 = detail.lotAtt09;
                lotAtt10 = detail.lotAtt10;
                lotAtt11 = detail.lotAtt11;
                lotAtt12 = detail.lotAtt12;
                lotAtt13 = detail.lotAtt13;
                lotAtt14 = detail.lotAtt14;
                lotAtt15 = detail.lotAtt15;
                lotAtt16 = detail.lotAtt16;
                lotAtt17 = detail.lotAtt17;
                lotAtt18 = detail.lotAtt18;
                lotAtt19 = detail.lotAtt19;
                lotAtt20 = detail.lotAtt20;
                lotAtt21 = detail.lotAtt21;
                lotAtt22 = detail.lotAtt22;
                lotAtt23 = detail.lotAtt23;
                lotAtt24 = detail.lotAtt24;
                userDefine1 = detail.userDefine1;
                userDefine2 = detail.userDefine2;
                userDefine3 = detail.userDefine3;
                userDefine4 = detail.userDefine4;
                userDefine5 = detail.userDefine5;
                userDefine6 = detail.userDefine6;
            }

            public string lineNo { get; set; } = "";
            public string sku { get; set; } = "";
            public decimal receivedQty { get; set; } = 0;
            public string lotAtt01 { get; set; } = "";
            public string lotAtt02 { get; set; } = "";
            public string lotAtt03 { get; set; } = "";
            public string lotAtt04 { get; set; } = "";
            public string lotAtt05 { get; set; } = "";
            public string lotAtt06 { get; set; } = "";
            public string lotAtt07 { get; set; } = "";
            public string lotAtt08 { get; set; } = "";
            public string lotAtt09 { get; set; } = "";
            public string lotAtt10 { get; set; } = "";
            public string lotAtt11 { get; set; } = "";
            public string lotAtt12 { get; set; } = "";
            public string lotAtt13 { get; set; } = "";
            public string lotAtt14 { get; set; } = "";
            public string lotAtt15 { get; set; } = "";
            public string lotAtt16 { get; set; } = "";
            public string lotAtt17 { get; set; } = "";
            public string lotAtt18 { get; set; } = "";
            public string lotAtt19 { get; set; } = "";
            public string lotAtt20 { get; set; } = "";
            public string lotAtt21 { get; set; } = "";
            public string lotAtt22 { get; set; } = "";
            public string lotAtt23 { get; set; } = "";
            public string lotAtt24 { get; set; } = "";
            public string userDefine1 { get; set; } = "";
            public string userDefine2 { get; set; } = "";
            public string userDefine3 { get; set; } = "";
            public string userDefine4 { get; set; } = "";
            public string userDefine5 { get; set; } = "";
            public string userDefine6 { get; set; } = "";
        }







        public class MainASNOut : MainASN
        {
            public List<Detail> DetailsOut { get; set; }
            public string url { get; set; }
            public long Id { get; set; }
        }


        public class ReciptASNOut
        {
            public string PONumber { get; set; }
            public int LineNumber { get; set; }
            public string TransType { get; set; } = "U";
            public string ReasonCode { get; set; } = "01";
            public string ItemNumber { get; set; }
            public string UnitOfMeasure { get; set; } = "EA";
            public string Warehouse { get; set; } = "M56";
            public string Location { get; set; } = "1";
            public string LotNumber { get; set; }
            public string MfgDate { get; set; }
            public string ExpirationDate { get; set; }
            public string InvoiceNumber { get; set; }
            public string Container { get; set; }
            public string ArrivalDate { get; set; }
            public string ReceivedQty { get; set; }
            public string Condition { get; set; }
            public string ReceiptDate { get; set; }
        }


        //public class Rootobject
        //{
        //    public Data data { get; set; }
        //}

        //public class Data
        //{
        //    public Header[] header { get; set; }
        //}

        //public class Header
        //{
        //    public string supplierStreet { get; set; }
        //    public string supplierCountry { get; set; }
        //    public string notes { get; set; }
        //    public string userDefine7 { get; set; }
        //    public string userDefine6 { get; set; }
        //    public string userDefine9 { get; set; }
        //    public string userDefine8 { get; set; }
        //    public string userDefine10 { get; set; }
        //    public string asnCreationTime { get; set; }
        //    public string userDefine1 { get; set; }
        //    public string carrierName { get; set; }
        //    public string userDefine3 { get; set; }
        //    public string supplierDistrict { get; set; }
        //    public string userDefine2 { get; set; }
        //    public string crossdockFlag { get; set; }
        //    public string userDefine5 { get; set; }
        //    public string userDefine4 { get; set; }
        //    public Detail[] details { get; set; }
        //    public string docNo { get; set; }
        //    public string priority { get; set; }
        //    public string warehouseId { get; set; }
        //    public string supplierMail { get; set; }
        //    public string placeOfLoading { get; set; }
        //    public string supplierAddress1 { get; set; }
        //    public string supplierAddress3 { get; set; }
        //    public string supplierAddress2 { get; set; }
        //    public string supplierAddress4 { get; set; }
        //    public string supplierId { get; set; }
        //    public string supplierProvince { get; set; }
        //    public string supplierCity { get; set; }
        //    public string followUp { get; set; }
        //    public string placeOfDelivery { get; set; }
        //    public string hedi10 { get; set; }
        //    public string customerId { get; set; }
        //    public string expectedArriveTime1 { get; set; }
        //    public string expectedArriveTime2 { get; set; }
        //    public string supplierName { get; set; }
        //    public string asnReferenceC { get; set; }
        //    public string supplierFax { get; set; }
        //    public string asnReferenceD { get; set; }
        //    public string asnReferenceA { get; set; }
        //    public string asnReferenceB { get; set; }
        //    public string supplierZip { get; set; }
        //    public string countryOfDestination { get; set; }
        //    public string placeOfDischarge { get; set; }
        //    public string hedi04 { get; set; }
        //    public string hedi05 { get; set; }
        //    public string hedi02 { get; set; }
        //    public string hedi03 { get; set; }
        //    public string hedi01 { get; set; }
        //    public string asnType { get; set; }
        //    public string supplierTel2 { get; set; }
        //    public string supplierTel1 { get; set; }
        //    public string countryOfOrigin { get; set; }
        //    public string carrierId { get; set; }
        //    public string hedi08 { get; set; }
        //    public string supplierContact { get; set; }
        //    public string hedi09 { get; set; }
        //    public string hedi06 { get; set; }
        //    public string hedi07 { get; set; }
        //}

        //public class Detail
        //{
        //    public string notes { get; set; }
        //    public string dedi11 { get; set; }
        //    public string dedi12 { get; set; }
        //    public string dedi10 { get; set; }
        //    public string dedi15 { get; set; }
        //    public string dedi16 { get; set; }
        //    public string userDefine6 { get; set; }
        //    public string dedi13 { get; set; }
        //    public string dedi14 { get; set; }
        //    public string userDefine1 { get; set; }
        //    public string lineNo { get; set; }
        //    public string userDefine3 { get; set; }
        //    public string userDefine2 { get; set; }
        //    public string lotAtt09 { get; set; }
        //    public string userDefine5 { get; set; }
        //    public string lotAtt08 { get; set; }
        //    public string userDefine4 { get; set; }
        //    public string lotAtt07 { get; set; }
        //    public string lotAtt06 { get; set; }
        //    public string lotAtt05 { get; set; }
        //    public string lotAtt04 { get; set; }
        //    public string sku { get; set; }
        //    public string lotAtt03 { get; set; }
        //    public string lotAtt02 { get; set; }
        //    public string lotAtt01 { get; set; }
        //    public string lotAtt10 { get; set; }
        //    public string dedi04 { get; set; }
        //    public string dedi05 { get; set; }
        //    public string dedi08 { get; set; }
        //    public string dedi09 { get; set; }
        //    public string dedi06 { get; set; }
        //    public string dedi07 { get; set; }
        //    public string lotAtt19 { get; set; }
        //    public string lotAtt18 { get; set; }
        //    public string lotAtt17 { get; set; }
        //    public string lotAtt16 { get; set; }
        //    public string lotAtt15 { get; set; }
        //    public string lotAtt14 { get; set; }
        //    public string lotAtt13 { get; set; }
        //    public string lotAtt12 { get; set; }
        //    public string lotAtt11 { get; set; }
        //    public string lotAtt21 { get; set; }
        //    public string lotAtt20 { get; set; }
        //    public string totalPrice { get; set; }
        //    public string lotAtt24 { get; set; }
        //    public string lotAtt23 { get; set; }
        //    public string lotAtt22 { get; set; }
        //    public string referenceNo { get; set; }
        //    public string expectedQty { get; set; }
        //}


    }
}