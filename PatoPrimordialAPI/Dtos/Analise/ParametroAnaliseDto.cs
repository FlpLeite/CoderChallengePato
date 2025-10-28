using System;

namespace PatoPrimordialAPI.Dtos.Analise;

public record ParametroAnaliseDto
{
    public long Id { get; init; }
    public string Chave { get; init; } = string.Empty;
    public double? ValorNum { get; init; }
    public string? ValorTxt { get; init; }
    public DateTime AtualizadoEm { get; init; }
}
