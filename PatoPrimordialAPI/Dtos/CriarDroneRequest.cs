namespace PatoPrimordialAPI.Dtos;

public class CriarDroneRequest
{
    public string NumeroSerie { get; set; } = string.Empty;
    public string? Status { get; set; }
    public string? FabricanteNome { get; set; }
    public string? Marca { get; set; }
    public string? PaisOrigem { get; set; }
    public decimal? PrecisaoNominalMinCm { get; set; }
    public decimal? PrecisaoNominalMaxM { get; set; }
}
