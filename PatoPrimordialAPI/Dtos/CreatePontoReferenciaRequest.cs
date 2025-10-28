using System.ComponentModel.DataAnnotations;

namespace PatoPrimordialAPI.Dtos;

public class CreatePontoReferenciaRequest
{
    [Required]
    [StringLength(200)]
    public string Nome { get; set; } = string.Empty;

    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }

    [Range(0.01, double.MaxValue)]
    public double RaioMetros { get; set; }
}
