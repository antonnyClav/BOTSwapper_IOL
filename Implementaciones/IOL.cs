using BOTSwapper.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace BOTSwapper.Implementaciones
{
    public class IOL : IBroker
    {
        private readonly HttpClient _http = new HttpClient();
        const string sURLIOL = "https://api.invertironline.com";
        string bearer = "";
        string refresh = "";
        DateTime expires;

        public IOL()
        {
            Task task = Login();
        }

        public async Task<string> Comprar(string simbolo, int cantidad, double precio, string plazo = "CI")
        {
            GrabarLog("Comprando " + simbolo);
            await Login();
            try
            {
                //Application.DoEvents();
                string validez = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "T17:59:59.000Z";
                var parametros = new Dictionary<string, string>()
                {
                    { "mercado", "bCBA" },
                    { "simbolo", simbolo },
                    { "cantidad", cantidad.ToString() },
                    { "precio", precio.ToString().Replace(",", ".") },
                    { "validez", validez },
                    { "plazo", plazo }
                };

                string response;
                response = await GetResponsePOST("/api/v2/operar/Comprar", parametros);
                if (response.Contains("Error") || response.Contains("opuesta"))
                {
                    return "Error";
                }
                else
                {
                    dynamic json = JObject.Parse(response);
                    string operacion = json.numeroOperacion;
                    if (json.ok == "false")
                    {
                        string description =
                            json["messages"] != null &&
                            json["messages"].HasValues &&
                            json["messages"][0]["description"] != null
                                ? json["messages"][0]["description"].ToString()
                                : "Error";
                        return description;
                    }
                    else
                    {
                        return operacion;
                    }
                }
            }
            catch (Exception ex)
            {
                GrabarLog("Error Metodo Comprar: " + ex.Message);
                return "Error";
            }
        }

        public async Task<string> Vender(string simbolo, int cantidad, double precio, string plazo = "CI")
        {
            GrabarLog("Vendiendo " + simbolo);
            await Login();
            try
            {
                //Application.DoEvents();
                string validez = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "T17:59:59.000Z";
                var parametros = new Dictionary<string, string>()
                {
                    { "mercado", "bCBA" },
                    { "simbolo", simbolo },
                    { "cantidad", cantidad.ToString() },
                    { "precio", precio.ToString().Replace(",", ".") },
                    { "validez", validez },
                    { "plazo", plazo }
                };
                string response;
                response = await GetResponsePOST("/api/v2/operar/Vender", parametros);
                if (response.Contains("Error") || response.Contains("opuesta"))
                {
                    return "Error";
                }
                else
                {
                    dynamic json = JObject.Parse(response);
                    string operacion = json.numeroOperacion;
                    if (json.ok == "false")
                    {
                        string description =
                            json["messages"] != null &&
                            json["messages"].HasValues &&
                            json["messages"][0]["description"] != null
                                ? json["messages"][0]["description"].ToString()
                                : "Error";

                        return description;
                    }
                    else
                    {
                        return operacion;
                    }
                }
            }
            catch (Exception ex)
            {
                GrabarLog("Error Metodo Vender: " + ex.Message);
                return "Error";
            }
        }

        public async Task<double>  ObtenerEfectivoDisponible()
        {
            await Login();
            try
            {
                string json = await GetResponseGET("/api/v2/estadocuenta");

                var estado = JsonConvert.DeserializeObject<EstadoCuenta>(json);

                if (estado?.cuentas == null)
                    return 0;

                var cuentaPesos = estado.cuentas
                    .FirstOrDefault(c => c.moneda == "peso_Argentino");

                if (cuentaPesos == null)
                    return 0;

                return cuentaPesos.disponible;
            }
            catch (Exception ex)
            {
                GrabarLog("Error Metodo ObtenerEfectivoDisponible: " + ex.Message);
                return 0;
            }

        }

        public async Task<string> GetEstadoOperacion(string idoperacion)
        {
            await Login();
            try
            {
                string response;
                response = await GetResponseGET("/api/v2/operaciones/" + idoperacion);
                if (response.Contains("Error") || response.Contains("Se exced"))
                {
                    return "Error";
                }
                else
                {
                    dynamic json = JObject.Parse(response);
                    return json.estadoActual.Value;
                }
            }
            catch (Exception ex)
            {
                GrabarLog("Error Metodo GetEstadoOperacion: " + ex.Message);
                return "Error";
            }

        }
        public async Task<string> EliminarOperacion(string operacionVenta)
        {
            try
            {
                // Refrescamos token si hace falta
                await Login();

                var url = $"{sURLIOL}/api/v2/operaciones/{operacionVenta}";

                var request = new HttpRequestMessage(HttpMethod.Delete, url);

                // Headers requeridos
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearer.Replace("Bearer ", ""));

                request.Headers.Add("Cookie", "1ea603=MuHfvuwOKZdvI6yElkJgUoz5BqAD8qe4WIW3jAmoLD/H7GeYR52OTikKNKm2SVpfTRC7es26SwmEsSuM7nlnlXv0p35fvIzPFiA2EMKplgaNh7ddDGF1TZCoG35cIbjKWIm2+yLGtu9mWNBjTnrtmGQlSHxoxh2OJn9kmJaClZKOkm9O");

                // Ejecutar request
                var response = await _http.SendAsync(request);

                // Si no fue 200 OK → devolver mensaje de error
                if (!response.IsSuccessStatusCode)
                {
                    return $"ERROR {response.StatusCode}: {await response.Content.ReadAsStringAsync()}";
                }

                // Normalmente la API devuelve vacío en DELETE
                var content = await response.Content.ReadAsStringAsync();
                return string.IsNullOrWhiteSpace(content) ? "OK (sin contenido)" : content;
            }
            catch (Exception ex)
            {
                GrabarLog("Error Metodo EliminarOperacion: " + ex.Message);
                return "Error: " + ex.Message;
            }
        }

        public async Task<string> ObtenerPortafolio()
        {
            await Login();
            return await GetResponseGET("/api/v2/portafolio/argentina");
        }
        
        private async Task Login()
        {
            var configuracion = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

            try
            {
                if (expires == DateTime.MinValue)
                {
                    var parametros = new Dictionary<string, string>()
                    {
                        { "username", configuracion.GetSection("MiConfiguracion:UsuarioIOL").Value },
                        { "password", configuracion.GetSection("MiConfiguracion:ClaveIOL").Value },
                        { "grant_type", "password" }
                    };
                    string response;
                    response = await GetResponsePOST("/token", parametros);
                    dynamic json = JObject.Parse(response);
                    bearer = "Bearer " + json.access_token;
                    expires = DateTime.Now.AddSeconds((double)json.expires_in - 300);
                    refresh = json.refresh_token;
                }
                else
                {
                    if (DateTime.Now >= expires)
                    {
                        var parametros = new Dictionary<string, string>()
                        {
                            { "refresh_token", refresh },
                            { "grant_type", "refresh_token" }
                        };
                        string response;
                        response = await GetResponsePOST("/token", parametros);
                        if (response.Contains("Error") || response.Contains("excedi"))
                        {
                            GrabarLog(response);
                        }
                        else
                        {
                            dynamic json = JObject.Parse(response);
                            bearer = "Bearer " + json.access_token;
                            expires = DateTime.Now.AddSeconds((double)json.expires_in - 300);
                            refresh = json.refresh_token;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                GrabarLog(e.Message);
            }

        }

        private async Task<string> GetResponsePOST(string sURLRecurso, Dictionary<string, string> parametros)
        {
            try
            {
                var url = $"{sURLIOL}{sURLRecurso}";

                // Armamos el body con form-urlencoded
                var content = new FormUrlEncodedContent(parametros);

                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = content;

                // Headers
                request.Headers.Add("Cookie",
                    "1ea603=9HlJ7jDhxAcl8Kr7mVSKVugsK3AhfmaJ+x7Rg4i21jZz82yrpqXoUM27KqH6RxLbn5G74mjXrXEqfv/repNJB8LhgnjL5/MKYm/Gkh/ci8k0Aif9n/ANh9CHuS2rEbaYTUaACZ3YNMJJMVyoy2Kxt1rwXw0ciOmbPBamT5KtBqLl9SQR"
                );

                if (!string.IsNullOrWhiteSpace(bearer))
                {
                    // bearer = "Bearer XXXXXXXXX"
                    var token = bearer.Replace("Bearer ", "");
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                // Ejecutar POST
                var response = await _http.SendAsync(request);

                // Leer contenido
                var responseText = await response.Content.ReadAsStringAsync();

                // Si no fue 200-299, devolvemos el error
                if (!response.IsSuccessStatusCode)
                {
                    GrabarLog($"ERROR GetResponsePOST {response.StatusCode}: {responseText}");
                    return $"ERROR {response.StatusCode}: {responseText}";
                }

                return responseText;
            }
            catch (Exception ex)
            {
                GrabarLog("Error Metodo GetResponsePOST: " + ex.Message);
                return "Error: " + ex.Message;
            }
        }

        private async Task<string> GetResponseGET(string endpoint)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, sURLIOL + endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer.Replace("Bearer ", ""));
                var response = await _http.SendAsync(request);

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                GrabarLog("Error GetResponseGET: " + ex.Message);
                return "Error";
            }
        }

        private static void GrabarLog(string mensaje)
        {
            object _locker = new object();
            string _logFilePath = "log.txt"; // Ruta del archivo

            try
            {
                lock (_locker)
                {
                    using (StreamWriter writer = new StreamWriter(_logFilePath, true))
                    {
                        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {mensaje}");
                    }
                }
            }
            catch (Exception ex)
            {
                GrabarLog("Error Metodo GrabarLog: " + ex.Message);
                // Si querés también podes manejar qué hacer si falla el log
                Console.WriteLine("Error al escribir log: " + ex.Message);
            }
        }

    }
}
