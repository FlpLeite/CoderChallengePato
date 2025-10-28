using System;

namespace PatoPrimordialAPI.Domain.Entities;

public class AnalisePato
{
    public long Id { get; set; }
    public long PatoId { get; set; }
    public Pato Pato { get; set; } = null!;
    public double CustoTransporte { get; set; }
    public int RiscoTotal { get; set; }
    public int ValorCientifico { get; set; }
    public int PoderioNecessario { get; set; }
    public double Prioridade { get; set; }
    public string ClassePrioridade { get; set; } = string.Empty;
    public string ClasseRisco { get; set; } = string.Empty;
    public double DistKm { get; set; }
    public DateTime CalculadoEm { get; set; }
}
