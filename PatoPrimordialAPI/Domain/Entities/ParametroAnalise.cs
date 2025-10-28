using System;

namespace PatoPrimordialAPI.Domain.Entities;

public class ParametroAnalise
{
    public long Id { get; set; }
    public string Chave { get; set; } = string.Empty;
    public double? ValorNum { get; set; }
    public string? ValorTxt { get; set; }
    public DateTime AtualizadoEm { get; set; }
}
