using OMDb_API.Models;
using OMDb_API.Services;

namespace OMDb_API
{
    public partial class PokemonDetalhesPage : ContentPage
    {
        private readonly PokemonService _pokemonService = new();
        private readonly CartasFavoritasService _cartasFavoritasService = new();
        private CartaPokemon? _cartaAtual;

        public PokemonDetalhesPage(string cardId)
        {
            InitializeComponent();
            _ = CarregarDadosAsync(cardId);
        }

        private async Task CarregarDadosAsync(string cardId)
        {
            try
            {
                _cartaAtual = await _pokemonService.ObterDetalhesAsync(cardId);
                if (_cartaAtual != null)
                {
                    NomeCartaDetalhe.Text = _cartaAtual.Titulo;
                    ResumoPolimorfico.Text = _cartaAtual.ObterResumoExibicao();
                    DescricaoCarta.Text = _cartaAtual.FlavorText;
                    HpCarta.Text = $"Vida: {_cartaAtual.Hp}";
                    ArtistaCarta.Text = $"Ilustrador: {_cartaAtual.Artista}";
                    AtaquesCarta.Text = $"Ataques: {_cartaAtual.Ataques}";
                    PrecoCarta.Text = $"✨ Raridade: {_cartaAtual.PrecoMercado}";
                    ImagemCartaDetalhe.Source = _cartaAtual.ImagemUrl;

                    AtualizarVisualBotao(_cartasFavoritasService.EstaNaLista(_cartaAtual.Id));
                    CartaoPokemon.IsVisible = true;
                }
            }
            catch
            {
                await DisplayAlert("Erro", "Não foi possível carregar os detalhes da carta.", "OK");
            }
            finally
            {
                LoadingDetalhes.IsVisible = false;
                LoadingDetalhes.IsRunning = false;
            }
        }

        private void BotaoFavoritarCarta_Clicked(object sender, EventArgs e)
        {
            if (_cartaAtual == null) return;

            bool adicionada = _cartasFavoritasService.AlternarItem(_cartaAtual);
            AtualizarVisualBotao(adicionada);
        }

        private void AtualizarVisualBotao(bool naLista)
        {
            if (naLista)
            {
                BotaoFavoritarCarta.Text = "★ Carta Favoritada";
                BotaoFavoritarCarta.BackgroundColor = Color.FromArgb("#2ECC71");
                BotaoFavoritarCarta.TextColor = Colors.Black;
            }
            else
            {
                BotaoFavoritarCarta.Text = "☆ Favoritar Carta";
                BotaoFavoritarCarta.BackgroundColor = Color.FromArgb("#2C2C2C");
                BotaoFavoritarCarta.TextColor = Colors.White;
            }
        }

        private async void BotaoVoltar_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}