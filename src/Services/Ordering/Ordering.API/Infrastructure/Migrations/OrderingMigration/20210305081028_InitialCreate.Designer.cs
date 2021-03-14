﻿// <auto-generated />
using System;
using Harta.Services.Ordering.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Harta.Services.Ordering.API.Infrastructure.Migrations.OrderingMigration
{
    [DbContext(typeof(OrderingContext))]
    [Migration("20210305081028_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("Relational:Sequence:ordering.customer-seq", "'customer-seq', 'ordering', '1', '10', '', '', 'Int64', 'False'")
                .HasAnnotation("Relational:Sequence:ordering.orderline-seq", "'orderline-seq', 'ordering', '1', '10', '', '', 'Int64', 'False'")
                .HasAnnotation("Relational:Sequence:ordering.order-seq", "'order-seq', 'ordering', '1', '10', '', '', 'Int64', 'False'")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Harta.Services.Ordering.Domain.AggregatesModel.CustomerAggregate.Customer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:HiLoSequenceName", "customer-seq")
                        .HasAnnotation("SqlServer:HiLoSequenceSchema", "ordering")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.SequenceHiLo);

                    b.Property<string>("IdentityGuid")
                        .IsRequired()
                        .HasColumnType("nvarchar(200)")
                        .HasMaxLength(200);

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("_ax4Code")
                        .IsRequired()
                        .HasColumnName("AX4Code")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("_d365Code")
                        .IsRequired()
                        .HasColumnName("D365Code")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("IdentityGuid")
                        .IsUnique();

                    b.ToTable("customers","ordering");
                });

            modelBuilder.Entity("Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate.Order", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:HiLoSequenceName", "order-seq")
                        .HasAnnotation("SqlServer:HiLoSequenceSchema", "ordering")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.SequenceHiLo);

                    b.Property<DateTime>("PODate")
                        .HasColumnType("datetime2");

                    b.Property<string>("PONumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("_custId")
                        .HasColumnName("CustomerId")
                        .HasColumnType("int");

                    b.Property<int>("_orderStatusId")
                        .HasColumnName("OrderStatusId")
                        .HasColumnType("int");

                    b.Property<string>("_path")
                        .IsRequired()
                        .HasColumnName("Path")
                        .HasColumnType("nvarchar(200)")
                        .HasMaxLength(200);

                    b.Property<int>("_systemTypeId")
                        .HasColumnName("SystemTypeId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("_custId");

                    b.HasIndex("_orderStatusId");

                    b.HasIndex("_systemTypeId");

                    b.ToTable("orders","ordering");
                });

            modelBuilder.Entity("Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate.OrderLine", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:HiLoSequenceName", "orderline-seq")
                        .HasAnnotation("SqlServer:HiLoSequenceSchema", "ordering")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.SequenceHiLo);

                    b.Property<int>("OrderId")
                        .HasColumnType("int");

                    b.Property<string>("_fgCode")
                        .IsRequired()
                        .HasColumnName("FGCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("_quantity")
                        .HasColumnName("Quantity")
                        .HasColumnType("int");

                    b.Property<string>("_size")
                        .IsRequired()
                        .HasColumnName("Size")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("OrderId");

                    b.ToTable("orderlines","ordering");
                });

            modelBuilder.Entity("Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate.OrderStatus", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int")
                        .HasDefaultValue(1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(200)")
                        .HasMaxLength(200);

                    b.HasKey("Id");

                    b.ToTable("orderstatus","ordering");
                });

            modelBuilder.Entity("Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate.SystemType", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int")
                        .HasDefaultValue(1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(200)")
                        .HasMaxLength(200);

                    b.HasKey("Id");

                    b.ToTable("systemtypes","ordering");
                });

            modelBuilder.Entity("Harta.Services.Ordering.Infrastructure.Idempotent.ClientRequest", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Time")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("requests","ordering");
                });

            modelBuilder.Entity("Harta.Services.Ordering.Domain.AggregatesModel.CustomerAggregate.Customer", b =>
                {
                    b.OwnsOne("Harta.Services.Ordering.Domain.AggregatesModel.CustomerAggregate.Address", "Address", b1 =>
                        {
                            b1.Property<int>("CustomerId")
                                .HasColumnType("int");

                            b1.Property<string>("City")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Country")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("State")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Street")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("ZipCode")
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("CustomerId");

                            b1.ToTable("customers");

                            b1.WithOwner()
                                .HasForeignKey("CustomerId");
                        });
                });

            modelBuilder.Entity("Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate.Order", b =>
                {
                    b.HasOne("Harta.Services.Ordering.Domain.AggregatesModel.CustomerAggregate.Customer", null)
                        .WithMany()
                        .HasForeignKey("_custId");

                    b.HasOne("Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate.OrderStatus", "OrderStatus")
                        .WithMany()
                        .HasForeignKey("_orderStatusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate.SystemType", "SystemType")
                        .WithMany()
                        .HasForeignKey("_systemTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate.OrderLine", b =>
                {
                    b.HasOne("Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate.Order", null)
                        .WithMany("OrderLines")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
