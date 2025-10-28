using System;
using System.Collections.Generic;

namespace PatoPrimordialAPI.Domain.Entities;

public class Fabricante
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public string? PaisOrigem { get; set; }

    public ICollection<Drone> Drones { get; set; } = new List<Drone>();
}
