using System;
using System.Collections.Generic;

namespace PatoPrimordialAPI.Dtos;

public record PatoDto
{
    public long Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public decimal AlturaCm { get; init; }
    public decimal PesoG { get; init; }
    public string Cidade { get; init; } = string.Empty;
    public string Pais { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public decimal PrecisaoM { get; init; }
    public string Estado { get; init; } = string.Empty;
    public int? Bpm { get; init; }
    public int? MutacoesQtde { get; init; }
    public string? PoderNome { get; init; }
    public string? PoderDescricao { get; init; }
    public IReadOnlyCollection<string> PoderTags { get; init; } = Array.Empty<string>();
    public long? PontoReferenciaId { get; init; }
    public string? PontoReferenciaNome { get; init; }
    public DateTime CriadoEm { get; init; }
    public DateTime AtualizadoEm { get; init; }
}
