using System.Linq.Expressions;
using VTInformatica.DTOs.Manufacturer;
using VTInformatica.Models;

namespace VTInformatica.Mapping
{
    public static class ManufacturerMapper
    {
        // Una sola definizione: usata nelle query (tradotta in SQL) e per le entità già in memoria.
        public static readonly Expression<Func<Manufacturer, ManufacturerDto>> Projection = m => new ManufacturerDto
        {
            ManufacturerId = m.ManufacturerId,
            ManufacturerName = m.ManufacturerName,
            ManufacturerLogo = m.ManufacturerLogo
        };

        private static readonly Func<Manufacturer, ManufacturerDto> Compiled = Projection.Compile();

        public static ManufacturerDto ToDto(Manufacturer manufacturer) => Compiled(manufacturer);
    }
}
