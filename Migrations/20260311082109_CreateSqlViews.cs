using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace examencsharp.Migrations
{
    /// <inheritdoc />
    public partial class CreateSqlViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Data", "data.sql");

            if (File.Exists(path))
            {
                var sql = File.ReadAllText(path);
                migrationBuilder.Sql(sql);
            }
            else
            {
                throw new FileNotFoundException($"Le fichier SQL est introuvable à l'emplacement : {path}");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS V_candidates;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS V_results;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS V_total_voters;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS V_participation_rate;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS V_dashboard;");
        }
    }
}
