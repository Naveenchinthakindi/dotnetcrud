namespace StudentApi.Models
{
    public class PurchaseOrderResponse
    {
        public int DocEntry { get; set; }
        public int DocNum { get; set; }
        public string VendorCode { get; set; } = string.Empty;
        public string VendorName { get; set; } = string.Empty;
        public string DocDate { get; set; } = string.Empty;
        public string DocDueDate { get; set; } = string.Empty;
        public decimal DocTotal { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string DocStatus { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
    }

    public class PurchaseOrderDetailResponse : PurchaseOrderResponse
    {
        public List<PurchaseOrderLineResponse> Lines { get; set; } = new();
    }

    public class PurchaseOrderLineResponse
    {
        public int LineNum { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemDescription { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal OpenQuantity { get; set; }
        public decimal Price { get; set; }
        public decimal LineTotal { get; set; }
        public string WarehouseCode { get; set; } = string.Empty;
    }
}
