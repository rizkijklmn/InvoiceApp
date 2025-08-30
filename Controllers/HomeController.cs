using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InvoiceApp.Models;
using InvoiceApp.db;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;

namespace InvoiceApp.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    // INDEX (lihat invoice)
    public async Task<IActionResult> Index(string? invoiceNo)
    {
        ViewData["invoiceNo"] = invoiceNo;

        // Detail invoice
        var invoiceList = await TrInvoiceDetail.GetInvoiceDetailsAsync(_context, invoiceNo);
        ViewData["invoiceList"] = invoiceList;

        // Ambil header
        var header = await _context.TrInvoice.FirstOrDefaultAsync(x => x.InvoiceNo == invoiceNo);
        ViewData["header"] = header;

        // Hitung summary
        if (invoiceList != null && invoiceList.Count > 0)
        {
            decimal subTotal = invoiceList.Sum(x => x.Total);

            // Hitung total berat barang
            double totalWeight = invoiceList.Sum(x => x.Weight * x.Qty);

            // Bulatkan ke bawah (floor)
            int weightForRule = (int)Math.Floor(totalWeight);

            // Ambil tarif kurir
            decimal courierFee = 0m;
            if (header != null)
            {
                var courierRule = await _context.ltCourierFee
                    .Where(f => f.CourierID == header.CourierID &&
                                weightForRule >= f.StartKg &&
                                (f.EndKg == null || weightForRule <= f.EndKg))
                    .FirstOrDefaultAsync();

                if (courierRule != null)
                {
                    courierFee = courierRule.Price * weightForRule;
                }
            }

            decimal grandTotal = subTotal + courierFee;

            // Simpan ke ViewData
            ViewData["totalWeight"] = totalWeight.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            ViewData["subTotal"] = subTotal.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
            ViewData["courierFee"] = courierFee.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
            ViewData["grandTotal"] = grandTotal.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        }
        else
        {
            ViewData["subTotal"] = 0m;
            ViewData["courierFee"] = 0m;
            ViewData["grandTotal"] = 0m;
        }

        // Dropdown data dengan selected value
        var salesList = await _context.MsSales.ToListAsync();
        var courierList = await _context.MsCourier.ToListAsync();
        var paymentList = await _context.MsPayment.ToListAsync();

        ViewBag.Sales = new SelectList(salesList, "SalesID", "SalesName", header?.SalesID);
        ViewBag.Couriers = new SelectList(courierList, "CourierID", "CourierName", header?.CourierID);
        ViewBag.Payments = new SelectList(paymentList, "PaymentID", "PaymentName", header?.PaymentType);

        return View();
    }

    // CREATE (GET)
    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Sales = new SelectList(_context.MsSales, "SalesID", "SalesName");
        ViewBag.Couriers = new SelectList(_context.MsCourier, "CourierID", "CourierName");
        ViewBag.Payments = new SelectList(_context.MsPayment, "PaymentID", "PaymentName");
        ViewBag.Products = new SelectList(_context.MsProduct, "ProductID", "ProductName");

        return View();
    }

    // CREATE (POST)
    [HttpPost]
    public async Task<IActionResult> Create(TrInvoice invoice, List<TrInvoiceDetail> details)
    {
        if (!ModelState.IsValid)
            return View(invoice);

        // pastikan details tidak null
        details ??= new List<TrInvoiceDetail>();

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            // Cek apakah invoice sudah ada
            bool invoiceExists = await _context.TrInvoice
                .AnyAsync(i => i.InvoiceNo == invoice.InvoiceNo);

            if (!invoiceExists)
            {
                invoice.CourierFee = invoice.CourierFee; 
                _context.TrInvoice.Add(invoice);
                await _context.SaveChangesAsync();
            }

            // SQL insert untuk TrInvoiceDetail
            var sql = @"
            INSERT INTO TrInvoiceDetail (InvoiceNo, ProductID, Price, Weight, Qty)
            VALUES (@invoiceNo, @productId, @price, @weight, @qty);
        ";

            foreach (var d in details)
            {
                // Ambil data product jika tersedia untuk mengisi berat/harga
                var product = await _context.MsProduct.FindAsync(d.ProductID);
                if (product != null)
                {
                    d.Weight = product.Weight;
                    d.Price = product.Price;
                }

                d.InvoiceNo = invoice.InvoiceNo;

                var parameters = new[]
                {
                new SqlParameter("@invoiceNo", d.InvoiceNo ?? (object)DBNull.Value),
                new SqlParameter("@productId", d.ProductID),
                new SqlParameter("@price", d.Price),
                new SqlParameter("@weight", d.Weight),
                new SqlParameter("@qty", d.Qty)
            };

                await _context.Database.ExecuteSqlRawAsync(sql, parameters);
            }

            await tx.CommitAsync();

            return RedirectToAction(nameof(Index), new { invoiceNo = invoice.InvoiceNo });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }


    [HttpGet]
    public async Task<IActionResult> Edit(string invoiceNo)
    {
        ModelState.Clear(); 
        var invoice = await _context.TrInvoice.FindAsync(invoiceNo);
        if (invoice == null) return NotFound();

        var details = await _context.TrInvoiceDetail
            .Where(x => x.InvoiceNo == invoiceNo)
            .ToListAsync();

        ViewData["details"] = details;
        ViewBag.Sales = new SelectList(_context.MsSales, "SalesID", "SalesName", invoice.SalesID);
        ViewBag.Couriers = new SelectList(_context.MsCourier, "CourierID", "CourierName", invoice.CourierID);
        ViewBag.Payments = new SelectList(_context.MsPayment, "PaymentID", "PaymentName", invoice.PaymentType);
        ViewBag.Products = new SelectList(_context.MsProduct, "ProductID", "ProductName");

        return View(invoice);
    }


    [HttpPost]
    public async Task<IActionResult> Edit(TrInvoice invoice, List<TrInvoiceDetail> details)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Sales = new SelectList(_context.MsSales, "SalesID", "SalesName", invoice.SalesID);
            ViewBag.Couriers = new SelectList(_context.MsCourier, "CourierID", "CourierName", invoice.CourierID);
            ViewBag.Payments = new SelectList(_context.MsPayment, "PaymentID", "PaymentName", invoice.PaymentType);
            ViewBag.Products = new SelectList(_context.MsProduct, "ProductID", "ProductName");
            ViewData["details"] = details ?? new List<TrInvoiceDetail>();
            return View(invoice);
        }

        details ??= new List<TrInvoiceDetail>();

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var existing = await _context.TrInvoice.FindAsync(invoice.InvoiceNo);
            if (existing == null) return NotFound();

            existing.InvoiceDate = invoice.InvoiceDate;
            existing.InvoiceTo = invoice.InvoiceTo;
            existing.ShipTo = invoice.ShipTo;
            existing.SalesID = invoice.SalesID;
            existing.CourierID = invoice.CourierID;
            existing.PaymentType = invoice.PaymentType;
            // jika ada field lain yg perlu diupdate, tambahkan di sini
            await _context.SaveChangesAsync();

            // Hapus semua detail lama untuk invoice ini (raw SQL)
            var pInvoice = new SqlParameter("@invoiceNo", invoice.InvoiceNo);
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM TrInvoiceDetail WHERE InvoiceNo = @invoiceNo", pInvoice);

            var insertSql = @"
            INSERT INTO TrInvoiceDetail (InvoiceNo, ProductID, Price, Weight, Qty)
            VALUES (@invoiceNo, @productId, @price, @weight, @qty);
        ";

            foreach (var d in details)
            {
                // ambil data product (jika perlu mengisi Price/Weight)
                var product = await _context.MsProduct.FindAsync(d.ProductID);
                if (product != null)
                {
                    d.Price = product.Price;
                    d.Weight = product.Weight;
                }

                var parms = new[]
                {
                new SqlParameter("@invoiceNo", invoice.InvoiceNo ?? (object)DBNull.Value),
                new SqlParameter("@productId", d.ProductID),
                new SqlParameter("@price", d.Price),
                new SqlParameter("@weight", d.Weight),
                new SqlParameter("@qty", d.Qty)
            };

                await _context.Database.ExecuteSqlRawAsync(insertSql, parms);
            }

            await tx.CommitAsync();
            return RedirectToAction(nameof(Index), new { invoiceNo = invoice.InvoiceNo });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string invoiceNo, int productID)
    {
        var invoice = await _context.TrInvoice.FindAsync(invoiceNo);
        if (invoice == null) return NotFound();

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            // Hapus detail berdasarkan invoiceNo dan productID
            var parameters = new[]
            {
                new SqlParameter("@invoiceNo", invoiceNo),
                new SqlParameter("@productID", productID)
            };
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM TrInvoiceDetail WHERE InvoiceNo = @invoiceNo AND ProductID = @productID",
                parameters
            );

            // Opsional: jika ingin menghapus invoice jika tidak ada detail tersisa
            var remainingDetails = await _context.TrInvoiceDetail
                                                .Where(d => d.InvoiceNo == invoiceNo)
                                                .AnyAsync();
            if (!remainingDetails)
            {
                _context.TrInvoice.Remove(invoice);
                await _context.SaveChangesAsync();
            }

            await tx.CommitAsync();
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

}
