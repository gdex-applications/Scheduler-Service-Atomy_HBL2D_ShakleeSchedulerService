using System.ComponentModel.DataAnnotations.Schema;

namespace AtmyHbl2dShkleBrnCdSchedulerService.Models
{
    public class Header
    {
        public string warehouseId { get; set; } = "";
        public string customerId { get; set; } = "";
        public string orderType { get; set; } = "";

        [Column(TypeName = "varchar")]
        public string docNo { get; set; } = "";
        public string soReferenceA { get; set; } = "";

        [Column(TypeName = "varchar")]
        public string soReferenceB { get; set; } = "";
        public string soReferenceC { get; set; } = "";
        public string soReferenceD { get; set; } = "";
        public string orderTime { get; set; } = "";
        public string expectedShipmentTime1 { get; set; } = "";
        public string requiredDeliveryTime { get; set; } = "";
        public string deliveryNo { get; set; } = "";

        [Column(TypeName = "varchar")]
        public string consigneeId { get; set; } = "";

        [Column(TypeName = "varchar")]
        public string consigneeName { get; set; } = "";

        [Column(TypeName = "varchar")]
        public string consigneeAddress1 { get; set; } = "";

        [Column(TypeName = "varchar")]
        public string consigneeAddress2 { get; set; } = "";

        [Column(TypeName = "varchar")]
        public string consigneeAddress3 { get; set; } = "";

        public string consigneeCountry { get; set; } = "";

        [Column(TypeName = "varchar")]
        public string consigneeProvince { get; set; } = "";

        [Column(TypeName = "varchar")]
        public string consigneeCity { get; set; } = "";
        public string consigneeMail { get; set; } = "";

        [Column(TypeName = "varchar")]
        public string consigneeTel1 { get; set; } = "";
        public string consigneeTel2 { get; set; } = "";

        [Column(TypeName = "varchar")]
        public string consigneeZip { get; set; } = "";

        [Column(TypeName = "varchar")]
        public string carrierId { get; set; } = "";
        public string carrierName { get; set; } = "";
        public string carrierFax { get; set; } = "";
        public string carrierMail { get; set; } = "";
        public string channel { get; set; } = "";

        [Column(TypeName = "varchar")]
        public string hedi01 { get; set; } = "";
        public string hedi02 { get; set; } = "";
        public string hedi03 { get; set; } = "";
        public string hedi04 { get; set; } = "";
        public string hedi05 { get; set; } = "";
        public string hedi06 { get; set; } = "";
        public string hedi07 { get; set; } = "";
        public string hedi08 { get; set; } = "";
        public string hedi09 { get; set; } = "";
        public string hedi10 { get; set; } = "";
        public string invoicePrintFlag { get; set; } = "";
        public string route { get; set; } = "";
        public string stop { get; set; } = "";
        public string userDefine1 { get; set; } = "";
        public string userDefine2 { get; set; } = "";
        public string userDefine3 { get; set; } = "";
        public string userDefine4 { get; set; } = "";
        public string userDefine5 { get; set; } = "";
        public string userDefine6 { get; set; } = "";
        public string notes { get; set; } = "";

        [Column(TypeName = "varchar")]
        public string OrderID { get; set; } = "";
        public string issueParty { get; set; } = "";

        [Column(TypeName = "varchar")]
        public string? FileName { get; set; }
        public IEnumerable<Details> details { get; set; } = [];
        public IEnumerable<Invoice> invoices { get; set; } = [];
    }

    public class Details
    {
        public string referenceNo { get; set; } = "";
        public string lineNo { get; set; } = "";
        public string sku { get; set; } = "";
        public string qtyOrdered { get; set; } = "";
        public string price { get; set; } = "";
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
        public string dedi04 { get; set; } = "";
        public string dedi05 { get; set; } = "";
        public string dedi06 { get; set; } = "";
        public string dedi07 { get; set; } = "";
        public string dedi08 { get; set; } = "";
        public string dedi09 { get; set; } = "";
        public string dedi10 { get; set; } = "";
        public string dedi11 { get; set; } = "";
        public string dedi12 { get; set; } = "";
        public string dedi13 { get; set; } = "";
        public string dedi14 { get; set; } = "";
        public string dedi15 { get; set; } = "";
        public string dedi16 { get; set; } = "";
        public string userDefine1 { get; set; } = "";
        public string userDefine2 { get; set; } = "";
        public string userDefine3 { get; set; } = "";
        public string userDefine4 { get; set; } = "";
        public string userDefine5 { get; set; } = "";
        public string userDefine6 { get; set; } = "";
        public string notes { get; set; } = "";
        //public string OrderID { get; set; } = "";
    }
    public class Invoice
    {
        public string invoiceType { get; set; } = "";
        public string orderNo { get; set; } = "";
        public string title { get; set; } = "";
        public string vat { get; set; } = "";
        public string reference1 { get; set; } = "";
        public string reference2 { get; set; } = "";
        public string reference3 { get; set; } = "";
        public string invoiceBank { get; set; } = "";
        public string invoiceBankAccount { get; set; } = "";
        public string amount { get; set; } = "";
        public string userDefine1 { get; set; } = "";
        public string userDefine2 { get; set; } = "";
        public string userDefine3 { get; set; } = "";
        public string userDefine4 { get; set; } = "";
        public string userDefine5 { get; set; } = "";
        public string notes { get; set; } = "";
        //public string OrderID { get; set; } = "";
        public IEnumerable<InvoiceItem> invoicesItem { get; set; } = [];
    }

    public class InvoiceItem
    {
        public string lineNumber { get; set; } = "";
        public string sku { get; set; } = "";
        public string skuDescr { get; set; } = "";
        public string qty { get; set; } = "";
        public string uom { get; set; } = "";
        public string unitPrice { get; set; } = "";
        public string taxAmount { get; set; } = "";
        public string taxRate { get; set; } = "";
        public string userDefine1 { get; set; } = "";
        public string userDefine2 { get; set; } = "";
        public string userDefine3 { get; set; } = "";
        public string userDefine4 { get; set; } = "";
        public string userDefine5 { get; set; } = "";
        //public string OrderID { get; set; } = "";

    }
    public class orderIdWithLineSqlModel
    {
        public string SaleNum { get; set; } = "";
        public string Seq { get; set; } = "";
    }



    public class AtomyUpdateApiRespondModel
    {
        public int Status { get; set; }
        public string StatusDetailCode { get; set; }
        public object[] Data { get; set; }
    }



}
