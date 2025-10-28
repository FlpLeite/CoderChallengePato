using System;

namespace PatoPrimordialAPI.Domain.Entities;

public class Avistamento
{
    public long Id { get; set; }
    public long DroneId { get; set; }
    public Drone Drone { get; set; } = null!;
    public long PatoId { get; set; }
    public Pato Pato { get; set; } = null!;
    public decimal AlturaValor { get; set; }
    public string AlturaUnidade { get; set; } = string.Empty;
    public decimal PesoValor { get; set; }
    public string PesoUnidade { get; set; } = string.Empty;
    public decimal AlturaCm { get; set; }
    public decimal PesoG { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal PrecisaoValor { get; set; }
    public string PrecisaoUnidade { get; set; } = string.Empty;
    public decimal PrecisaoM { get; set; }
    public double Confianca { get; set; }
    public string Cidade { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
    public string EstadoPato { get; set; } = string.Empty;
    public int? Bpm { get; set; }
    public int? MutacoesQtde { get; set; }
    public string? PoderNome { get; set; }
    public string? PoderDescricao { get; set; }
    public string? PoderTagsCsv { get; set; }
    public DateTime CriadoEm { get; set; }
}
