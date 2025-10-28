using System;
using System.Collections.Generic;

namespace PatoPrimordialAPI.Domain.Entities;

public class Drone
{
    public long Id { get; set; }
    public string NumeroSerie { get; set; } = string.Empty;
    public long? FabricanteId { get; set; }
    public Fabricante? Fabricante { get; set; }
    public string Status { get; set; } = "desconhecido";
    public decimal? PrecisaoNominalMinCm { get; set; }
    public decimal? PrecisaoNominalMaxM { get; set; }
    public DateTime? UltimoContatoEm { get; set; }

    public ICollection<Avistamento> Avistamentos { get; set; } = new List<Avistamento>();
}
