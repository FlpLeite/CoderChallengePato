using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PatoPrimordialAPI.Migrations
{
    /// <inheritdoc />
    public partial class CreateSchemaBigint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fabricantes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Marca = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PaisOrigem = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fabricantes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "parametros_analise",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Chave = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ValorNum = table.Column<double>(type: "double precision", nullable: true),
                    ValorTxt = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parametros_analise", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pontos_referencia",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    RaioMetros = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pontos_referencia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "drones",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroSerie = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FabricanteId = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PrecisaoNominalMinCm = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    PrecisaoNominalMaxM = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    UltimoContatoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_drones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_drones_fabricantes_FabricanteId",
                        column: x => x.FabricanteId,
                        principalTable: "fabricantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "patos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AlturaCm = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    PesoG = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Cidade = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Pais = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    PrecisaoM = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    PontoReferenciaId = table.Column<long>(type: "bigint", nullable: true),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Bpm = table.Column<int>(type: "integer", nullable: true),
                    MutacoesQtde = table.Column<int>(type: "integer", nullable: true),
                    PoderNome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PoderDescricao = table.Column<string>(type: "text", nullable: true),
                    PoderTagsCsv = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_patos_pontos_referencia_PontoReferenciaId",
                        column: x => x.PontoReferenciaId,
                        principalTable: "pontos_referencia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "analise_pato",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PatoId = table.Column<long>(type: "bigint", nullable: false),
                    CustoTransporte = table.Column<double>(type: "double precision", nullable: false),
                    RiscoTotal = table.Column<int>(type: "integer", nullable: false),
                    ValorCientifico = table.Column<int>(type: "integer", nullable: false),
                    PoderioNecessario = table.Column<int>(type: "integer", nullable: false),
                    Prioridade = table.Column<double>(type: "double precision", nullable: false),
                    ClassePrioridade = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ClasseRisco = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DistKm = table.Column<double>(type: "double precision", nullable: false),
                    CalculadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analise_pato", x => x.Id);
                    table.ForeignKey(
                        name: "FK_analise_pato_patos_PatoId",
                        column: x => x.PatoId,
                        principalTable: "patos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "avistamentos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DroneId = table.Column<long>(type: "bigint", nullable: false),
                    PatoId = table.Column<long>(type: "bigint", nullable: false),
                    AlturaValor = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    AlturaUnidade = table.Column<string>(type: "text", nullable: false),
                    PesoValor = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    PesoUnidade = table.Column<string>(type: "text", nullable: false),
                    AlturaCm = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    PesoG = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    PrecisaoValor = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    PrecisaoUnidade = table.Column<string>(type: "text", nullable: false),
                    PrecisaoM = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Confianca = table.Column<double>(type: "double precision", nullable: false),
                    Cidade = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Pais = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    EstadoPato = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Bpm = table.Column<int>(type: "integer", nullable: true),
                    MutacoesQtde = table.Column<int>(type: "integer", nullable: true),
                    PoderNome = table.Column<string>(type: "text", nullable: true),
                    PoderDescricao = table.Column<string>(type: "text", nullable: true),
                    PoderTagsCsv = table.Column<string>(type: "text", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_avistamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_avistamentos_drones_DroneId",
                        column: x => x.DroneId,
                        principalTable: "drones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_avistamentos_patos_PatoId",
                        column: x => x.PatoId,
                        principalTable: "patos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_analise_pato_PatoId",
                table: "analise_pato",
                column: "PatoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_avistamentos_CriadoEm",
                table: "avistamentos",
                column: "CriadoEm");

            migrationBuilder.CreateIndex(
                name: "IX_avistamentos_DroneId",
                table: "avistamentos",
                column: "DroneId");

            migrationBuilder.CreateIndex(
                name: "IX_avistamentos_PatoId",
                table: "avistamentos",
                column: "PatoId");

            migrationBuilder.CreateIndex(
                name: "IX_drones_FabricanteId",
                table: "drones",
                column: "FabricanteId");

            migrationBuilder.CreateIndex(
                name: "IX_drones_NumeroSerie",
                table: "drones",
                column: "NumeroSerie",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_parametros_analise_Chave",
                table: "parametros_analise",
                column: "Chave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_patos_Codigo",
                table: "patos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_patos_PontoReferenciaId",
                table: "patos",
                column: "PontoReferenciaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "analise_pato");

            migrationBuilder.DropTable(
                name: "avistamentos");

            migrationBuilder.DropTable(
                name: "parametros_analise");

            migrationBuilder.DropTable(
                name: "drones");

            migrationBuilder.DropTable(
                name: "patos");

            migrationBuilder.DropTable(
                name: "fabricantes");

            migrationBuilder.DropTable(
                name: "pontos_referencia");
        }
    }
}
