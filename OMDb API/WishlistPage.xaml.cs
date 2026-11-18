using Microsoft.Maui.Controls;
using OMDb_API.Models;
using OMDb_API.Services;

namespace OMDb_API
{
    public partial class WishlistPage : ContentPage
    {
        private readonly ListaDesejosService _wishlistService = new();

        public WishlistPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CarregarWishlist();
        }

        private void CarregarWishlist()
        {
            var jogos = _wishlistService.ObterLista();
            ListaWishlist.ItemsSource = jogos;
            MensagemVazia.IsVisible = !jogos.Any();
        }

        private async void ListaWishlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Jogo jogo)
            {
                ListaWishlist.SelectedItem = null;
                await Navigation.PushAsync(new JogoDetalhesPage(jogo.Id));
            }
        }

        private void RemoverWishlist_Clicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is string gameId)
            {
                _wishlistService.Remover(gameId);
                CarregarWishlist();
            }
        }
    }
}