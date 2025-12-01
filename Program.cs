using BOTSwapper.Implementaciones;
using BOTSwapper.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BOTSwapper
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Cargar appsettings.json
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Crear factory
            IBrokerFactory brokerFactory = new BrokerFactory(config);

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Main(brokerFactory));
        }
    }
}