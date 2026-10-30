using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddListings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "listings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    model_id = table.Column<Guid>(type: "uuid", nullable: false),
                    city_id = table.Column<Guid>(type: "uuid", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    mileage = table.Column<int>(type: "integer", nullable: false),
                    mileage_unit = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    engine_volume = table.Column<decimal>(type: "numeric(3,1)", precision: 3, scale: 1, nullable: true),
                    horsepower = table.Column<int>(type: "integer", nullable: false),
                    seats = table.Column<int>(type: "integer", nullable: false),
                    vin = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: true),
                    body_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fuel_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    transmission = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    drivetrain = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    market = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    description = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    is_credit = table.Column<bool>(type: "boolean", nullable: false),
                    is_barter = table.Column<bool>(type: "boolean", nullable: false),
                    is_painted = table.Column<bool>(type: "boolean", nullable: false),
                    is_damaged = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_listings", x => x.id);
                    table.ForeignKey(
                        name: "fk_listings_cities_city_id",
                        column: x => x.city_id,
                        principalTable: "cities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_listings_models_model_id",
                        column: x => x.model_id,
                        principalTable: "models",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_listings_city_id",
                table: "listings",
                column: "city_id");

            migrationBuilder.CreateIndex(
                name: "ix_listings_created_at_id",
                table: "listings",
                columns: new[] { "created_at", "id" },
                descending: new bool[0],
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_listings_model_id",
                table: "listings",
                column: "model_id");

            migrationBuilder.CreateIndex(
                name: "ix_listings_owner_id_created_at",
                table: "listings",
                columns: new[] { "owner_id", "created_at" },
                descending: new[] { false, true },
                filter: "is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "listings");
        }
    }
}
