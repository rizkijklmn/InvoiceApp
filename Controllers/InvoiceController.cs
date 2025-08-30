using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvoiceApp.db;
using InvoiceApp.Models;

public class InvoiceController : Controller
{
    private readonly AppDbContext _context;
    public InvoiceController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Detail Invoice
    public async Task<IActionResult> Detail(string? invoiceNo)
    {
        if (string.IsNullOrEmpty(invoiceNo)) return NotFound();

        var header = await _context.TrInvoice.FindAsync(invoiceNo);
        if (header == null) return NotFound();

        var details = await TrInvoiceDetail.GetInvoiceDetailsAsync(_context, invoiceNo);

        var vm = new InvoiceEditViewModel
        {
            InvoiceNo = header.InvoiceNo,
            InvoiceDate = header.InvoiceDate,
            InvoiceTo = header.InvoiceTo,
            ShipTo = header.ShipTo,
            SalesID = header.SalesID,
            CourierID = header.CourierID,
            PaymentType = header.PaymentType,
            // CourierFee = header.CourierFee,
            Details = details
        };

        return View(vm);
    }

    // POST: Update Invoice
    [HttpPost]
    public async Task<IActionResult> Update(InvoiceEditViewModel model)
    {
        if (!ModelState.IsValid) return View("Detail", model);

        var invoice = await _context.TrInvoice.FindAsync(model.InvoiceNo);
        if (invoice == null) return NotFound();

        invoice.InvoiceDate = model.InvoiceDate;
        invoice.InvoiceTo = model.InvoiceTo;
        invoice.ShipTo = model.ShipTo;
        invoice.SalesID = model.SalesID;
        invoice.CourierID = model.CourierID;
        invoice.PaymentType = model.PaymentType;
        // invoice.CourierFee = (int)model.CourierFee;

        _context.TrInvoice.Update(invoice);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Invoice updated successfully!";
        return RedirectToAction("Detail", new { invoiceNo = model.InvoiceNo });
    }

    // GET: Delete Invoice
    public async Task<IActionResult> Delete(string invoiceNo)
    {
        var invoice = await _context.TrInvoice.FindAsync(invoiceNo);
        if (invoice == null) return NotFound();

        _context.TrInvoice.Remove(invoice);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Invoice deleted successfully!";
        return RedirectToAction("Index", "Home");
    }

    // GET: Edit Detail Item
    public async Task<IActionResult> EditDetail(string invoiceNo, int productId)
    {
        var detail = await _context.TrInvoiceDetail
            .Where(d => d.InvoiceNo == invoiceNo && d.ProductID == productId)
            .FirstOrDefaultAsync();

        if (detail == null) return NotFound();

        return View(detail);
    }

    // POST: Update Detail Item
    [HttpPost]
    public async Task<IActionResult> UpdateDetail(TrInvoiceDetail model)
    {
        var detail = await _context.TrInvoiceDetail
            .Where(d => d.InvoiceNo == model.InvoiceNo && d.ProductID == model.ProductID)
            .FirstOrDefaultAsync();

        if (detail == null) return NotFound();

        detail.Qty = model.Qty;
        detail.Weight = model.Weight;
        detail.Price = model.Price;

        _context.TrInvoiceDetail.Update(detail);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Invoice detail updated successfully!";
        return RedirectToAction("Detail", new { invoiceNo = model.InvoiceNo });
    }
}
