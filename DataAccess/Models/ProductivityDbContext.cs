using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WPF_RobinLine.Models;
using static WPF_App.Views.ProductivityView;

namespace DataAccess.Models;

public partial class ProductivityDbContext : DbContext
{
    public ProductivityDbContext()
    {
    }

    public ProductivityDbContext(DbContextOptions<ProductivityDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Failure> Failures { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<Operator> Operators { get; set; }

    public virtual DbSet<Production> Productions { get; set; }

    public virtual DbSet<Result> Results { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Shift> Shifts { get; set; }

    public virtual DbSet<Target> Targets { get; set; }

    public virtual DbSet<Size> Sizes { get; set; }

    public DbSet<ProductionSummary> ProductionSummaries { get; set; }  // This is for FromSqlRaw
    public DbSet<HourlyProduction> HourlyProductions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ProductivityDb;Trusted_Connection=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductionSummary>().HasNoKey();
        modelBuilder.Entity<HourlyProduction>().HasNoKey();

        modelBuilder.Entity<Failure>(entity =>
        {
            entity.HasKey(e => e.FailureId).HasName("PK__Failure__CCF1723DA03435EA");

            entity.ToTable("Failure");

            entity.Property(e => e.FailureId).HasColumnName("failure_id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.ProductionId).HasColumnName("production_id");
            entity.Property(e => e.ResultId).HasColumnName("result_id");
            entity.Property(e => e.Severity)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("severity");
            entity.Property(e => e.Timestamp)
                .HasColumnType("datetime")
                .HasColumnName("timestamp");

            entity.HasOne(d => d.Production).WithMany(p => p.Failures)
                .HasForeignKey(d => d.ProductionId)
                .HasConstraintName("FK__Failure__product__5CD6CB2B");

            entity.HasOne(d => d.Result).WithMany(p => p.Failures)
                .HasForeignKey(d => d.ResultId)
                .HasConstraintName("FK__Failure__result___5DCAEF64");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__Item__52020FDD663C4754");

            entity.ToTable("Item");

            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.ModelName)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasColumnName("model_name");
            entity.Property(e => e.OperatorId).HasColumnName("operator_id");
            entity.Property(e => e.PartType).HasColumnName("part_type");
            entity.Property(e => e.SizeId).HasColumnName("size_id");
            entity.HasOne(d => d.Size).WithMany(p => p.Items)
                .HasForeignKey(d => d.SizeId)
                .HasConstraintName("FK_Item_Size");

            entity.Property(e => e.Timestamp)
                .HasColumnType("datetime")
                .HasColumnName("timestamp");
            entity.Property(e => e.Type).HasColumnName("type");

            entity.HasOne(d => d.Operator).WithMany(p => p.Items)
                .HasForeignKey(d => d.OperatorId)
                .HasConstraintName("FK__Item__operator_i__5070F446");
        });

        modelBuilder.Entity<Operator>(entity =>
        {
            entity.HasKey(e => e.OperatorId).HasName("PK__Operator__9D9A890158E0926D");

            entity.ToTable("Operator");

            entity.Property(e => e.OperatorId).HasColumnName("operator_id");
            entity.Property(e => e.HiredDate)
                .HasColumnType("datetime")
                .HasColumnName("hired_date");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("role");
            entity.Property(e => e.RoleId).HasColumnName("role_id");

            entity.HasOne(d => d.RoleNavigation).WithMany(p => p.Operators)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_Operator_Role");
        });

        modelBuilder.Entity<Production>(entity =>
        {
            entity.HasKey(e => e.ProdId).HasName("PK__Producti__56958AB2A13B3C46");

            entity.ToTable("Production");

            entity.Property(e => e.ProdId).HasColumnName("prod_id");
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.OperatorId).HasColumnName("operator_id");
            entity.Property(e => e.ResultId).HasColumnName("result_id");
            entity.Property(e => e.ShiftId).HasColumnName("shift_id");
            entity.Property(e => e.Timestamp)
                .HasColumnType("datetime")
                .HasColumnName("timestamp");

            entity.HasOne(d => d.Item).WithMany(p => p.Productions)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK__Productio__item___5812160E");

            entity.HasOne(d => d.Operator).WithMany(p => p.Productions)
                .HasForeignKey(d => d.OperatorId)
                .HasConstraintName("FK__Productio__opera__59FA5E80");

            entity.HasOne(d => d.Result).WithMany(p => p.Productions)
                .HasForeignKey(d => d.ResultId)
                .HasConstraintName("FK__Productio__resul__59063A47");

            entity.HasOne(d => d.Shift).WithMany(p => p.Productions)
                .HasForeignKey(d => d.ShiftId)
                .HasConstraintName("FK__Productio__shift__571DF1D5");
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.HasKey(e => e.ResultId).HasName("PK__Result__AFB3C316E9BB9607");

            entity.ToTable("Result");

            entity.Property(e => e.ResultId).HasColumnName("result_id");
            entity.Property(e => e.CycleTime).HasColumnName("cycle_time");
            entity.Property(e => e.IsDefective).HasColumnName("is_defective");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__760965CCF9AA0E10");

            entity.ToTable("Role");

            entity.HasIndex(e => e.RoleName, "UQ__Role__783254B19DDED3C5").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasKey(e => e.ShiftId).HasName("PK__Shift__7B26722002323147");

            entity.ToTable("Shift");

            entity.Property(e => e.ShiftId).HasColumnName("shift_id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.EndTime)
                .HasColumnType("time")
                .HasColumnName("end_time");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.StartTime)
                .HasColumnType("time")
                .HasColumnName("start_time");
            entity.Property(e => e.TargetProd).HasColumnName("target_prod");
        });

        modelBuilder.Entity<Target>(entity =>
        {
            entity.HasKey(e => e.TargetId).HasName("PK__Target__57D3816EA6678ECA");

            entity.ToTable("Target");

            entity.Property(e => e.TargetId)
                .ValueGeneratedNever()
                .HasColumnName("target_id");
            entity.Property(e => e.EfficencyGoal)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("efficency_goal");
            entity.Property(e => e.HourlyTarget).HasColumnName("hourly_target");
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.ShiftTarget).HasColumnName("shift_target");

            entity.HasOne(d => d.Item).WithMany(p => p.Targets)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK__Target__item_id__60A75C0F");
        });

        modelBuilder.Entity<Size>(entity =>
        {
            entity.HasKey(e => e.SizeId).HasName("PK__Size__B4E1C3CA");

            entity.ToTable("Size");

            entity.Property(e => e.SizeId)
                .HasColumnName("size_id");

            entity.Property(e => e.SizeValue)
                .HasMaxLength(10)
                .IsUnicode(true) 
                .HasColumnName("size_value");

            entity.Property(e => e.SizeType)
                .IsUnicode(true)
                .HasColumnName("size_type");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
