using System.ComponentModel.DataAnnotations;

namespace PatoPrimordialAPI.Dtos.Analise;

public record AtualizarParametroAnaliseDto
{
    [Required]
    public string Chave { get; init; } = string.Empty;
    public double? ValorNum { get; init; }
    public string? ValorTxt { get; init; }
}
