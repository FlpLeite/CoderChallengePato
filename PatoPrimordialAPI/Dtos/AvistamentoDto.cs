using System;

namespace PatoPrimordialAPI.Dtos;

public record AvistamentoDto
{
    public string Id { get; init; } = string.Empty;
    public string DroneId { get; init; } = string.Empty;
    public string DroneNumeroSerie { get; init; } = string.Empty;
    public decimal AlturaValor { get; init; }
    public string AlturaUnidade { get; init; } = string.Empty;
    public decimal PesoValor { get; init; }
    public string PesoUnidade { get; init; } = string.Empty;
    public decimal PrecisaoValor { get; init; }
    public string PrecisaoUnidade { get; init; } = string.Empty;
    public decimal PrecisaoM { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string Cidade { get; init; } = string.Empty;
    public string Pais { get; init; } = string.Empty;
    public string EstadoPato { get; init; } = string.Empty;
    public int? Bpm { get; init; }
    public int? MutacoesQtde { get; init; }
    public DateTime CriadoEm { get; init; }
}
