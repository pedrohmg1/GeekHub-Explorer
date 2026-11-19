using OMDb_API.Models;
using OMDb_API.Services;

namespace OMDb_API
{
    public partial class JogoDetalhesPage : ContentPage
    {
        private readonly RawgService _rawgService = new();
        private readonly CrossMediaService _crossMediaService = new();
        private readonly ListaDesejosService _wishlistService = new();
        private Jogo? _jogoAtual;

        public JogoDetalhesPage(string gameId)
        {
            InitializeComponent();
            _ = CarregarDadosAsync(gameId);
        }

        private async Task CarregarDadosAsync(string gameId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Carregando detalhes para o ID: {gameId}");
                _jogoAtual = await _rawgService.ObterDetalhesAsync(gameId);

                if (_jogoAtual != null)
                {
                    TituloJogoDetalhe.Text = _jogoAtual.Titulo;
                    ResumoPolimorfico.Text = _jogoAtual.ObterResumoExibicao();
                    DescricaoJogoDetalhe.Text = _jogoAtual.Descricao;
                    EstudioJogo.Text = $"Estúdio: {_jogoAtual.Desenvolvedora}";
                    PlataformasJogo.Text = $"Plataformas: {_jogoAtual.Plataformas}";
                    NotaRawg.Text = $"★ RAWG: {_jogoAtual.Nota}";
                    CapaJogoDetalhe.Source = _jogoAtual.ImagemUrl;

                    AtualizarVisualBotaoWishlist(_wishlistService.EstaNaLista(_jogoAtual.Id));
                    CartaoJogo.IsVisible = true;

                    // 1. Jogos Similares (RAWG)
                    try
                    {
                        var similares = await _rawgService.BuscarJogosSimilaresAsync(_jogoAtual);
                        if (similares != null && similares.Any())
                        {
                            ListaJogosSimilares.ItemsSource = similares;
                            SecaoJogosSimilares.IsVisible = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ERRO SIMILARES]: {ex.Message}");
                    }

                    // 2. Filmes Relacionados (OMDb)
                    try
                    {
                        var filmesRelacionados = await _crossMediaService.BuscarFilmesDoJogoAsync(_jogoAtual);
                        if (filmesRelacionados != null && filmesRelacionados.Any())
                        {
                            ListaFilmesRelacionados.ItemsSource = filmesRelacionados;
                            SecaoFilmesRelacionados.IsVisible = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ERRO FILMES]: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERRO GERAL CARREGAR]: {ex.Message}");
                await DisplayAlert("Erro", "Não foi possível carregar os detalhes do jogo.", "OK");
            }
            finally
            {
                LoadingDetalhes.IsVisible = false;
                LoadingDetalhes.IsRunning = false;
            }
        }

        private void BotaoWishlist_Clicked(object sender, EventArgs e)
        {
            if (_jogoAtual == null) return;

            bool adicionado = _wishlistService.AlternarItem(_jogoAtual);
            AtualizarVisualBotaoWishlist(adicionado);
        }

        private void AtualizarVisualBotaoWishlist(bool naLista)
        {
            if (naLista)
            {
                BotaoWishlist.Text = "♥ Na Lista de Desejos";
                BotaoWishlist.BackgroundColor = Color.FromArgb("#2ECC71");
                BotaoWishlist.TextColor = Colors.Black;
            }
            else
            {
                BotaoWishlist.Text = "♡ Lista de Desejos";
                BotaoWishlist.BackgroundColor = Color.FromArgb("#2C2C2C");
                BotaoWishlist.TextColor = Colors.White;
            }
        }

        private async void ListaJogosSimilares_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Jogo jogoSimilar)
            {
                ListaJogosSimilares.SelectedItem = null;
                await Navigation.PushAsync(new JogoDetalhesPage(jogoSimilar.Id));
            }
        }

        private async void ListaFilmesRelacionados_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Filme filme)
            {
                ListaFilmesRelacionados.SelectedItem = null;
                await Navigation.PushAsync(new FilmeDetalhesPage(filme.Id));
            }
        }

        private async void BotaoVoltar_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}