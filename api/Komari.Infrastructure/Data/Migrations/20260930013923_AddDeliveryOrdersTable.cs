using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Komari.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryOrdersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "BillId",
                table: "orders",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "delivery_orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerAddressId = table.Column<Guid>(type: "uuid", nullable: false),
                    Street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Neighborhood = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ZipCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Complement = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReferencePoint = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DeliveryFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ChangeFor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    DriverName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EstimatedMinutes = table.Column<int>(type: "integer", nullable: true),
                    DispatchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_delivery_orders_customer_addresses_CustomerAddressId",
                        column: x => x.CustomerAddressId,
                        principalTable: "customer_addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_delivery_orders_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_delivery_orders_orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_orders_Type",
                table: "orders",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_orders_CreatedAt",
                table: "delivery_orders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_orders_CustomerAddressId",
                table: "delivery_orders",
                column: "CustomerAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_orders_CustomerId",
                table: "delivery_orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_orders_OrderId",
                table: "delivery_orders",
                column: "OrderId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "delivery_orders");

            migrationBuilder.DropIndex(
                name: "IX_orders_Type",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "orders");

            migrationBuilder.AlterColumn<Guid>(
                name: "BillId",
                table: "orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
