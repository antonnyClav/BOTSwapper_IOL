using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Net;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using System.Configuration;
using System.Media;
using Primary;
using Primary.Data;
using Microsoft.Data.SqlClient;
using System.Data;
using ScottPlot;
using System.Diagnostics;
using OpenTK;
using RestSharp;
using Azure;
using Newtonsoft.Json;
using System;
using System;
using System.Drawing;
using System.Numerics;
using BOTSwapper.Interfaces;

namespace BOTSwapper

{
    public partial class Main : Form
    {
        private readonly IBroker _broker;

        const string sURLIOL = "https://api.invertironline.com";
        const string sURLVETA = "https://api.veta.xoms.com.ar";
        const string prefijoPrimary = "MERV - XMEV - ";
        const string sufijoCI = " - CI";
        const string sufijo24 = " - 24hs";
        string tokenVETA;
        string bearer;
        string refresh;
        DateTime expires;
        List<string> nombres;
        List<Ticker> tickers;
        double umbral;
        double umbralMinimo;
        SqlConnection oCnn;
        string cs;
        SqlCommand sqlCommand;
        SqlDataReader rdr;
        double timeOffset;
        int intentos;
        int HoraDesdeDB;
        int HoraHastaDB;
        int HoraDesdeMK;
        int HoraHastaMK;
        int Tenencia1;
        int Tenencia2;
        public Main(IBrokerFactory broker)
        {
            _broker = broker.CrearBroker();

            InitializeComponent();

            DateTime hoy = DateTime.Now;
            this.Top = 10;
            this.Text = "BOT Swapper intraday - Copyright " + hoy.Year;
            this.Tag = this.Text;
            DoubleBuffered = true;
            CheckForIllegalCrossThreadCalls = false;
            nombres = new List<string>();
            tickers = new List<Ticker>();

            cboTicker1.Items.Add("GD29");
            cboTicker1.Items.Add("GD30");
            cboTicker1.Items.Add("GD35");
            cboTicker1.Items.Add("GD38");
            cboTicker1.Items.Add("GD41");
            cboTicker1.Items.Add("TX28");
            cboTicker1.Text = "GD30";
            cboTicker2.Items.Add("AL29");
            cboTicker2.Items.Add("AL30");
            cboTicker2.Items.Add("AL35");
            cboTicker2.Items.Add("AE38");
            cboTicker2.Items.Add("AL41");
            cboTicker2.Items.Add("TX26");
            cboTicker2.Text = "AL30";
            double umbral;
            for (umbral = 0.01; umbral <= 10; umbral += 0.01)
            {
                cboUmbral.Items.Add(Math.Round(umbral, 2));
            }
            cboUmbral.Text = "0,14";

            cboPlazo.Items.Clear();
            cboPlazo.Items.AddRange(new string[] { "CI", "24" });
            cboPlazo.Text = "24";

            var configuracion = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            try
            {
                cboUmbral.Text = configuracion.GetSection("MiConfiguracion:Umbral").Value;
                txtUsuarioIOL.Text = configuracion.GetSection("MiConfiguracion:UsuarioIOL").Value;
                txtClaveIOL.Text = configuracion.GetSection("MiConfiguracion:ClaveIOL").Value;
                txtUsuarioVETA.Text = configuracion.GetSection("MiConfiguracion:UsuarioVETA").Value;
                txtClaveVETA.Text = configuracion.GetSection("MiConfiguracion:ClaveVETA").Value;
                timeOffset = double.Parse(configuracion.GetSection("MiConfiguracion:TimeOffset").Value);
                cs = configuracion.GetSection("MiConfiguracion:CS").Value;
                intentos = int.Parse(configuracion.GetSection("MiConfiguracion:Intentos").Value);
                umbralMinimo = double.Parse(configuracion.GetSection("MiConfiguracion:Umbral").Value);
                txtBandaSup.Text = configuracion.GetSection("MiConfiguracion:BandaSup").Value;
                txtBandaInf.Text = configuracion.GetSection("MiConfiguracion:BandaInf").Value;
                cboTicker1.Text = configuracion.GetSection("MiConfiguracion:Ticker1").Value;
                cboTicker2.Text = configuracion.GetSection("MiConfiguracion:Ticker2").Value;
                cboPlazo.Text = configuracion.GetSection("MiConfiguracion:Plazo").Value;
                HoraDesdeDB = int.Parse(configuracion["MiConfiguracion:HoraDesdeDB"]);
                HoraHastaDB = int.Parse(configuracion["MiConfiguracion:HoraHastaDB"]);
                HoraDesdeMK = int.Parse(configuracion["MiConfiguracion:HoraDesdeMK"]);
                HoraHastaMK = int.Parse(configuracion["MiConfiguracion:HoraHastaMK"]);
                Tenencia1 = int.Parse(configuracion["MiConfiguracion:Tenencia1"]);
                Tenencia2 = int.Parse(configuracion["MiConfiguracion:Tenencia2"]);

            }
            catch (Exception ex)
            {
                ToLog(ex.Message);
            }

            AplicarTemaOscuro(this);
        }

        private void AplicarTemaOscuro(Control control)
        {
            //this.FormBorderStyle = FormBorderStyle.None;
            //this.Text = string.Empty;
            //this.ControlBox = false;

            // Colores base
            System.Drawing.Color fondo = System.Drawing.Color.FromArgb(15, 15, 15);         // negro suave
            System.Drawing.Color texto = System.Drawing.Color.LimeGreen;                    // verde tipo terminal
            System.Drawing.Color boton = System.Drawing.Color.FromArgb(70, 90, 120);        // gris azulado
            System.Drawing.Color bordeBoton = System.Drawing.Color.FromArgb(100, 130, 160);

            // Fondo del formulario
            if (control is Form)
                control.BackColor = fondo;

            // Recorremos todos los controles hijos
            foreach (Control c in control.Controls)
            {
                // Colores según tipo
                if (c is System.Windows.Forms.Button btn)
                {
                    btn.BackColor = boton;
                    btn.ForeColor = texto;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderColor = bordeBoton;
                    btn.FlatAppearance.BorderSize = 1;
                }
                else if (c is System.Windows.Forms.ComboBox cmb)
                {
                    cmb.BackColor = fondo;
                    cmb.ForeColor = texto;
                    cmb.FlatStyle = FlatStyle.Flat;
                }
                else if (c is System.Windows.Forms.TextBox txt)
                {
                    txt.BackColor = fondo;
                    txt.ForeColor = texto;
                    txt.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (c is System.Windows.Forms.Label lbl)
                {
                    lbl.ForeColor = texto;
                    lbl.BackColor = System.Drawing.Color.Transparent;
                }
                else if (c is DataGridView dgv)
                {
                    dgv.BackgroundColor = fondo;
                    dgv.ForeColor = texto;
                    dgv.GridColor = boton;
                    dgv.EnableHeadersVisualStyles = false;
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = boton;
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = texto;
                    dgv.RowHeadersDefaultCellStyle.BackColor = boton;
                    dgv.RowHeadersDefaultCellStyle.ForeColor = texto;
                }
                else
                {
                    c.BackColor = fondo;
                    c.ForeColor = texto;
                }

                // Llamada recursiva para los controles internos (paneles, groupboxes, etc.)
                if (c.HasChildren)
                    AplicarTemaOscuro(c);
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            SystemSounds.Exclamation.Play();
            chkAutoVol.Checked = false;
        }

        private string GetResponsePOST(string sURLRecurso, Dictionary<string, string> parametros)
        {

            var client = new RestClient(sURLIOL);
            var request = new RestRequest(sURLRecurso, Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddHeader("Cookie", "1ea603=9HlJ7jDhxAcl8Kr7mVSKVugsK3AhfmaJ+x7Rg4i21jZz82yrpqXoUM27KqH6RxLbn5G74mjXrXEqfv/repNJB8LhgnjL5/MKYm/Gkh/ci8k0Aif9n/ANh9CHuS2rEbaYTUaACZ3YNMJJMVyoy2Kxt1rwXw0ciOmbPBamT5KtBqLl9SQR");

            if (bearer != null)
            {
                request.AddHeader("Authorization", bearer);
            }

            // Agregar cada parámetro del diccionario
            foreach (var p in parametros)
            {
                request.AddParameter(p.Key, p.Value);
            }

            try
            {
                var response = client.Execute(request);
                //Console.WriteLine(response.Content);
                return response.Content;
            }
            catch (Exception ex)
            {

                return ex.Message;
            }

        }

        //private string GetResponseGET(string sURLRecurso, string token)
        //{
        //    var client = new RestClient(sURLIOL);
        //    var request = new RestRequest(sURLRecurso, Method.GET);
        //    request.AddHeader("Authorization", token);
        //    request.AddHeader("Cookie", "1ea603=MuHfvuwOKZdvI6yElkJgUoz5BqAD8qe4WIW3jAmoLD/H7GeYR52OTikKNKm2SVpfTRC7es26SwmEsSuM7nlnlXv0p35fvIzPFiA2EMKplgaNh7ddDGF1TZCoG35cIbjKWIm2+yLGtu9mWNBjTnrtmGQlSHxoxh2OJn9kmJaClZKOkm9O");

        //    try
        //    {
        //        var response = client.Execute(request);
        //        return response.Content;
        //    }
        //    catch (Exception ex)
        //    {

        //        return ex.Message;
        //    }
        //}

        private async void ToLog(string s)
        {
            lstLog.Items.Add(DateTime.Now.ToLongTimeString() + ": " + s);
            lstLog.SelectedIndex = lstLog.Items.Count - 1;

            GrabarLog(s);
        }

        public void GrabarLog(string mensaje)
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
                // Si querés también podes manejar qué hacer si falla el log
                Console.WriteLine("Error al escribir log: " + ex.Message);
            }
        }

        private async void btnIngresar_Click(object sender, EventArgs e)
        {
            await Inicio();
        }

        private async Task Inicio()
        {
            try
            {
                var api = new Api(new Uri(sURLVETA));
                await api.Login(txtUsuarioVETA.Text, txtClaveVETA.Text);
                if (api.AccessToken == null)
                {
                    ToLog("Login VETA ERROR!");
                    return;
                }
                else
                {
                    ToLog("Login VETA Ok");
                    txtToken.Text = api.AccessToken;
                }
                tokenVETA = "Bearer " + api.AccessToken;

                var allInstruments = await api.GetAllInstruments();

                var entries = new[] { Entry.Last, Entry.Bids, Entry.Offers };

                FillListaTickers();

                var instrumentos = allInstruments.Where(c => tickers.Any(t => t.NombreLargo == c.Symbol));
                using var socket = api.CreateMarketDataSocket(instrumentos, entries, 1, 1);
                socket.OnData = OnMarketData;
                var socketTask = await socket.Start();
                socketTask.Wait(1000);
                ToLog("Websocket Ok");
                LoginIOL();
                ToLog("Login IOL Ok");

                oCnn = new SqlConnection(cs);
                await oCnn.OpenAsync();
                using (SqlCommand setLanguageCommand = new SqlCommand("SET LANGUAGE us_english;", oCnn))
                {
                    await setLanguageCommand.ExecuteNonQueryAsync();
                }
                ToLog("SQL Server conectado Ok");
                RefreshChart();
                tmrRefresh.Interval = 10000;
                tmrRefresh.Enabled = true;
                tmrRefresh.Start();
                await socketTask;
            }
            catch (Exception e)
            {
                ToLog(e.Message);
            }
        }

        private async void LoginIOL()
        {
            try
            {
                if (expires == DateTime.MinValue)
                {
                    var parametros = new Dictionary<string, string>()
                    {
                        { "username", txtUsuarioIOL.Text },
                        { "password", txtClaveIOL.Text },
                        { "grant_type", "password" }
                    };
                    string response;
                    response = GetResponsePOST("/token", parametros);
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
                        response = GetResponsePOST("/token", parametros);
                        if (response.Contains("Error") || response.Contains("excedi"))
                        {
                            ToLog(response);
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
                txtBearer.Text = bearer;
                tmrToken.Interval = 1000;
                tmrToken.Enabled = true;
                tmrToken.Start();

            }
            catch (Exception e)
            {
                ToLog(e.Message);
            }

        }

        //tony
        private async void OnMarketData(Api api, MarketData marketData)
        {
            try
            {
                if (marketData == null || marketData.InstrumentId == null)
                {
                    ToLog("MarketData o InstrumentId es nulo.");
                    return;
                }

                var ticker = marketData.InstrumentId.Symbol ?? "SIN_TICKER";

                // Variables inicializadas en 0
                decimal bid = 0;
                decimal bidSize = 0;
                decimal offer = 0;
                decimal offerSize = 0;
                decimal? last = 0;

                // Verificar si Data existe
                var data = marketData.Data;
                if (data != null)
                {
                    // Bids
                    if (data.Bids != null && data.Bids.Length > 0 && data.Bids[0] != null)
                    {
                        bid = data.Bids[0].Price;
                        bidSize = data.Bids[0].Size;
                    }

                    // Offers
                    if (data.Offers != null && data.Offers.Length > 0 && data.Offers[0] != null)
                    {
                        offer = data.Offers[0].Price;
                        offerSize = data.Offers[0].Size;
                    }

                    // Last
                    if (data.Last != null)
                    {
                        last = data.Last.Price;
                    }
                }

                // Buscar el ticker en tu lista
                var elemento = tickers.FirstOrDefault(t => t.NombreLargo == ticker);
                if (elemento != null)
                {
                    elemento.bidSize = (int)bidSize;
                    elemento.bid = bid;
                    elemento.offer = offer;
                    elemento.offerSize = (int)offerSize;
                    elemento.last = (decimal)last;
                }
                else
                {
                    ToLog($"Ticker no encontrado: {ticker}");
                }
            }
            catch (Exception ex)
            {
                ToLog($"Error en OnMarketData: {ex.Message}");
            }
        }

        private void FillListaTickers()
        {
            nombres.Clear();
            nombres.Add(cboTicker1.Text);
            nombres.Add(cboTicker2.Text);

            foreach (var nombre in nombres)
            {
                tickers.Add(new Ticker(prefijoPrimary + nombre + sufijoCI, nombre + "CI", nombre));
                tickers.Add(new Ticker(prefijoPrimary + nombre + sufijo24, nombre + "24", nombre));
            }
        }

        private async void tmrRefresh_Tick(object sender, EventArgs e)
        {
            string ticker1, ticker2;
            int ticker1bidSize = 0, ticker2bidSize = 0;
            double ticker1Bid = 0, ticker2Bid = 0;
            double ticker1Last = 0, ticker2Last = 0;
            double ticker1Ask = 0, ticker2Ask = 0;
            int ticker1askSize = 0, ticker2askSize = 0;
            int iTenenciaTicker1 = 0, iTenenciaTicker2 = 0;
            double ventaTicker1 = 0, ventaTicker2 = 0;
            double compraTicker1 = 0, compraTicker2 = 0;
            double delta1a2 = 0, delta2a1 = 0;
            string response;

            Ticker ticker;
            LoginIOL();

            ticker1 = cboTicker1.Text;
            ticker = tickers.FirstOrDefault(t => t.NombreMedio == ticker1 + cboPlazo.Text);

            ticker1Last = (double)ticker.last;
            txtTicker1Last.Text = ticker.last.ToString();

            ticker1bidSize = ticker.bidSize;
            txtTicker1bidSize.Text = ticker.bidSize.ToString();

            ticker1Bid = (double)ticker.bid;
            txtTicker1Bid.Text = ticker.bid.ToString();

            ticker1Ask = (double)ticker.offer;
            txtTicker1Ask.Text = ticker.offer.ToString();

            ticker1askSize = ticker.offerSize;
            txtTicker1askSize.Text = ticker.offerSize.ToString();

            ticker2 = cboTicker2.Text;
            ticker = tickers.FirstOrDefault(t => t.NombreMedio == ticker2 + cboPlazo.Text);

            ticker2Last = (double)ticker.last;
            txtTicker2Last.Text = ticker.last.ToString();

            ticker2bidSize = ticker.bidSize;
            txtTicker2bidSize.Text = ticker.bidSize.ToString();

            ticker2Bid = (double)ticker.bid;
            txtTicker2Bid.Text = ticker.bid.ToString();

            ticker2Ask = (double)ticker.offer;
            txtTicker2Ask.Text = ticker.offer.ToString();

            ticker2askSize = ticker.offerSize;
            txtTicker2askSize.Text = ticker.offerSize.ToString();

            RefreshChart();

            //DB
            if (int.Parse(Ahora().ToString("HHmm")) >= HoraDesdeDB && int.Parse(Ahora().ToString("HHmm")) <= HoraHastaDB)
            {
                if (ticker1Bid > 0 && ticker1Last > 0 && ticker1Ask > 0 && ticker2Bid > 0 && ticker2Last > 0 && ticker2Ask > 0)
                {
                    SqlCommand sqlCommand = oCnn.CreateCommand();
                    sqlCommand.CommandText = "sp_MD_INS";
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@dt", Ahora());
                    sqlCommand.Parameters.AddWithValue("@Bono1_Bid", ticker1Bid);
                    sqlCommand.Parameters.AddWithValue("@Bono1_Last", ticker1Last);
                    sqlCommand.Parameters.AddWithValue("@Bono1_Ask", ticker1Ask);
                    sqlCommand.Parameters.AddWithValue("@Bono2_Bid", ticker2Bid);
                    sqlCommand.Parameters.AddWithValue("@Bono2_Last", ticker2Last);
                    sqlCommand.Parameters.AddWithValue("@Bono2_Ask", ticker2Ask);
                    sqlCommand.Parameters.AddWithValue("@bono1", this.cboTicker1.Text);
                    sqlCommand.Parameters.AddWithValue("@bono2", this.cboTicker2.Text);
                    sqlCommand.ExecuteNonQuery();
                }
            }
            else
            {
                ToLog("Fuera de horario para guardar DB.");
            }

            txtTenenciaTicker1.Text = "0";
            txtTenenciaTicker2.Text = "0";

            try
            {

                //response = GetResponseGET("/api/v2/portafolio/argentina", bearer);
                response = await _broker.ObtenerPortafolio();

                if (response.Contains("Error") || response.Contains("timed out") ||
                    response.Contains("tiempo de espera") || response.Contains("remoto") ||
                    response.Contains("r 401"))
                { ToLog("Error de obtención de tenencia: " + response); }
                else
                {
                    dynamic respuesta;
                    respuesta = JArray.Parse("[" + response + "]");
                    if (respuesta != null && respuesta.Count > 0)
                    {
                        var item0 = respuesta[0];

                        if (item0["activos"] == null)
                        {
                            ToLog("No se encontraron Activos en IOL");
                            return;
                        }
                    }
                    else
                    {
                        ToLog("No se encontraron Activos en IOL");
                        return;
                    }

                    foreach (dynamic activo in respuesta[0].activos)
                    {
                        if (activo.titulo.simbolo == ticker1)
                        {
                            iTenenciaTicker1 = Tenencia1 > 0 ? Tenencia1 : (int)activo.cantidad;
                            txtTenenciaTicker1.Text = iTenenciaTicker1.ToString();
                        }
                        if (activo.titulo.simbolo == ticker2)
                        {
                            iTenenciaTicker2 = Tenencia2 > 0 ? Tenencia2 : (int)activo.cantidad;
                            txtTenenciaTicker2.Text = iTenenciaTicker2.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ToLog("Error de obtención de tenencia: " + ex.Message);
            }

            txtVentaTicker1.Text = "0";
            txtCompraTicker2.Text = "0";
            txtVentaTicker2.Text = "0";
            txtCompraTicker1.Text = "0";
            txtDelta1a2.Text = "0";
            txtDelta2a1.Text = "0";


            if (iTenenciaTicker1 <= ticker1bidSize)
            {
                ventaTicker1 = (ticker1Bid * iTenenciaTicker1) / 100;
                txtVentaTicker1.Text = ventaTicker1.ToString();

                compraTicker2 = (ventaTicker1 / ticker2Ask) * 100;
                if (compraTicker2 <= ticker2askSize)
                {
                    txtCompraTicker2.Text = compraTicker2.ToString();
                }
                else
                {
                    txtCompraTicker2.Text = "0";
                    txtVentaTicker1.Text = "0";
                }
            }
            else
            {
                txtVentaTicker1.Text = "0";
                txtCompraTicker2.Text = "0";
            }

            if (iTenenciaTicker2 <= ticker2bidSize)
            {
                ventaTicker2 = (ticker2Bid * iTenenciaTicker2) / 100;
                txtVentaTicker2.Text = ventaTicker2.ToString();

                compraTicker1 = (ventaTicker2 / ticker1Ask) * 100;
                if (compraTicker1 <= ticker1askSize)
                {
                    txtCompraTicker1.Text = compraTicker1.ToString();
                }
                else
                {
                    txtCompraTicker1.Text = "0";
                    txtVentaTicker2.Text = "0";
                }
            }
            else
            {
                txtCompraTicker1.Text = "0";
                txtVentaTicker2.Text = "0";
            }
            if (txtMM.Text != "")
            {
                delta1a2 = Math.Round(double.Parse(txt1a2.Text) - double.Parse(txtMM.Text), 2);
                txtDelta1a2.Text = delta1a2.ToString();
                delta2a1 = Math.Round(double.Parse(txtMM.Text) - double.Parse(txt2a1.Text), 2);
                txtDelta2a1.Text = delta2a1.ToString();

                if (int.Parse(DateTime.Now.ToString("HHmm")) >= HoraDesdeMK && int.Parse(DateTime.Now.ToString("HHmm")) <= HoraHastaMK)
                {
                    if (iTenenciaTicker2 > 0 && compraTicker1 > 0) 
                        this.Text = this.Tag.ToString() + "---> Umbral actual: " + delta2a1;
                    if (iTenenciaTicker1 > 0 && compraTicker2 > 0)
                        this.Text = this.Tag.ToString() + "---> Umbral actual: " + delta1a2;

                    if (chkAuto.Checked)
                    {
                        umbral = double.Parse(cboUmbral.Text);

                        if (iTenenciaTicker2 > 0 && compraTicker1 > 0)
                        {
                                                       
                            Application.DoEvents();
                            if (delta2a1 >= umbral)
                            {
                                if (chkBandas.Checked == false ||
                                    (chkBandas.Checked == true && double.Parse(txt2a1.Text) <= double.Parse(txtBandaInf.Text)))
                                {
                                    SystemSounds.Asterisk.Play();
                                    ToLog("---> Umbral comparado:" + delta2a1);
                                    Rotar2a1();
                                }
                            }
                        }


                        if (iTenenciaTicker1 > 0 && compraTicker2 > 0)
                        {                                                       
                            Application.DoEvents();
                            if (delta1a2 >= umbral)
                            {
                                if (chkBandas.Checked == false ||
                                (chkBandas.Checked == true && double.Parse(txt1a2.Text) >= double.Parse(txtBandaSup.Text)))
                                {
                                    SystemSounds.Asterisk.Play();
                                    ToLog("---> Umbral comparado:" + delta1a2);
                                    Rotar1a2();
                                }
                            }
                        }
                    }
                }
                else
                {
                    ToLog("Fuera de horario para operar.");
                }
            }
            return;
            //tony

            tmrRefresh.Stop();
            // Si NO tengo ninguna posición abierta, abrir desde efectivo
            if (iTenenciaTicker1 == 0 && iTenenciaTicker2 == 0)
            {
                double efectivo = await ObtenerEfectivoDisponible();

                // limitar que use hasta:
                double montoOperacion = Math.Min(efectivo, 100000.00);

                // Comprar el más barato en términos relativos
                if (delta1a2 <= -umbral) // ticker1 barato
                {
                    //int cantidad = CalcularCantidad(montoOperacion, ticker1Ask);
                    int cantidad = 100;
                    if (cantidad > 0)
                    {
                        string op = await Comprar(ticker1, cantidad, ticker1Ask);
                        ToLog($"APERTURA INICIAL: Compré {cantidad} de {ticker1} a {ticker1Ask} => {op}");
                    }
                    tmrRefresh.Start();
                    return;
                }

                if (delta2a1 <= -umbral) // ticker2 barato
                {
                    //int cantidad = CalcularCantidad(montoOperacion, ticker2Ask);
                    int cantidad = 100;
                    if (cantidad > 0)
                    {
                        string op = await Comprar(ticker2, cantidad, ticker2Ask);
                        ToLog($"APERTURA INICIAL: Compré {cantidad} de {ticker2} a {ticker2Ask} => {op}");
                    }
                    tmrRefresh.Start();
                    return;
                }
            }
            tmrRefresh.Start();

        }

        private async Task<double> ObtenerEfectivoDisponible()
        {
            return await _broker.ObtenerEfectivoDisponible();
        }
        //private double ObtenerEfectivoDisponible2()
        //{
        //    string json = GetResponseGET("/api/v2/estadocuenta", bearer);

        //    var estado = JsonConvert.DeserializeObject<EstadoCuenta>(json);

        //    if (estado?.cuentas == null)
        //        return 0;

        //    var cuentaPesos = estado.cuentas
        //        .FirstOrDefault(c => c.moneda == "peso_Argentino");

        //    if (cuentaPesos == null)
        //        return 0;

        //    return cuentaPesos.disponible;
        //}


        private int CalcularCantidad(double efectivoDisponible, double precioAsk)
        {
            if (precioAsk <= 0) return 0;

            int cantidad = (int)Math.Floor(efectivoDisponible / precioAsk);

            // No comprar 0
            if (cantidad < 1) return 0;

            return cantidad;
        }
        public int CalcularCantidadSegura(decimal montoDisponible, decimal precio, decimal factorCosto = 1.001915m)
        {
            // ---- 1) Detectar si el precio viene por lámina (100 nominales)
            //       heurística: precios muy grandes (> 10.000) suelen ser "por lámina".
            decimal precioPorNominal;
            if (precio > 10000m)
            {
                // ejemplo: 95640 -> 956.40 por nominal
                precioPorNominal = precio / 100m;
            }
            else
            {
                precioPorNominal = precio;
            }

            // ---- 2) costo por 1 nominal incluyendo comisiones
            decimal costoUnitario = precioPorNominal * factorCosto;

            // ---- 3) cantidad cruda (nominales) que alcanza
            if (costoUnitario <= 0m)
                return 0;

            int cantidadCruda = (int)Math.Floor(montoDisponible / costoUnitario);

            // ---- 4) devolvemos la cantidad máxima de nominales que se puede comprar
            //      (si querés forzar múltiplos de 100, comentar la siguiente línea y descomentar la de abajo)
            return cantidadCruda;

            // Si querés DEVOLVER únicamente múltiplos de 100 (lotes):
            // int cantidadLotes = (cantidadCruda / 100) * 100;
            // return cantidadLotes;
        }


        private async void RefreshChart()
        {

            using (SqlCommand setLanguageCommand = new SqlCommand("SET LANGUAGE us_english;", oCnn))
            {
                await setLanguageCommand.ExecuteNonQueryAsync();
            }

            sqlCommand = oCnn.CreateCommand();
            sqlCommand.CommandText = "sp_GetDataSet";
            sqlCommand.CommandType = CommandType.StoredProcedure;
            sqlCommand.Parameters.Add("@dt", SqlDbType.DateTime).Value = Ahora();
            sqlCommand.Parameters.Add("@bono1", SqlDbType.VarChar).Value = this.cboTicker1.Text;
            sqlCommand.Parameters.Add("@bono2", SqlDbType.VarChar).Value = this.cboTicker2.Text;
            rdr = sqlCommand.ExecuteReader();

            var dtList = new List<DateTime>();
            var ratioList = new List<double>();
            var mm180List = new List<double>();
            var gdalList = new List<double>();
            var algdList = new List<double>();
            while (rdr.Read())
            {
                dtList.Add(Convert.ToDateTime(rdr["DT"]));
                ratioList.Add(Convert.ToDouble(rdr["Ratio"]));
                mm180List.Add(Convert.ToDouble(rdr["MM180"]));
                gdalList.Add(Convert.ToDouble(rdr["GDAL"]));
                algdList.Add(Convert.ToDouble(rdr["ALGD"]));
            }
            rdr.Close();
            double[] xs = dtList.Select(dt => dt.ToOADate()).ToArray();
            double[] ratioYs = ratioList.ToArray();
            double[] mm180Ys = mm180List.ToArray();
            double[] gdalYs = gdalList.ToArray();
            double[] algdYs = algdList.ToArray();

            crtGrafico.Plot.Clear();
            //crtGrafico.Plot.Add.Scatter(xs, ratioYs, label: "Ratio");
            crtGrafico.Plot.Add.ScatterLine(xs, ratioYs, ScottPlot.Color.FromColor(System.Drawing.Color.Blue));
            var sp = crtGrafico.Plot.Add.ScatterLine(xs, mm180Ys, ScottPlot.Color.FromColor(System.Drawing.Color.Goldenrod));
            sp.LinePattern = LinePattern.Dashed;
            crtGrafico.Plot.Add.ScatterLine(xs, gdalYs, ScottPlot.Color.FromColor(System.Drawing.Color.Green));
            crtGrafico.Plot.Add.ScatterLine(xs, algdYs, ScottPlot.Color.FromColor(System.Drawing.Color.Red));

            crtGrafico.Plot.ShowGrid();
            crtGrafico.Plot.Axes.DateTimeTicksBottom();
            // Configure plot (optional)
            //crtGrafico.Plot.Title("My Plot");
            crtGrafico.Plot.XLabel("");
            crtGrafico.Plot.YLabel("");
            //crtGrafico.Plot.Legend();
            crtGrafico.Plot.Axes.AutoScaleExpand();
            crtGrafico.Refresh();

            sqlCommand = oCnn.CreateCommand();
            sqlCommand.CommandText = "sp_GetData";
            sqlCommand.CommandType = CommandType.StoredProcedure;
            sqlCommand.Parameters.Add("@dt", SqlDbType.DateTime).Value = Ahora();
            sqlCommand.Parameters.Add("@bono1", SqlDbType.VarChar).Value = this.cboTicker1.Text;
            sqlCommand.Parameters.Add("@bono2", SqlDbType.VarChar).Value = this.cboTicker2.Text;
            rdr = sqlCommand.ExecuteReader();
            double Max;
            double Min;
            if (rdr.Read())
            {
                Min = Math.Floor(double.Parse(rdr["Piso"].ToString()));
                Max = Math.Ceiling(double.Parse(rdr["Techo"].ToString()));

                //crtGrafico.Plot.Axes.AutoScale();

                txtLastData.Text = rdr["DT"].ToString();
                txtMax.Text = rdr["Techo"].ToString();
                txtMin.Text = rdr["Piso"].ToString();
                txtRatio.Text = rdr["Ratio"].ToString();
                txtMM.Text = rdr["MM180"].ToString();
                txt1a2.Text = rdr["GDAL"].ToString();
                txt2a1.Text = rdr["ALGD"].ToString();
                decimal desvio = decimal.Parse(rdr["Desvio"].ToString());

                //decimal vol = decimal.Parse(rdr["Volatilidad"].ToString()); //tony
                double vol = (double)rdr["Volatilidad"];

                txtDesvios.Text = desvio.ToString();
                txtVolatilidad.Text = vol.ToString();
                if (chkAutoVol.Checked == true)
                {
                    if ((double)desvio > umbralMinimo)
                    {
                        cboUmbral.Text = desvio.ToString();
                    }
                    else
                    {
                        cboUmbral.Text = umbralMinimo.ToString();
                    }
                }
            }
            rdr.Close();
        }

        private void btnRotar1a2_Click(object sender, EventArgs e)
        {
            Rotar1a2();
        }

        private async void Rotar1a2()
        {
            string ticker1, ticker2;
            int cantidadDesde;
            int cantidadHasta;
            double precioDesde;
            double precioHasta;

            ticker1 = cboTicker1.Text;
            ticker2 = cboTicker2.Text;

            //cantidadDesde = 10;
            cantidadDesde = int.Parse(txtTenenciaTicker1.Text);
            precioDesde = double.Parse(txtTicker1Bid.Text);

            //cantidadHasta = 10;
            cantidadHasta = (int)Math.Ceiling(double.Parse(txtCompraTicker2.Text));
            precioHasta = double.Parse(txtTicker2Ask.Text);

            bool bOperacionOk = false;
            if (cantidadDesde > 0 && precioDesde > 0 && precioHasta > 0 && cantidadHasta > 0)
            {
                bOperacionOk = await Operar(ticker1, cantidadDesde, precioDesde, ticker2, cantidadHasta, precioHasta);
            }

            if (bOperacionOk)
            {
                if (Tenencia1 > 0 || Tenencia2 > 0)
                {
                    Tenencia1 = Tenencia1 - cantidadDesde;
                    Tenencia2 = cantidadHasta;

                    txtTenenciaTicker1.Text = Tenencia1.ToString();
                    txtTenenciaTicker2.Text = Tenencia2.ToString();                    
                }
                //chkAuto.Checked = false;
                Application.DoEvents();
            }
        }

        private void btnRotar2a1_Click(object sender, EventArgs e)
        {
            Rotar2a1();
        }

        private async void Rotar2a1()
        {
            string ticker1, ticker2;
            int cantidadDesde;
            int cantidadHasta;
            double precioDesde;
            double precioHasta;

            ticker1 = cboTicker1.Text;
            ticker2 = cboTicker2.Text;

            //cantidadDesde = 10;
            cantidadDesde = int.Parse(txtTenenciaTicker2.Text);
            precioDesde = double.Parse(txtTicker2Bid.Text);

            //cantidadHasta = 10;
            cantidadHasta = (int)Math.Ceiling(double.Parse(txtCompraTicker1.Text));
            precioHasta = double.Parse(txtTicker1Ask.Text);

            bool bOperacionOk = false;
            if (cantidadDesde > 0 && precioDesde > 0 && precioHasta > 0 && cantidadHasta > 0)
            {
                await Operar(ticker2, cantidadDesde, precioDesde, ticker1, cantidadHasta, precioHasta);
            }

            if (bOperacionOk)
            {
                if (Tenencia1 > 0 || Tenencia2 > 0)
                {
                    Tenencia1 = Tenencia1 - cantidadDesde;
                    Tenencia2 = cantidadHasta;

                    txtTenenciaTicker1.Text = Tenencia1.ToString();
                    txtTenenciaTicker2.Text = Tenencia2.ToString();                    
                }
                //chkAuto.Checked = false;
                Application.DoEvents();
            }
        }

        private async Task<bool> Operar(string ticker1, int cantidadTicker1, double precioTicker1, string ticker2, int cantidadTicker2, double precioTicker2)
        {
            bool bReturn = false;
            LoginIOL();
            //ToLog("Iniciando");

            //ToLog(ticker1 + " Q:" + cantidadTicker1 + " P:" + precioTicker1 + " -> "
            //  + ticker2 + " Q:" + cantidadTicker2 + " P:" + precioTicker2);

            //Application.DoEvents();
            tmrRefresh.Enabled = false;
            tmrRefresh.Stop();

            ToLog("Venta de " + ticker1 + " Q: " + cantidadTicker1 + " P: " + precioTicker1);
            string operacionVenta = await Vender(ticker1, cantidadTicker1, precioTicker1);
            ToLog("Venta Estado Operacion " + operacionVenta);

            if (operacionVenta != "Error")
            {
                string estadooperacion = "";
                for (int i = 1; i <= intentos; i++)
                {
                    ToLog("Intento de venta " + i.ToString() + " de " + ticker1);
                    estadooperacion = await GetEstadoOperacion(operacionVenta);
                    ToLog("Intento " + i.ToString() + " estado " + estadooperacion);
                    if (estadooperacion == "terminada")
                    {
                        ToLog("Venta Terminada!!!");
                        break;
                    }
                    //Application.DoEvents();

                    // 👉 Espera 1 segundos antes del próximo intento
                    await Task.Delay(1000);
                }
                if (estadooperacion == "terminada")
                {
                    string operacionCompra = await Comprar(ticker2, cantidadTicker2, precioTicker2);
                    ToLog("Compra de " + ticker2 + " Q: " + cantidadTicker2 + " P: " + precioTicker2);
                    ToLog("Compra Estado Operacion " + operacionCompra);
                    if (!string.IsNullOrEmpty(operacionCompra) &&
                        operacionCompra.Contains("El monto de la operación excede el", StringComparison.OrdinalIgnoreCase))
                    {
                        double efectivo = await ObtenerEfectivoDisponible();
                        int cantidad = CalcularCantidadSegura((decimal)efectivo, (decimal)precioTicker2);
                        ToLog("RE-Compra de " + ticker2 + " Q: " + cantidad + " P: " + precioTicker2);
                        operacionCompra = await Comprar(ticker2, cantidad, precioTicker2);
                    }

                    if (operacionCompra != "Error")
                    {
                        estadooperacion = await GetEstadoOperacion(operacionCompra);
                        for (int i = 1; i <= intentos; i++)
                        {
                            ToLog("Intento de compra " + i.ToString() + " de " + ticker2);
                            estadooperacion = await GetEstadoOperacion(operacionCompra);
                            ToLog("Intento " + i.ToString() + " estado: " + estadooperacion);
                            if (estadooperacion == "terminada")
                            {
                                bReturn = true;
                                ToLog("Compra Terminada!!!");
                                break;
                            }

                            // 👉 Espera 1 segundo antes del próximo intento
                            await Task.Delay(1000);
                        }
                    }
                }
                else
                {
                    ToLog("Vencio la venta de " + ticker1);
                    ToLog("------------------------------");
                    await _broker.EliminarOperacion(operacionVenta);
                }
            }
            else
            {
                ToLog("Error en la venta de " + ticker1);
                ToLog("------------------------------");
            }
            tmrRefresh.Enabled = true;
            tmrRefresh.Start();
            ToLog("Desocupado");
            ToLog("Fin--------------------------");

            return bReturn;
        }
        private async Task<string> GetEstadoOperacion(string idoperacion)
        {
            return await _broker.GetEstadoOperacion(idoperacion);
        }

        //private string GetEstadoOperacion2(string idoperacion)
        //{
        //    string response;
        //    response = GetResponseGET("/api/v2/operaciones/" + idoperacion, bearer);
        //    if (response.Contains("Error") || response.Contains("Se exced"))
        //    {
        //        return "Error";
        //    }
        //    else
        //    {
        //        dynamic json = JObject.Parse(response);
        //        return json.estadoActual.Value;
        //    }
        //}

        private async Task<string> Comprar(string simbolo, int cantidad, double precio)
        {
            string plazo = cboPlazo.Text == "CI" ? "t0" : "t1";
            return await _broker.Comprar(simbolo, cantidad, precio, plazo);
        }

        private string Comprar2(string simbolo, int cantidad, double precio)
        {
            ToLog("Comprando " + simbolo);
            try
            {
                //Application.DoEvents();
                string validez = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "T17:59:59.000Z";
                string plazo = cboPlazo.Text == "CI" ? "t0" : "t1";

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
                response = GetResponsePOST("/api/v2/operar/Comprar", parametros);
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
                ToLog("Error Metodo Comprar: " + ex.Message);
                return "Error";
            }

        }

        private async Task<string> Vender(string simbolo, int cantidad, double precio)
        {
            string plazo = cboPlazo.Text == "CI" ? "t0" : "t1";
            return await _broker.Vender(simbolo, cantidad, precio, plazo);
        }

        private string Vender2(string simbolo, int cantidad, double precio)
        {
            ToLog("Vendiendo " + simbolo);
            try
            {
                //Application.DoEvents();
                string validez = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "T17:59:59.000Z";
                string plazo = cboPlazo.Text == "CI" ? "t0" : "t1";
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
                response = GetResponsePOST("/api/v2/operar/Vender", parametros);
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
                ToLog("Error Metodo Vender: " + ex.Message);
                return "Error";
            }
        }
        private DateTime Ahora()
        {
            TimeSpan timeSpan = TimeSpan.FromHours(timeOffset);
            DateTime ahora = DateTime.Now.Add(timeSpan);
            return ahora;
        }

        private void tmrToken_Tick(object sender, EventArgs e)
        {
            //ToLog(Math.Round((expires - DateTime.Now).TotalSeconds).ToString());
        }

        private void cboTicker1_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillListaTickers();
        }

        private void cboTicker2_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillListaTickers();
        }
    }

    public class Ticker
    {
        public string NombreLargo { get; set; }
        public string NombreMedio { get; set; }
        public string NombreCorto { get; set; }
        public int bidSize;
        public decimal bid;
        public decimal last;
        public decimal offer;
        public int offerSize;
        public Ticker(string nombrelargo, string nombremedio, string nombrecorto)
        {
            NombreLargo = nombrelargo;
            NombreMedio = nombremedio;
            NombreCorto = nombrecorto;
            bidSize = 0;
            bid = 0;
            last = 0;
            offer = 0;
        }
    }

    public class EstadoCuenta
    {
        public List<Cuenta> cuentas { get; set; } = new List<Cuenta>();
    }

    public class Cuenta
    {
        public string moneda { get; set; } = string.Empty;
        public double saldo { get; set; }
        public double disponible { get; set; }
    }

}
