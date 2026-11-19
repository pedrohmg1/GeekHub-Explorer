using OMDb_API.Models;
using OMDb_API.Services;
using System.Text;
using System.Text.Json;

namespace OMDb_API
{
    public partial class PerfilGeekPage : ContentPage
    {
        private readonly FavoritosService _filmesService = new();
        private readonly ListaDesejosService _jogosService = new();
        private readonly CartasFavoritasService _pokemonService = new();
        private readonly LivrosFavoritosService _livrosService = new();

        private readonly OmdbService _omdbApi = new();
        private readonly RawgService _rawgApi = new();
        private readonly PokemonService _tcgdexApi = new();
        private readonly OpenLibraryService _openLibraryApi = new();

        private MidiaBase? _midiaSorteadaAtual;

        public PerfilGeekPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            AtualizarDashboard();
        }

        private void AtualizarDashboard()
        {
            var filmes = _filmesService.ObterLista();
            var jogos = _jogosService.ObterLista();
            var cartas = _pokemonService.ObterLista();
            var livros = _livrosService.ObterLista();

            ContagemFilmes.Text = filmes.Count.ToString();
            ContagemJogos.Text = jogos.Count.ToString();
            ContagemPokemon.Text = cartas.Count.ToString();
            ContagemLivros.Text = livros.Count.ToString();

            int total = filmes.Count + jogos.Count + cartas.Count + livros.Count;
            SubtituloPerfil.Text = $"Total de {total} {(total == 1 ? "item salvo" : "itens salvos")} no seu acervo pessoal";

            if (total > 0)
            {
                double pFilmes = (double)filmes.Count / total;
                double pJogos = (double)jogos.Count / total;
                double pPokemon = (double)cartas.Count / total;
                double pLivros = (double)livros.Count / total;

                BarraFilmes.Progress = pFilmes;
                BarraJogos.Progress = pJogos;
                BarraPokemon.Progress = pPokemon;
                BarraLivros.Progress = pLivros;

                PercFilmes.Text = $"{Math.Round(pFilmes * 100)}%";
                PercJogos.Text = $"{Math.Round(pJogos * 100)}%";
                PercPokemon.Text = $"{Math.Round(pPokemon * 100)}%";
                PercLivros.Text = $"{Math.Round(pLivros * 100)}%";

                var categorias = new Dictionary<string, int>
                {
                    { "Cinéfilo 🎬", filmes.Count },
                    { "Gamer 🎮", jogos.Count },
                    { "Treinador Pokémon TCG 🃏", cartas.Count },
                    { "Leitor / Colecionador de HQs 📚", livros.Count }
                };

                var dominante = categorias.OrderByDescending(c => c.Value).First();
                TituloPerfilDominante.Text = $"Você é {Math.Round((double)dominante.Value / total * 100)}% {dominante.Key}";
            }
            else
            {
                BarraFilmes.Progress = BarraJogos.Progress = BarraPokemon.Progress = BarraLivros.Progress = 0;
                PercFilmes.Text = PercJogos.Text = PercPokemon.Text = PercLivros.Text = "0%";
                TituloPerfilDominante.Text = "Salve itens nas abas para calcular seu DNA Geek!";
            }

            var todasMidias = new List<MidiaBase>();
            todasMidias.AddRange(filmes);
            todasMidias.AddRange(jogos);
            todasMidias.AddRange(cartas);
            todasMidias.AddRange(livros);

            var recentes = todasMidias
                .OrderByDescending(m => m.TimestampAdicionado)
                .Take(8)
                .ToList();

            ListaAtividadeRecente.ItemsSource = recentes;
            MensagemSemAtividade.IsVisible = !recentes.Any();
        }

        private async void BotaoSurpreendaMe_Clicked(object sender, EventArgs e)
        {
            if (sender is Button botaoClicado)
            {
                botaoClicado.IsEnabled = false;
                botaoClicado.Text = "🎲 Buscando...";
            }

            try
            {
                // Coleta todos os IDs já salvos para nunca repetir um item que você já tem
                var idsSalvos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                idsSalvos.UnionWith(_filmesService.ObterLista().Select(x => x.Id));
                idsSalvos.UnionWith(_jogosService.ObterLista().Select(x => x.Id));
                idsSalvos.UnionWith(_pokemonService.ObterLista().Select(x => x.Id));
                idsSalvos.UnionWith(_livrosService.ObterLista().Select(x => x.Id));

                string[] termosFilmes = { "Inception", "Interstellar", "Matrix", "Blade Runner", "Spider-Man", "Lord of the Rings", "Batman", "Dune", "Star Wars", "Alien", "Studio Ghibli", "Back to the Future" };
                string[] termosJogos = { "Zelda", "Witcher", "Hollow Knight", "Cyberpunk", "God of War", "Elden Ring", "Final Fantasy", "Persona", "Hades", "Dark Souls", "Resident Evil", "Mass Effect" };
                string[] termosPokemon = { "Charizard", "Umbreon", "Pikachu", "Mewtwo", "Rayquaza", "Gengar", "Lucario", "Eevee", "Dragonite", "Greninja", "Blastoise", "Gardevoir" };
                string[] termosLivros = { "Dune", "Berserk", "Hobbit", "Watchmen", "Neuromancer", "Akira", "Sandman", "Foundation", "Witcher", "Naruto", "Batman", "One Piece" };

                MidiaBase? itemInedito = null;

                // Tenta até 4 vezes encontrar um item inédito direto nas APIs
                for (int tentativa = 0; tentativa < 4 && itemInedito == null; tentativa++)
                {
                    int categoriaSorteada = Random.Shared.Next(4);

                    if (categoriaSorteada == 0)
                    {
                        string termo = termosFilmes[Random.Shared.Next(termosFilmes.Length)];
                        var lista = await _omdbApi.BuscarPorNomeAsync(termo);
                        var candidatos = lista.Where(x => !idsSalvos.Contains(x.Id) && x.Id != _midiaSorteadaAtual?.Id).ToList();
                        if (candidatos.Any())
                            itemInedito = candidatos[Random.Shared.Next(candidatos.Count)];
                    }
                    else if (categoriaSorteada == 1)
                    {
                        string termo = termosJogos[Random.Shared.Next(termosJogos.Length)];
                        var lista = await _rawgApi.BuscarPorNomeAsync(termo);
                        var candidatos = lista.Where(x => !idsSalvos.Contains(x.Id) && x.Id != _midiaSorteadaAtual?.Id).ToList();
                        if (candidatos.Any())
                            itemInedito = candidatos[Random.Shared.Next(candidatos.Count)];
                    }
                    else if (categoriaSorteada == 2)
                    {
                        string termo = termosPokemon[Random.Shared.Next(termosPokemon.Length)];
                        var lista = await _tcgdexApi.BuscarPorNomeAsync(termo);
                        var candidatos = lista.Where(x => !idsSalvos.Contains(x.Id) && x.Id != _midiaSorteadaAtual?.Id).ToList();
                        if (candidatos.Any())
                            itemInedito = candidatos[Random.Shared.Next(candidatos.Count)];
                    }
                    else
                    {
                        string termo = termosLivros[Random.Shared.Next(termosLivros.Length)];
                        var lista = await _openLibraryApi.BuscarPorNomeAsync(termo);
                        var candidatos = lista.Where(x => !idsSalvos.Contains(x.Id) && x.Id != _midiaSorteadaAtual?.Id).ToList();
                        if (candidatos.Any())
                            itemInedito = candidatos[Random.Shared.Next(candidatos.Count)];
                    }
                }

                if (itemInedito != null)
                {
                    _midiaSorteadaAtual = itemInedito;

                    GridItemSorteado.IsVisible = true;
                    BoxColecaoVaziaSorteio.IsVisible = false;
                    BotaoAbrirDetalhesSorteio.IsVisible = true;

                    LabelCategoriaSorteio.Text = _midiaSorteadaAtual.Categoria.ToUpperInvariant();
                    CapaSorteio.Source = _midiaSorteadaAtual.ImagemUrl;
                    TituloSorteio.Text = _midiaSorteadaAtual.Titulo;
                    ResumoSorteio.Text = _midiaSorteadaAtual.ObterResumoExibicao();
                    StatusSorteio.Text = "✨ Descoberta Inédita (Fora da sua lista)";
                    EstrelasSorteio.Text = $"Nota / Info: {_midiaSorteadaAtual.Nota}";

                    Color corDestaque = _midiaSorteadaAtual switch
                    {
                        Filme => Color.FromArgb("#F5C518"),
                        Jogo => Color.FromArgb("#06B6D4"),
                        CartaPokemon => Color.FromArgb("#EF4444"),
                        Livro => Color.FromArgb("#A855F7"),
                        _ => Color.FromArgb("#8B5CF6")
                    };

                    BordaCapaSorteio.Stroke = corDestaque;
                    LabelCategoriaSorteio.TextColor = corDestaque;
                    OverlaySurpreendaMe.IsVisible = true;
                }
                else
                {
                    await DisplayAlertAsync("Ops!", "Não foi possível buscar uma recomendação nova agora. Verifique sua conexão e tente novamente.", "OK");
                }
            }
            catch
            {
                await DisplayAlertAsync("Erro de Conexão", "Falha ao consultar as APIs para gerar uma recomendação.", "OK");
            }
            finally
            {
                if (sender is Button btn)
                {
                    btn.IsEnabled = true;
                    btn.Text = btn.Text.Contains("Outro") ? "🎲 Sortear Outro" : "🎲 Surpreenda-me";
                }
            }
        }

        private async void BotaoAbrirDetalhesSorteio_Clicked(object sender, EventArgs e)
        {
            if (_midiaSorteadaAtual == null) return;

            var midiaParaAbrir = _midiaSorteadaAtual;
            OverlaySurpreendaMe.IsVisible = false;
            await AbrirDetalhesMidiaAsync(midiaParaAbrir);
        }

        private void FecharModalSorteio_Clicked(object sender, EventArgs e)
        {
            OverlaySurpreendaMe.IsVisible = false;
        }

        private async void BotaoExportar_Clicked(object sender, EventArgs e)
        {
            string opcao = await DisplayActionSheetAsync(
                "Exportar Minha Coleção Geek",
                "Cancelar",
                null,
                "📄 Compartilhar Resumo em Texto",
                "💾 Exportar Backup Completo (.JSON)");

            var filmes = _filmesService.ObterLista();
            var jogos = _jogosService.ObterLista();
            var cartas = _pokemonService.ObterLista();
            var livros = _livrosService.ObterLista();

            if (opcao == "📄 Compartilhar Resumo em Texto")
            {
                var sb = new StringBuilder();
                sb.AppendLine("🏆 MINHA COLEÇÃO — GEEKHUB EXPLORER");
                sb.AppendLine($"🎬 Filmes ({filmes.Count}): {string.Join(", ", filmes.Select(f => f.Titulo))}");
                sb.AppendLine($"🎮 Jogos ({jogos.Count}): {string.Join(", ", jogos.Select(j => j.Titulo))}");
                sb.AppendLine($"🃏 Cartas Pokémon ({cartas.Count}): {string.Join(", ", cartas.Select(c => c.Titulo))}");
                sb.AppendLine($"📚 Livros/HQs ({livros.Count}): {string.Join(", ", livros.Select(l => l.Titulo))}");

                await Share.Default.RequestAsync(new ShareTextRequest
                {
                    Title = "Minha Coleção GeekHub Explorer",
                    Text = sb.ToString()
                });
            }
            else if (opcao == "💾 Exportar Backup Completo (.JSON)")
            {
                var backup = new
                {
                    ExportadoEm = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                    Filmes = filmes,
                    Jogos = jogos,
                    CartasPokemon = cartas,
                    Livros = livros
                };

                string json = JsonSerializer.Serialize(backup, new JsonSerializerOptions { WriteIndented = true });
                string caminhoArquivo = Path.Combine(FileSystem.CacheDirectory, $"GeekHub_Backup_{DateTime.Now:yyyyMMdd_HHmm}.json");
                await File.WriteAllTextAsync(caminhoArquivo, json);

                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Backup JSON — GeekHub Explorer",
                    File = new ShareFile(caminhoArquivo)
                });
            }
        }

        private async void ListaAtividadeRecente_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is MidiaBase midia)
            {
                ListaAtividadeRecente.SelectedItem = null;
                await AbrirDetalhesMidiaAsync(midia);
            }
        }

        private async Task AbrirDetalhesMidiaAsync(MidiaBase midia)
        {
            switch (midia)
            {
                case Filme f:
                    await Navigation.PushAsync(new FilmeDetalhesPage(f.Id));
                    break;
                case Jogo j:
                    await Navigation.PushAsync(new JogoDetalhesPage(j.Id));
                    break;
                case CartaPokemon p:
                    await Navigation.PushAsync(new PokemonDetalhesPage(p.Id));
                    break;
                case Livro l:
                    await Navigation.PushAsync(new LivroDetalhesPage(l));
                    break;
            }
        }
    }
}