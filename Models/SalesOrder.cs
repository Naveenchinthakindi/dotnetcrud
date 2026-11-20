namespace StudentApi.Models
{
    public class SalesOrderResponse
    {
        public int DocEntry { get; set; }
        public int DocNum { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerRefNo { get; set; } = string.Empty;
        public string DocDate { get; set; } = string.Empty;
        public string DocDueDate { get; set; } = string.Empty;
        public decimal DocTotal { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string DocStatus { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
    }

    public class SalesOrderDetailResponse : SalesOrderResponse
    {
        public List<SalesOrderLineResponse> Lines { get; set; } = new();
    }

    public class SalesOrderLineResponse
    {
        public int LineNum { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemDescription { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal OpenQuantity { get; set; }
        public decimal Price { get; set; }
        public decimal LineTotal { get; set; }
        public string WarehouseCode { get; set; } = string.Empty;
        public string LineStatus { get; set; } = string.Empty;
    }
}
