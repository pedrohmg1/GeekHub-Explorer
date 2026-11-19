using OMDb_API.Models;
using OMDb_API.Services;

namespace OMDb_API
{
    public partial class FilmeDetalhesPage : ContentPage
    {
        private readonly OmdbService _omdbService = new();
        private readonly CrossMediaService _crossMediaService = new();
        private readonly FavoritosService _favoritosService = new();
        private readonly OpenLibraryService _openLibraryService = new();
        private Filme? _filmeAtual;

        public FilmeDetalhesPage(string imdbId)
        {
            InitializeComponent();
            _ = CarregarDadosAsync(imdbId);
        }

        private async Task CarregarDadosAsync(string imdbId)
        {
            try
            {
                _filmeAtual = await _omdbService.ObterDetalhesAsync(imdbId);
                if (_filmeAtual != null)
                {
                    TituloFilmeDetalhe.Text = _filmeAtual.Titulo;
                    ResumoPolimorfico.Text = _filmeAtual.ObterResumoExibicao();
                    EnredoFilmeDetalhe.Text = _filmeAtual.Sinopse;
                    DiretorFilme.Text = $"Diretor: {_filmeAtual.Diretor}";
                    ElencoFilme.Text = $"Elenco: {_filmeAtual.Elenco}";
                    BilheteriaFilme.Text = $"Bilheteria: {_filmeAtual.Bilheteria}";
                    NotaIMDb.Text = $"★ IMDb: {_filmeAtual.Nota}";
                    CapaFilmeDetalhe.Source = _filmeAtual.ImagemUrl;

                    AtualizarVisualBotaoFavorito(_favoritosService.EstaNaLista(_filmeAtual.Id));
                    CartaoFilme.IsVisible = true;

                    // 1. Busca Jogos Relacionados (RAWG API)
                    try
                    {
                        var jogosRelacionados = await _crossMediaService.BuscarJogosDoFilmeAsync(_filmeAtual);
                        if (jogosRelacionados != null && jogosRelacionados.Any())
                        {
                            ListaJogosRelacionados.ItemsSource = jogosRelacionados;
                            SecaoJogosRelacionados.IsVisible = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ERRO JOGOS RELACIONADOS]: {ex.Message}");
                    }

                    // 2. Busca Livros e HQs Relacionados (Open Library API)
                    try
                    {
                        var livrosRelacionados = await _openLibraryService.BuscarLivrosDoFilmeAsync(_filmeAtual.Titulo);
                        if (livrosRelacionados != null && livrosRelacionados.Any())
                        {
                            ListaLivrosRelacionados.ItemsSource = livrosRelacionados;
                            SecaoLivrosRelacionados.IsVisible = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ERRO OPEN LIBRARY]: {ex.Message}");
                    }
                }
            }
            catch
            {
                await DisplayAlert("Erro", "Não foi possível carregar os detalhes.", "OK");
            }
            finally
            {
                LoadingDetalhes.IsVisible = false;
                LoadingDetalhes.IsRunning = false;
            }
        }

        private void BotaoFavoritar_Clicked(object sender, EventArgs e)
        {
            if (_filmeAtual == null) return;

            bool adicionado = _favoritosService.AlternarItem(_filmeAtual);
            AtualizarVisualBotaoFavorito(adicionado);
        }

        private void AtualizarVisualBotaoFavorito(bool naLista)
        {
            if (naLista)
            {
                BotaoFavoritar.Text = "★ Salvo em Assistir no Futuro";
                BotaoFavoritar.BackgroundColor = Color.FromArgb("#2ECC71");
                BotaoFavoritar.TextColor = Colors.Black;
            }
            else
            {
                BotaoFavoritar.Text = "☆ Assistir no Futuro";
                BotaoFavoritar.BackgroundColor = Color.FromArgb("#2C2C2C");
                BotaoFavoritar.TextColor = Colors.White;
            }
        }

        private async void ListaJogosRelacionados_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Jogo jogo)
            {
                ListaJogosRelacionados.SelectedItem = null;
                await Navigation.PushAsync(new JogoDetalhesPage(jogo.Id));
            }
        }

        private async void ListaLivrosRelacionados_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Livro livroSelecionado)
            {
                ListaLivrosRelacionados.SelectedItem = null;
                await Navigation.PushAsync(new LivroDetalhesPage(livroSelecionado));
            }
        }

        private async void BotaoVoltar_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}