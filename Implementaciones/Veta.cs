using BOTSwapper.Interfaces;

namespace BOTSwapper.Implementaciones
{
    public class Veta : IBroker
    {
        public Task<string> Comprar(string simbolo, int cantidad, double precio, string plazo = "CI")
        {
            throw new NotImplementedException();
        }

        public Task<string> EliminarOperacion(string operacionVenta)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetEstadoOperacion(string idoperacion)
        {
            throw new NotImplementedException();
        }

        public Task<double> ObtenerEfectivoDisponible()
        {
            throw new NotImplementedException();
        }

        public Task<string> ObtenerPortafolio()
        {
            throw new NotImplementedException();
        }

        public Task<string> Vender(string simbolo, int cantidad, double precio, string plazo = "CI")
        {
            throw new NotImplementedException();
        }
    }
}
