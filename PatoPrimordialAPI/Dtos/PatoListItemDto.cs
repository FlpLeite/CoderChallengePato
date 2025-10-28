using System;

namespace PatoPrimordialAPI.Dtos;

public record PatoListItemDto
{
    public long Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Cidade { get; init; } = string.Empty;
    public string Pais { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public int? MutacoesQtde { get; init; }
    public decimal PrecisaoM { get; init; }
}
