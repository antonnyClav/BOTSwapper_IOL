using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOTSwapper.Interfaces
{
    public interface IBroker
    {
        Task<string> Vender(string simbolo, int cantidad, double precio, string plazo = "CI");
        Task<string> Comprar(string simbolo, int cantidad, double precio, string plazo = "CI");
        Task<double> ObtenerEfectivoDisponible();
        Task<string> GetEstadoOperacion(string idoperacion);
        Task<string> EliminarOperacion(string operacionVenta);
        Task<string> ObtenerPortafolio();
    }
}
