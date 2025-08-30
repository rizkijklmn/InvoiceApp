using Microsoft.EntityFrameworkCore;
using InvoiceApp.Models;

namespace InvoiceApp.db;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<MsCourier> MsCourier { get; set; }
    public DbSet<ltCourierFee> ltCourierFee { get; set; }
    public DbSet<MsPayment> MsPayment { get; set; }
    public DbSet<MsProduct> MsProduct { get; set; }
    public DbSet<MsSales> MsSales { get; set; }
    public DbSet<TrInvoice> TrInvoice { get; set; }
    public DbSet<TrInvoiceDetail> TrInvoiceDetail { get; set; }


    // view model
    public DbSet<InvoiceViewModel> InvoiceViewModels { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Konfigurasikan composite key untuk TrInvoiceDetail
        modelBuilder.Entity<TrInvoiceDetail>(entity =>
        {
            entity.HasKey(e => new { e.InvoiceNo, e.ProductID });
        });

        modelBuilder.Entity<InvoiceViewModel>(entity =>
        {
            entity.HasNoKey();
        });
    }

}
