using OMDb_API.Models;
using OMDb_API.Services;

namespace OMDb_API
{
    public partial class PokemonPage : ContentPage
    {
        private readonly PokemonService _pokemonService = new();
        private CancellationTokenSource? _cts;

        public PokemonPage()
        {
            InitializeComponent();
        }

        private async void PokemonEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            string termo = e.NewTextValue?.Trim() ?? "";
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            if (termo.Length < 3)
            {
                ListaPokemon.ItemsSource = null;
                return;
            }

            try
            {
                await Task.Delay(500, token);
                if (!token.IsCancellationRequested)
                {
                    LoadingIndicator.IsVisible = true;
                    LoadingIndicator.IsRunning = true;
                    ListaPokemon.ItemsSource = await _pokemonService.BuscarPorNomeAsync(termo);
                }
            }
            catch (TaskCanceledException)
            {
                // Ignora apenas quando o usuário continua digitando
            }
            catch (Exception ex)
            {
                ListaPokemon.ItemsSource = null;
                await DisplayAlert("Aviso da API Pokémon", $"Não foi possível buscar as cartas agora: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }
        }

        private async void ListaPokemon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is CartaPokemon carta)
            {
                ListaPokemon.SelectedItem = null;
                await Navigation.PushAsync(new PokemonDetalhesPage(carta.Id));
            }
        }
    }
}