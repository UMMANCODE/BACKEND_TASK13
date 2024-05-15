using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcPustok.Migrations {
	/// <inheritdoc />
	public partial class Updated : Migration {
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder) {
			migrationBuilder.DropPrimaryKey(
					name: "PK_BookTags",
					table: "BookTags");

			migrationBuilder.DropIndex(
					name: "IX_BookTags_BookId",
					table: "BookTags");

			migrationBuilder.DropColumn(
					name: "Id",
					table: "BookTags");

			migrationBuilder.AddColumn<int>(
					name: "Id",
					table: "BookTags",
					type: "int",
					nullable: false)
					.Annotation("SqlServer:Identity", "1, 1");

			migrationBuilder.AlterColumn<string>(
					name: "FullName",
					table: "AspNetUsers",
					type: "nvarchar(max)",
					nullable: true,
					oldClrType: typeof(string),
					oldType: "nvarchar(max)");

			migrationBuilder.AddColumn<string>(
					name: "Discriminator",
					table: "AspNetUsers",
					type: "nvarchar(128)",
					maxLength: 128,
					nullable: false,
					defaultValue: "User");

			migrationBuilder.AddPrimaryKey(
					name: "PK_BookTags",
					table: "BookTags",
					column: "Id");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder) {
			migrationBuilder.DropPrimaryKey(
					name: "PK_BookTags",
					table: "BookTags");

			migrationBuilder.DropColumn(
					name: "Id",
					table: "BookTags");

			migrationBuilder.AddColumn<int>(
					name: "Id",
					table: "BookTags",
					type: "int",
					nullable: false,
					defaultValue: 0);

			migrationBuilder.AlterColumn<string>(
					name: "FullName",
					table: "AspNetUsers",
					type: "nvarchar(max)",
					nullable: false,
					defaultValue: "",
					oldClrType: typeof(string),
					oldType: "nvarchar(max)",
					oldNullable: true);

			migrationBuilder.DropColumn(
					name: "Discriminator",
					table: "AspNetUsers");

			migrationBuilder.AddPrimaryKey(
					name: "PK_BookTags",
					table: "BookTags",
					columns: new[] { "BookId", "TagId" });

			migrationBuilder.CreateIndex(
					name: "IX_BookTags_BookId",
					table: "BookTags",
					column: "BookId");
		}
	}
}
