using Microsoft.Maui.Controls;
using OMDb_API.Models;
using OMDb_API.Services;

namespace OMDb_API
{
    public partial class FavoritosPage : ContentPage
    {
        private readonly FavoritosService _favoritosService = new();

        public FavoritosPage()
        {
            InitializeComponent();
        }

        // Recarrega a lista sempre que o usuário entra na aba ou volta dos detalhes
        protected override void OnAppearing()
        {
            base.OnAppearing();
            CarregarFavoritos();
        }

        private void CarregarFavoritos()
        {
            var favoritos = _favoritosService.ObterLista();
            ListaFavoritos.ItemsSource = favoritos;
            MensagemVazia.IsVisible = !favoritos.Any();
        }

        private async void ListaFavoritos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Filme filme)
            {
                ListaFavoritos.SelectedItem = null;
                await Navigation.PushAsync(new FilmeDetalhesPage(filme.Id));
            }
        }

        private void RemoverFavorito_Clicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is string imdbId)
            {
                _favoritosService.Remover(imdbId);
                CarregarFavoritos();
            }
        }
    }
}