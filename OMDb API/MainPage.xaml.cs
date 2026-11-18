using Microsoft.Maui.Controls;
using OMDb_API.Models;
using OMDb_API.Services;

namespace OMDb_API
{
    public partial class MainPage : ContentPage
    {
        private readonly OmdbService _omdbService = new();
        private CancellationTokenSource? _cts;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void FilmeEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            string termo = e.NewTextValue?.Trim() ?? "";
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            if (termo.Length < 3)
            {
                ListaFilmes.ItemsSource = null;
                return;
            }

            try
            {
                await Task.Delay(500, token);
                if (!token.IsCancellationRequested)
                {
                    LoadingIndicator.IsVisible = true;
                    LoadingIndicator.IsRunning = true;
                    ListaFilmes.ItemsSource = await _omdbService.BuscarPorNomeAsync(termo);
                }
            }
            catch (TaskCanceledException) { }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }
        }

        private async void ListaFilmes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Filme filme)
            {
                ListaFilmes.SelectedItem = null;
                // Modo 3: Navegação em Pilha (Stack)
                await Navigation.PushAsync(new FilmeDetalhesPage(filme.Id));
            }
        }
    }
}