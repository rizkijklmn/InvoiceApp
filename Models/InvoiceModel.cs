using System.ComponentModel.DataAnnotations;
using InvoiceApp.db;
using Microsoft.EntityFrameworkCore;
namespace InvoiceApp.Models
{
    public class MsCourier
    {
        [Key]
        public int CourierID { get; set; }
        public string? CourierName { get; set; }
    }

    [Keyless]
    public class ltCourierFee
    {
        public int WeightID { get; set; }
        public int CourierID { get; set; }
        public int StartKg { get; set; }
        public int EndKg { get; set; }
        public decimal Price { get; set; }
    }

    public class MsPayment
    {
        [Key]
        public int PaymentID { get; set; }
        public string? PaymentName { get; set; }
    }

    public class MsProduct
    {
        [Key]
        public int ProductID { get; set; }
        public string? ProductName { get; set; }
        public double Weight { get; set; }
        public decimal Price { get; set; }
    }

    public class MsSales
    {
        [Key]
        public int SalesID { get; set; }
        public string? SalesName { get; set; }
    }

    public class TrInvoice
    {
        [Key]
        public string? InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string? InvoiceTo { get; set; }
        public string? ShipTo { get; set; }
        public int SalesID { get; set; }
        public int CourierID { get; set; }
        public int PaymentType { get; set; }
        public decimal CourierFee { get; set; }
    }

    [Keyless]
    public class TrInvoiceDetail
    {
        public string? InvoiceNo { get; set; }
        public int ProductID { get; set; }
        public double Weight { get; set; }
        public short Qty { get; set; }
        public decimal Price { get; set; }

        // Method untuk mendapatkan data detail invoice dengan join ke MsProduct
        public static async Task<List<InvoiceViewModel>> GetInvoiceDetailsAsync(AppDbContext context, string? invoiceNo = null)
        {
            string sql = @"
        SELECT ti.InvoiceNo, m.ProductID , m.ProductName, t.Weight, t.Qty, t.Price AS Price, (t.Qty * t.Price) AS Total
        FROM assessmentdb.dbo.trinvoicedetail t
        LEFT JOIN assessmentdb.dbo.msproduct m ON m.ProductID = t.ProductID
        LEFT JOIN assessmentdb.dbo.trinvoice ti ON ti.InvoiceNo = t.InvoiceNo 
        LEFT JOIN assessmentdb.dbo.ltcourierfee lt on lt.CourierID = ti.CourierID
    ";

            if (!string.IsNullOrEmpty(invoiceNo))
            {
                sql += " WHERE t.InvoiceNo = {0}";
            }

            sql += @"
        GROUP BY ti.InvoiceNo, m.ProductID, m.ProductName, t.Weight, t.Price, t.Qty
    ";

            return await context.Set<InvoiceViewModel>()
                .FromSqlRaw(sql, invoiceNo ?? (object)DBNull.Value)
                .ToListAsync();
        }

    }

    // ViewModel untuk menampung hasil query
    public class InvoiceViewModel
    {
        public string? InvoiceNo { get; set; }
        public int ProductID { get; set; }
        public string? ProductName { get; set; }
        public double Weight { get; set; }
        public short Qty { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
    }

    public class InvoiceEditViewModel
    {
        public string? InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string? InvoiceTo { get; set; }
        public string? ShipTo { get; set; }
        public int SalesID { get; set; }
        public int CourierID { get; set; }
        public int PaymentType { get; set; }
        public decimal CourierFee { get; set; }

        public List<InvoiceViewModel> Details { get; set; } = new();
    }

}

