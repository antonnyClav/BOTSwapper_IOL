using BOTSwapper.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOTSwapper.Implementaciones
{
    public class BrokerFactory : IBrokerFactory
    {
        private readonly IConfiguration _config;

        public BrokerFactory(IConfiguration config)
        {
            _config = config;
        }

        public IBroker CrearBroker()
        {
            // lectura segura
            var raw = _config["MiConfiguracion:Broker"];
            return raw == "IOL"
            ? new IOL()
            : new Veta();
        }
    }

}
