
using SeConselhoFosseBom.Class.ApiClients;
using System.Globalization;
using SeConselhoFosseBom.Class.Models;
using System.Diagnostics;
namespace SeConselhoFosseBom
{
    public partial class MainPage : ContentPage
    {
        private static string _languageDefault = "pt-br";
        private readonly static string MsgPadrao = "Se conselho fosse bom, seria vendido 😄";
        private readonly HttpClientHelper _httpClient;
        private Dictionary<string, string> _listLanguages = new Dictionary<string, string>
        {
            { "Portuguese (Brazil)", "pt-br" },
            { "English", "en" },
            { "Spanish", "es" },
            { "French", "fr" },
            { "German", "de" },
            { "Italian", "it" },
            { "Japanese", "ja" },
            { "Chinese (Simplified)", "zh-cn" },
            { "Russian", "ru" }
        };

        public MainPage(HttpClientHelper httpClient)
        {
            _httpClient = httpClient;

            InitializeComponent();
            CarregarIdiomasAsync();
            CarregarDataAsync();
            CarregarTextosAsync();
        }

        private async void CarregarTextosAsync()
        {
            string labelBotao = "Buscar Conselho";

            btnBuscar.Text = await TraduzirAsync(labelBotao) ?? labelBotao;
            lblConselho.Text = await TraduzirAsync(lblConselho.Text) ?? MsgPadrao;
            _languageDefault = _listLanguages?.FirstOrDefault(x => x.Key == pickerIdiomas?.SelectedItem?.ToString()).Value ?? "pt-br";

        }

        private async Task<string?> TraduzirAsync(string texto, string? source = null)
        {
            try
            {
                if (string.IsNullOrEmpty(texto))
                    throw new Exception("Nenhum texto informado para tradução.");

                if (texto == MsgPadrao)
                    return texto;

                string target = _listLanguages?.FirstOrDefault(x => x.Key == pickerIdiomas?.SelectedItem?.ToString()).Value ?? "pt-br";

                if (string.IsNullOrEmpty(source))
                    source = _languageDefault;

                if (target == source)
                    return texto;


                var response = await _httpClient.GetAsync<TranslateResponseModel>("MyMemory", $"/get?q={texto}&langpair={source}|{target}");
                    
                return response?.Matches?.OrderBy(x => Math.Abs(x.Match - 1)).ThenBy(x => Math.Abs(Convert.ToInt32(x.Penalty - 0))).FirstOrDefault()?.Translation;

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao traduzir: {ex.Message}");
                //await DisplayAlert("Erro", $"Não foi possível traduzir o texto. Tente novamente mais tarde.\n{ex.Message}", "OK");
                return texto; 
            }
        }

        private async void CarregarIdiomasAsync()
        {
            try
            {
                pickerIdiomas.ItemsSource = _listLanguages?.Select(x => x.Key).ToList();

                if (pickerIdiomas.Items?.Count == 0)
                    pickerIdiomas.ItemsSource = new string[] { "Portuguese (Brazil)" };

                pickerIdiomas.SelectedItem = "Portuguese (Brazil)";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao carregar idiomas: {ex.Message}");
                await DisplayAlert("Erro", $"Não foi possível carregar os idiomas. Tente novamente mais tarde.\n{ex.Message}", "OK");
                pickerIdiomas.ItemsSource = new string[] { "Portuguese (Brazil)" };

                _languageDefault = _listLanguages?.FirstOrDefault(x => x.Key == CultureInfo.CurrentCulture.TwoLetterISOLanguageName).Value ?? "pt-br";
            }
        }

        private void CarregarDataAsync()
        {
            try
            {
                DateTime data = DateTime.Now;

                string culturaSelecionada = _listLanguages?
                    .FirstOrDefault(x => x.Key == pickerIdiomas.SelectedItem?.ToString()).Value.ToLowerInvariant() ?? "pt-br";

                CultureInfo cultura = new(culturaSelecionada);

                // Nome do dia da semana e do mês, respeitando a cultura
                string diaSemana = cultura.DateTimeFormat.GetDayName(data.DayOfWeek);
                string diaSemanaFormatado = char.ToUpper(diaSemana[0]) + diaSemana[1..];
                string mes = data.ToString("MMMM", cultura);

                // Conectores conforme a cultura
                string conector1 = culturaSelecionada.StartsWith("pt") ? "de" : "of";
                string conector2 = culturaSelecionada.StartsWith("pt") ? "de" : ",";

                // Monta a string final
                string dataFormatada = $"{diaSemanaFormatado}\n{data:dd} {conector1} {mes} {conector2} {data:yyyy}";

                lblData.Text = dataFormatada;

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao formatar data: {ex.Message}");
                lblData.Text = "Erro ao carregar data";
            }
        }

        private async Task<string> ObterConselhoAsync()
        {
            try
            {
                var result = await _httpClient.GetAsync<AdviceResponseModel>("adviceslip", "/advice");
                var conselho = await TraduzirAsync(result?.Slip?.Advice ?? MsgPadrao , "en");

                return conselho ?? MsgPadrao;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao obter conselho: {ex.Message}");
                await DisplayAlert("Erro", "Não foi possível obter um conselho. Tente novamente mais tarde.", "OK");
                return MsgPadrao;
            }
        }
        private async Task ExecutarComLoadingAsync(Label label, ActivityIndicator indicator, Func<Task<string>> metodoAsync)
        {
            indicator.IsVisible = true;
            indicator.IsRunning = true;
            label.Text = "⏳ Carregando...";

            try
            {
                var resultado = await metodoAsync();
                label.Text = resultado;
            }
            catch (Exception)
            {
                label.Text = "❌ Erro ao carregar";
                // log se quiser
            }
            finally
            {
                indicator.IsRunning = false;
                indicator.IsVisible = false;
            }
        }



        private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            ExecutarComLoadingAsync(lblConselho, loadingIndicator, ObterConselhoAsync);
        }

        private void pickerIdiomas_SelectedIndexChanged(object sender, EventArgs e)
        {
            CarregarTextosAsync();
            CarregarDataAsync();
        }


    }

}
