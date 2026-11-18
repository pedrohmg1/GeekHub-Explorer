using Microsoft.Maui.Controls;
using OMDb_API.Models;
using OMDb_API.Services;

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
            string termo = e.NewTextValue?.Trim() ?? "";
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            if (termo.Length < 3)
            {
                ListaJogos.ItemsSource = null;
                return;
            }

            try
            {
                await Task.Delay(500, token);
                if (!token.IsCancellationRequested)
                {
                    LoadingIndicator.IsVisible = true;
                    LoadingIndicator.IsRunning = true;
                    ListaJogos.ItemsSource = await _rawgService.BuscarPorNomeAsync(termo);
                }
            }
            catch (TaskCanceledException) { }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }
        }

        private async void ListaJogos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Jogo jogo)
            {
                ListaJogos.SelectedItem = null;
                await Navigation.PushAsync(new JogoDetalhesPage(jogo.Id));
            }
        }
    }
}