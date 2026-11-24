using Microsoft.Maui.Controls;
using OMDb_API.Models;
using OMDb_API.Services;
using System.Diagnostics;

namespace OMDb_API
{
    public partial class JogosPage : ContentPage
    {
        private readonly RawgService _rawgService = new();
        private CancellationTokenSource? _cts;

        public JogosPage()
        {
            InitializeComponent();
        }

        private async void JogoEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            string termo = e.NewTextValue?.Trim() ?? string.Empty;

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            if (termo.Length < 3)
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                ListaJogos.ItemsSource = null;
                return;
            }

            try
            {
                await Task.Delay(500, token);

                if (token.IsCancellationRequested)
                    return;

                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;

                var resultados = await _rawgService.BuscarPorNomeAsync(termo);

                if (!token.IsCancellationRequested)
                {
                    ListaJogos.ItemsSource = resultados;

                    if (resultados.Any())
                    {
                        HistoricoBuscaService.RegistrarBusca("jogos", termo);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Ignora buscas canceladas enquanto o usuário ainda está digitando
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RAWG API Erro]: {ex.Message}");

                if (!token.IsCancellationRequested)
                {
                    ListaJogos.ItemsSource = null;
                    await DisplayAlertAsync("Erro ao buscar jogos", $"Não foi possível consultar a API RAWG.\n\nDetalhes: {ex.Message}", "OK");
                }
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                    LoadingIndicator.IsVisible = false;
                    LoadingIndicator.IsRunning = false;
                }
            }
        }

        private async void ListaJogos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Jogo jogo)
            {
                ListaJogos.SelectedItem = null;

                try
                {
                    await Navigation.PushAsync(new JogoDetalhesPage(jogo.Id));
                }
                catch (Exception ex)
                {
                    await DisplayAlertAsync("Erro", $"Não foi possível abrir os detalhes do jogo: {ex.Message}", "OK");
                }
            }
        }
    }
}