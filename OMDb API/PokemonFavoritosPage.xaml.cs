using Microsoft.Maui.Controls;
using OMDb_API.Models;
using OMDb_API.Services;

namespace OMDb_API
{
    public partial class PokemonFavoritosPage : ContentPage
    {
        private readonly CartasFavoritasService _cartasFavoritasService = new();

        public PokemonFavoritosPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CarregarCartas();
        }

        private void CarregarCartas()
        {
            var cartas = _cartasFavoritasService.ObterLista();
            ListaCartasFavoritas.ItemsSource = cartas;
            MensagemVazia.IsVisible = !cartas.Any();
        }

        private async void ListaCartasFavoritas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is CartaPokemon carta)
            {
                ListaCartasFavoritas.SelectedItem = null;
                await Navigation.PushAsync(new PokemonDetalhesPage(carta.Id));
            }
        }

        private void RemoverCarta_Clicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is string cardId)
            {
                _cartasFavoritasService.Remover(cardId);
                CarregarCartas();
            }
        }
    }
}