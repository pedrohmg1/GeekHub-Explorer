using Microsoft.Maui.Controls;
using OMDb_API.Models;
using OMDb_API.Services;

namespace OMDb_API
{
    public partial class LivroDetalhesPage : ContentPage
    {
        private readonly OpenLibraryService _openLibraryService = new();
        private readonly OmdbService _omdbService = new();
        private readonly LivrosFavoritosService _favoritosService = new();
        private Livro _livroAtual;

        public LivroDetalhesPage(Livro livroResumo)
        {
            InitializeComponent();
            _livroAtual = livroResumo;
            _ = CarregarDetalhesAsync();
        }

        private async Task CarregarDetalhesAsync()
        {
            try
            {
                // Preenche dados já obtidos na busca
                TituloLivroDetalhe.Text = _livroAtual.Titulo;
                ResumoPolimorfico.Text = _livroAtual.ObterResumoExibicao();
                CapaLivroDetalhe.Source = _livroAtual.ImagemUrl;
                PaginasLivro.Text = $"Extensão: {_livroAtual.NumeroPaginas}";

                // Busca sinopse e gêneros/assuntos detalhados
                var detalhes = await _openLibraryService.ObterDetalhesAsync(_livroAtual.Id);
                if (detalhes != null)
                {
                    _livroAtual.Sinopse = detalhes.Sinopse;
                    _livroAtual.Assuntos = detalhes.Assuntos;
                }

                SinopseLivroDetalhe.Text = _livroAtual.Sinopse;
                AssuntosLivro.Text = $"Temas / Assuntos: {_livroAtual.Assuntos}";

                AtualizarVisualBotaoFavorito(_favoritosService.EstaNaLista(_livroAtual.Id));
                CartaoLivro.IsVisible = true;

                // Busca filmes/séries adaptados dessa obra na OMDb API
                string termoFilme = _livroAtual.Titulo.Split(':')[0].Split('-')[0].Trim();
                var filmes = await _omdbService.BuscarPorNomeAsync(termoFilme);
                if (filmes.Any())
                {
                    ListaFilmesDoLivro.ItemsSource = filmes.Take(6).ToList();
                    SecaoFilmesDoLivro.IsVisible = true;
                }
            }
            catch
            {
                await DisplayAlert("Erro", "Não foi possível carregar todos os detalhes da obra.", "OK");
            }
            finally
            {
                LoadingDetalhes.IsVisible = false;
                LoadingDetalhes.IsRunning = false;
            }
        }

        private void BotaoFavoritarLivro_Clicked(object sender, EventArgs e)
        {
            bool adicionado = _favoritosService.AlternarItem(_livroAtual);
            AtualizarVisualBotaoFavorito(adicionado);
        }

        private void AtualizarVisualBotaoFavorito(bool naLista)
        {
            if (naLista)
            {
                BotaoFavoritarLivro.Text = "★ Salvo nas Leituras";
                BotaoFavoritarLivro.BackgroundColor = Color.FromArgb("#2ECC71");
                BotaoFavoritarLivro.TextColor = Colors.Black;
            }
            else
            {
                BotaoFavoritarLivro.Text = "☆ Salvar Leitura";
                BotaoFavoritarLivro.BackgroundColor = Color.FromArgb("#2E1065");
                BotaoFavoritarLivro.TextColor = Colors.White;
            }
        }

        private async void ListaFilmesDoLivro_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Filme filme)
            {
                ListaFilmesDoLivro.SelectedItem = null;
                await Navigation.PushAsync(new FilmeDetalhesPage(filme.Id));
            }
        }

        private async void BotaoVoltar_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}