namespace StudentApi.Models
{
    public class ProductionOrderResponse
    {
        public int DocEntry { get; set; }
        public int DocNum { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal PlannedQuantity { get; set; }
        public decimal CompletedQuantity { get; set; }
        public decimal RejectedQuantity { get; set; }
        public decimal OpenQuantity { get; set; }
        public string PostDate { get; set; } = string.Empty;
        public string DueDate { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string Warehouse { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
    }

    public class ProductionOrderDetailResponse : ProductionOrderResponse
    {
        public List<ProductionOrderLineResponse> Lines { get; set; } = new();
    }

    public class ProductionOrderLineResponse
    {
        public int LineNum { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal BaseQuantity { get; set; }
        public decimal PlannedQuantity { get; set; }
        public decimal IssuedQuantity { get; set; }
        public decimal OpenQuantity { get; set; }
        public string Warehouse { get; set; } = string.Empty;
        public string IssueType { get; set; } = string.Empty;
    }
}
