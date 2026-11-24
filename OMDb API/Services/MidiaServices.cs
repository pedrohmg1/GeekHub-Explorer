using OMDb_API.Models;
using System.Security.Authentication;
using System.Text.Json;

namespace OMDb_API.Services
{
    public interface IMidiaService<T> where T : MidiaBase
    {
        Task<List<T>> BuscarPorNomeAsync(string termo);
        Task<T?> ObterDetalhesAsync(string id);
    }

    internal static class ApiHttpClient
    {
        public static HttpClient Criar(int timeoutSegundos = 15)
        {
            var handler = new HttpClientHandler
            {
                SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
            };

#if DEBUG
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif

            return new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(timeoutSegundos)
            };
        }
    }

    // ==========================================
    // SERVIÇO 1: OMDb API (FILMES + SUPORTE A TÍTULOS PT-BR)
    // ==========================================
    public class OmdbService : IMidiaService<Filme>
    {
        private readonly HttpClient _httpClient = ApiHttpClient.Criar(12);
        private const string ApiKey = "206fd699";

        private static readonly Dictionary<string, string> TitulosPtParaEn = new(StringComparer.OrdinalIgnoreCase)
        {
            { "coringa", "Joker" },
            { "vingadores", "Avengers" },
            { "os vingadores", "The Avengers" },
            { "homem aranha", "Spider-Man" },
            { "homem-aranha", "Spider-Man" },
            { "homem de ferro", "Iron Man" },
            { "capitão américa", "Captain America" },
            { "capitao america", "Captain America" },
            { "guardiões da galáxia", "Guardians of the Galaxy" },
            { "guardioes da galaxia", "Guardians of the Galaxy" },
            { "o cavaleiro das trevas", "The Dark Knight" },
            { "batman o cavaleiro das trevas", "The Dark Knight" },
            { "o poderoso chefão", "The Godfather" },
            { "o poderoso chefao", "The Godfather" },
            { "a origem", "Inception" },
            { "interestelar", "Interstellar" },
            { "clube da luta", "Fight Club" },
            { "de volta para o futuro", "Back to the Future" },
            { "senhor dos anéis", "The Lord of the Rings" },
            { "senhor dos aneis", "The Lord of the Rings" },
            { "o senhor dos anéis", "The Lord of the Rings" },
            { "o senhor dos aneis", "The Lord of the Rings" },
            { "guerra nas estrelas", "Star Wars" },
            { "velozes e furiosos", "Fast & Furious" },
            { "um sonho de liberdade", "The Shawshank Redemption" },
            { "a espera de um milagre", "The Green Mile" },
            { "o rei leão", "The Lion King" },
            { "o rei leao", "The Lion King" },
            { "procurando nemo", "Finding Nemo" },
            { "divertida mente", "Inside Out" },
            { "os incríveis", "The Incredibles" },
            { "os incriveis", "The Incredibles" },
            { "meu malvado favorito", "Despicable Me" },
            { "a era do gelo", "Ice Age" },
            { "se beber não case", "The Hangover" },
            { "se beber nao case", "The Hangover" },
            { "as branquelas", "White Chicks" },
            { "esqueceram de mim", "Home Alone" },
            { "curtindo a vida adoidado", "Ferris Bueller's Day Off" },
            { "o exterminador do futuro", "The Terminator" },
            { "duro de matar", "Die Hard" },
            { "caça-fantasmas", "Ghostbusters" },
            { "caca fantasmas", "Ghostbusters" },
            { "tubarão", "Jaws" },
            { "tubarao", "Jaws" },
            { "o iluminado", "The Shining" },
            { "invocação do mal", "The Conjuring" },
            { "invocacao do mal", "The Conjuring" },
            { "it a coisa", "It" },
            { "pânico", "Scream" },
            { "panico", "Scream" },
            { "jogos mortais", "Saw" },
            { "jogos vorazes", "The Hunger Games" },
            { "crepúsculo", "Twilight" },
            { "crepusculo", "Twilight" },
            { "piratas do caribe", "Pirates of the Caribbean" },
            { "planeta dos macacos", "Planet of the Apes" },
            { "o lobo de wall street", "The Wolf of Wall Street" },
            { "bastardos inglórios", "Inglourious Basterds" },
            { "bastardos inglorios", "Inglourious Basterds" },
            { "django livre", "Django Unchained" },
            { "o grande truque", "The Prestige" },
            { "duna", "Dune" },
            { "parasita", "Parasite" },
            { "a viagem de chihiro", "Spirited Away" },
            { "meu amigo totoro", "My Neighbor Totoro" }
        };

        private async Task<string> TraduzirTituloParaInglesAsync(string termoOriginal)
        {
            string limpo = termoOriginal.Trim();

            if (TitulosPtParaEn.TryGetValue(limpo, out string? tituloMapeado))
                return tituloMapeado;

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                string urlTraducao = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(limpo)}&langpair=pt|en";
                string json = await _httpClient.GetStringAsync(urlTraducao, cts.Token);

                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("responseData", out var responseData) &&
                    responseData.TryGetProperty("translatedText", out var translatedProp))
                {
                    string? traduzido = translatedProp.GetString()?.Trim();
                    if (!string.IsNullOrWhiteSpace(traduzido) &&
                        !traduzido.Contains("MYMEMORY", StringComparison.OrdinalIgnoreCase))
                    {
                        return traduzido;
                    }
                }
            }
            catch
            {
                // Mantém o termo original se o tradutor falhar ou exceder 3 segundos
            }

            return limpo;
        }

        private async Task<List<Filme>> ConsultarOmdbDiretoAsync(string termo)
        {
            string url = $"https://www.omdbapi.com/?s={Uri.EscapeDataString(termo)}&apikey={ApiKey}";
            string json = await _httpClient.GetStringAsync(url);
            var dto = JsonSerializer.Deserialize<OmdbSearchResponseDto>(json);

            if (dto?.Response != "True" || dto.Search == null)
                return new List<Filme>();

            return dto.Search
                .Where(f => !string.IsNullOrEmpty(f.Poster) && f.Poster != "N/A")
                .Select(f => new Filme
                {
                    Id = f.ImdbID ?? string.Empty,
                    Titulo = f.Title ?? "Sem título",
                    Ano = f.Year ?? "N/A",
                    ImagemUrl = f.Poster ?? string.Empty
                }).ToList();
        }

        public async Task<List<Filme>> BuscarPorNomeAsync(string termo)
        {
            if (string.IsNullOrWhiteSpace(termo))
                return new List<Filme>();

            string termoOriginal = termo.Trim();
            string termoIngles = await TraduzirTituloParaInglesAsync(termoOriginal);

            if (string.Equals(termoOriginal, termoIngles, StringComparison.OrdinalIgnoreCase))
            {
                return await ConsultarOmdbDiretoAsync(termoOriginal);
            }

            var tarefaIngles = ConsultarOmdbDiretoAsync(termoIngles);
            var tarefaOriginal = ConsultarOmdbDiretoAsync(termoOriginal);

            await Task.WhenAll(tarefaIngles, tarefaOriginal);

            var combinados = new List<Filme>();
            var idsAdicionados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var filme in tarefaIngles.Result.Concat(tarefaOriginal.Result))
            {
                if (!string.IsNullOrEmpty(filme.Id) && idsAdicionados.Add(filme.Id))
                {
                    combinados.Add(filme);
                }
            }

            return combinados;
        }

        public async Task<Filme?> ObterDetalhesAsync(string id)
        {
            string url = $"https://www.omdbapi.com/?i={id}&apikey={ApiKey}";
            string json = await _httpClient.GetStringAsync(url);
            var dto = JsonSerializer.Deserialize<OmdbDetailDto>(json);

            if (dto == null || dto.Response == "False")
                return null;

            return new Filme
            {
                Id = dto.ImdbID ?? id,
                Titulo = dto.Title ?? "Sem título",
                Ano = dto.Year ?? "N/A",
                Duracao = dto.Runtime ?? "N/A",
                Genero = dto.Genre ?? "N/A",
                Diretor = dto.Director ?? "N/A",
                Elenco = dto.Actors ?? "N/A",
                Sinopse = dto.Plot ?? "Sem sinopse.",
                Bilheteria = dto.BoxOffice ?? "N/A",
                Nota = dto.ImdbRating ?? "N/A",
                ImagemUrl = dto.Poster != "N/A" ? (dto.Poster ?? "") : ""
            };
        }
    }

    // ==========================================
    // SERVIÇO 2: RAWG.io API (BUSCA PRECISA + SEM FANGAMES + ANTI-NSFW)
    // ==========================================
    public class RawgService : IMidiaService<Jogo>
    {
        private readonly HttpClient _httpClient = ApiHttpClient.Criar(15);
        private const string ApiKey = "3a0594ffd6cc4ffba21454ed25009ecc";

        private static readonly HashSet<string> TagsBloqueadas = new(StringComparer.OrdinalIgnoreCase)
        {
            "nsfw", "hentai", "sexual-content", "erotic", "erotica",
            "porn", "adult", "adult-only", "ecchi", "lewd",
            "fangame", "fan-game", "mod", "parody", "meme"
        };

        private static readonly HashSet<string> TagsGenericasIgnoradas = new(StringComparer.OrdinalIgnoreCase)
        {
            "singleplayer", "multiplayer", "co-op", "online-co-op", "local-co-op",
            "steam-achievements", "steam-cloud", "steam-trading-cards",
            "full-controller-support", "partial-controller-support", "controller",
            "atmospheric", "great-soundtrack", "story-rich", "3d", "2d",
            "first-person", "third-person", "exclusive", "true-exclusive"
        };

        private static bool EhJogoSeguro(RawgItemDto g)
        {
            if (string.IsNullOrEmpty(g.BackgroundImage))
                return false;

            if (string.Equals(g.EsrbRating?.Slug, "adults-only", StringComparison.OrdinalIgnoreCase))
                return false;

            if (g.Tags != null && g.Tags.Any(t => !string.IsNullOrEmpty(t.Slug) && TagsBloqueadas.Contains(t.Slug)))
                return false;

            if (!string.IsNullOrWhiteSpace(g.Name) &&
                System.Text.RegularExpressions.Regex.IsMatch(
                    g.Name,
                    @"\b(hentai|porn|xxx|nude|naked|erotic|lewd|boobs|milf|strip|waifu|sex|nsfw|fangame|fan-game|parody)\b",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                return false;
            }

            return true;
        }

        public async Task<List<Jogo>> BuscarPorNomeAsync(string termo)
        {
            string termoLimpo = termo.Trim();
            string url = $"https://api.rawg.io/api/games?key={ApiKey}&search={Uri.EscapeDataString(termoLimpo)}&search_precise=true&exclude_additions=true&page_size=35";
            string json = await _httpClient.GetStringAsync(url);
            var dto = JsonSerializer.Deserialize<RawgSearchResponseDto>(json);

            if (dto?.Results == null)
                return new List<Jogo>();

            var filtrados = dto.Results
                .Where(g => EhJogoSeguro(g) && g.Added >= 15)
                .ToList();

            if (!filtrados.Any())
            {
                filtrados = dto.Results.Where(EhJogoSeguro).ToList();
            }

            return filtrados
                .OrderByDescending(g => string.Equals(g.Name, termoLimpo, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(g => g.Name != null && g.Name.StartsWith(termoLimpo, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(g => g.Added)
                .Take(12)
                .Select(MapearResumo)
                .ToList();
        }

        public async Task<Jogo?> ObterDetalhesAsync(string id)
        {
            string url = $"https://api.rawg.io/api/games/{id}?key={ApiKey}";
            string json = await _httpClient.GetStringAsync(url);
            var dto = JsonSerializer.Deserialize<RawgDetailDto>(json);

            if (dto == null)
                return null;

            string generoPrincipal = dto.Genres?
                .Where(x => !string.IsNullOrEmpty(x.Slug))
                .OrderBy(x => x.Slug is "action" or "indie" or "adventure" ? 1 : 0)
                .Select(x => x.Slug!)
                .FirstOrDefault() ?? string.Empty;

            var tagsEspecificas = dto.Tags?
                .Where(x => !string.IsNullOrEmpty(x.Slug) && !TagsGenericasIgnoradas.Contains(x.Slug!))
                .Select(x => x.Slug!)
                .Take(2)
                .ToList() ?? new List<string>();

            return new Jogo
            {
                Id = dto.Id.ToString(),
                Titulo = dto.Name ?? "Sem nome",
                Ano = dto.Released ?? "N/A",
                ImagemUrl = dto.BackgroundImage ?? string.Empty,
                Descricao = string.IsNullOrWhiteSpace(dto.DescriptionRaw) ? "Sem descrição." : dto.DescriptionRaw,
                Nota = $"{dto.Rating:0.0} / 5",
                Metacritic = dto.Metacritic?.ToString() ?? "N/A",
                Generos = dto.Genres != null ? string.Join(", ", dto.Genres.Select(x => x.Name)) : "N/A",
                GenerosSlugs = generoPrincipal,
                TagsSlugs = string.Join(",", tagsEspecificas),
                Desenvolvedora = dto.Developers != null && dto.Developers.Any()
                    ? string.Join(", ", dto.Developers.Select(x => x.Name))
                    : "N/A",
                Plataformas = dto.Platforms != null && dto.Platforms.Any()
                    ? string.Join(", ", dto.Platforms.Select(x => x.Platform?.Name).Where(n => n != null))
                    : "N/A"
            };
        }

        public async Task<List<Jogo>> BuscarJogosSimilaresAsync(Jogo jogo)
        {
            var candidatos = new List<RawgItemDto>();

            if (!string.IsNullOrWhiteSpace(jogo.GenerosSlugs) || !string.IsNullOrWhiteSpace(jogo.TagsSlugs))
            {
                string paramGenero = !string.IsNullOrWhiteSpace(jogo.GenerosSlugs)
                    ? $"&genres={Uri.EscapeDataString(jogo.GenerosSlugs)}"
                    : string.Empty;

                string paramTags = !string.IsNullOrWhiteSpace(jogo.TagsSlugs)
                    ? $"&tags={Uri.EscapeDataString(jogo.TagsSlugs)}"
                    : string.Empty;

                string urlPrecisa = $"https://api.rawg.io/api/games?key={ApiKey}{paramGenero}{paramTags}&ordering=-metacritic&page_size=25";
                string jsonPreciso = await _httpClient.GetStringAsync(urlPrecisa);
                var dtoPreciso = JsonSerializer.Deserialize<RawgSearchResponseDto>(jsonPreciso);

                if (dtoPreciso?.Results != null)
                {
                    candidatos.AddRange(dtoPreciso.Results.Where(g => g.Id.ToString() != jogo.Id && EhJogoSeguro(g) && g.Added >= 15));
                }
            }

            if (candidatos.Count < 6 && !string.IsNullOrWhiteSpace(jogo.GenerosSlugs))
            {
                string urlGenero = $"https://api.rawg.io/api/games?key={ApiKey}&genres={Uri.EscapeDataString(jogo.GenerosSlugs)}&ordering=-metacritic&page_size=30";
                string jsonGenero = await _httpClient.GetStringAsync(urlGenero);
                var dtoGenero = JsonSerializer.Deserialize<RawgSearchResponseDto>(jsonGenero);

                if (dtoGenero?.Results != null)
                {
                    var idsJaAdicionados = candidatos.Select(c => c.Id).ToHashSet();
                    candidatos.AddRange(dtoGenero.Results
                        .Where(g => g.Id.ToString() != jogo.Id && !idsJaAdicionados.Contains(g.Id) && EhJogoSeguro(g) && g.Added >= 15));
                }
            }

            return candidatos
                .Take(18)
                .OrderBy(_ => Random.Shared.Next())
                .Take(6)
                .Select(MapearResumo)
                .ToList();
        }

        private static Jogo MapearResumo(RawgItemDto g)
        {
            return new Jogo
            {
                Id = g.Id.ToString(),
                Titulo = g.Name ?? "Sem nome",
                Ano = (!string.IsNullOrEmpty(g.Released) && g.Released.Length >= 4) ? g.Released[..4] : "N/A",
                ImagemUrl = g.BackgroundImage ?? string.Empty,
                Nota = $"{g.Rating:0.0}"
            };
        }
    }

    // ==========================================
    // SERVIÇO 3: INTEGRAÇÃO ENTRE AS DUAS APIS
    // ==========================================
    public class CrossMediaService
    {
        private readonly OmdbService _omdbService = new();
        private readonly RawgService _rawgService = new();

        private static string ExtrairTermoFranquia(string titulo)
        {
            string baseTitulo = titulo.Split(':')[0].Split('-')[0].Trim();
            return baseTitulo.Length >= 3 ? baseTitulo : titulo;
        }

        public async Task<List<Jogo>> BuscarJogosDoFilmeAsync(Filme filme)
        {
            string termo = ExtrairTermoFranquia(filme.Titulo);
            var jogos = await _rawgService.BuscarPorNomeAsync(termo);
            return jogos.Take(6).ToList();
        }

        public async Task<List<Filme>> BuscarFilmesDoJogoAsync(Jogo jogo)
        {
            string termo = ExtrairTermoFranquia(jogo.Titulo);
            var filmes = await _omdbService.BuscarPorNomeAsync(termo);
            return filmes.Take(6).ToList();
        }
    }

    // ==========================================
    // SERVIÇO 4: REPOSITÓRIO GENÉRICO LOCAL (HERANÇA + GENERICS)
    // ==========================================
    public abstract class RepositorioLocalService<T> where T : MidiaBase
    {
        protected abstract string StorageKey { get; }

        public List<T> ObterLista()
        {
            string json = Preferences.Default.Get(StorageKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
                return new List<T>();

            return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
        }

        private void SalvarLista(List<T> lista)
        {
            string json = JsonSerializer.Serialize(lista);
            Preferences.Default.Set(StorageKey, json);
        }

        public bool EstaNaLista(string id)
        {
            return ObterLista().Any(item => item.Id == id);
        }

        public T? ObterItem(string id)
        {
            return ObterLista().FirstOrDefault(item => item.Id == id);
        }

        public bool AlternarItem(T item)
        {
            var lista = ObterLista();
            var existente = lista.FirstOrDefault(x => x.Id == item.Id);

            if (existente != null)
            {
                lista.Remove(existente);
                SalvarLista(lista);
                return false;
            }

            item.DataAdicionado = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            item.TimestampAdicionado = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            lista.Insert(0, item);
            SalvarLista(lista);
            return true;
        }

        public void AtualizarAvaliacaoPessoal(string id, string status, int notaPessoal, string anotacao)
        {
            var lista = ObterLista();
            var item = lista.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                item.StatusConsumo = status;
                item.NotaPessoal = notaPessoal;
                item.AnotacaoPessoal = anotacao;
                SalvarLista(lista);
            }
        }

        public void Remover(string id)
        {
            var lista = ObterLista();
            lista.RemoveAll(x => x.Id == id);
            SalvarLista(lista);
        }

        public void ImportarLista(List<T>? itensImportados, bool substituir)
        {
            if (itensImportados == null) return;

            if (substituir)
            {
                SalvarLista(itensImportados);
                return;
            }

            var listaAtual = ObterLista();
            var idsExistentes = new HashSet<string>(listaAtual.Select(x => x.Id), StringComparer.OrdinalIgnoreCase);

            foreach (var item in itensImportados)
            {
                if (!string.IsNullOrEmpty(item.Id) && idsExistentes.Add(item.Id))
                {
                    listaAtual.Add(item);
                }
            }

            SalvarLista(listaAtual);
        }
    }

    // Especialização 1: Filmes (Assistir no Futuro)
    public class FavoritosService : RepositorioLocalService<Filme>
    {
        protected override string StorageKey => "filmes_assistir_futuro";
    }

    // Especialização 2: Jogos (Lista de Desejos)
    public class ListaDesejosService : RepositorioLocalService<Jogo>
    {
        protected override string StorageKey => "jogos_lista_desejos";
    }

    // Especialização 3: Cartas Pokémon Favoritas
    public class CartasFavoritasService : RepositorioLocalService<CartaPokemon>
    {
        protected override string StorageKey => "pokemon_cartas_favoritas";
    }

    // Especialização 4: Livros Favoritos
    public class LivrosFavoritosService : RepositorioLocalService<Livro>
    {
        protected override string StorageKey => "livros_favoritos";
    }

    // ==========================================
    // SERVIÇO AUXILIAR: HISTÓRICO DE BUSCAS POR ABA
    // ==========================================
    public static class HistoricoBuscaService
    {
        public static List<string> ObterHistorico(string categoria)
        {
            string json = Preferences.Default.Get($"historico_{categoria}", string.Empty);
            if (string.IsNullOrWhiteSpace(json))
                return new List<string>();

            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }

        public static void RegistrarBusca(string categoria, string termo)
        {
            if (string.IsNullOrWhiteSpace(termo)) return;

            var lista = ObterHistorico(categoria);
            lista.RemoveAll(x => string.Equals(x, termo.Trim(), StringComparison.OrdinalIgnoreCase));
            lista.Insert(0, termo.Trim());

            if (lista.Count > 5)
                lista = lista.Take(5).ToList();

            Preferences.Default.Set($"historico_{categoria}", JsonSerializer.Serialize(lista));
        }
    }

    // ==========================================
    // SERVIÇO 5: TCGDEX POKÉMON API (SEM CHAVE E ESTÁVEL)
    // ==========================================
    public class PokemonService : IMidiaService<CartaPokemon>
    {
        private readonly HttpClient _httpClient = ApiHttpClient.Criar(15);

        public async Task<List<CartaPokemon>> BuscarPorNomeAsync(string termo)
        {
            string termoLimpo = Uri.EscapeDataString(termo.Trim());
            string url = $"https://api.tcgdex.net/v2/en/cards?name={termoLimpo}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"A API Pokémon respondeu com status {(int)response.StatusCode}.");
            }

            string json = await response.Content.ReadAsStringAsync();
            var lista = JsonSerializer.Deserialize<List<TcgdexCardResumoDto>>(json);

            if (lista == null)
                return new List<CartaPokemon>();

            return lista
                .Where(c => !string.IsNullOrEmpty(c.Image))
                .Take(16)
                .Select(c => new CartaPokemon
                {
                    Id = c.Id ?? string.Empty,
                    Titulo = c.Name ?? "Sem nome",
                    ImagemUrl = $"{c.Image}/low.png",
                    Colecao = $"Código: {c.Id}"
                })
                .ToList();
        }

        public async Task<CartaPokemon?> ObterDetalhesAsync(string id)
        {
            string url = $"https://api.tcgdex.net/v2/en/cards/{Uri.EscapeDataString(id)}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            string json = await response.Content.ReadAsStringAsync();
            var c = JsonSerializer.Deserialize<TcgdexCardDetalheDto>(json);

            if (c == null)
                return null;

            string ataquesFormatados = "Nenhum ataque listado";
            if (c.Attacks != null && c.Attacks.Any())
            {
                ataquesFormatados = string.Join(" | ", c.Attacks.Select(a =>
                {
                    string dano = (a.Damage.HasValue && a.Damage.Value.ValueKind != JsonValueKind.Null)
                        ? a.Damage.Value.ToString()
                        : "";
                    return string.IsNullOrEmpty(dano) ? (a.Name ?? "") : $"{a.Name} ({dano})";
                }));
            }

            return new CartaPokemon
            {
                Id = c.Id ?? id,
                Titulo = c.Name ?? "Sem nome",
                Ano = (!string.IsNullOrEmpty(c.Set?.ReleaseDate) && c.Set.ReleaseDate.Length >= 4)
                    ? c.Set.ReleaseDate[..4]
                    : "N/A",
                ImagemUrl = !string.IsNullOrEmpty(c.Image) ? $"{c.Image}/high.png" : string.Empty,
                Hp = c.Hp.HasValue ? $"HP {c.Hp.Value}" : "N/A",
                Nota = c.Hp.HasValue ? $"HP {c.Hp.Value}" : "N/A",
                Tipos = c.Types != null && c.Types.Any() ? string.Join(", ", c.Types) : "Incolor",
                Colecao = c.Set?.Name ?? "Coleção Oficial",
                Raridade = c.Rarity ?? "Padrão",
                Artista = c.Illustrator ?? "Desconhecido",
                FlavorText = !string.IsNullOrWhiteSpace(c.Description)
                    ? $"\"{c.Description}\""
                    : "Carta oficial do Pokémon Trading Card Game.",
                Ataques = ataquesFormatados,
                PrecoMercado = c.Rarity ?? "Carta Oficial"
            };
        }
    }

    // ==========================================
    // SERVIÇO 6: OPEN LIBRARY API (LIVROS, HQs E MANGÁS)
    // ==========================================
    public class OpenLibraryService : IMidiaService<Livro>
    {
        private readonly HttpClient _httpClient = ApiHttpClient.Criar(15);

        public async Task<List<Livro>> BuscarPorNomeAsync(string termo)
        {
            if (string.IsNullOrWhiteSpace(termo))
                return new List<Livro>();

            string url = $"https://openlibrary.org/search.json?q={Uri.EscapeDataString(termo.Trim())}&limit=20";
            string json = await _httpClient.GetStringAsync(url);
            var dto = JsonSerializer.Deserialize<OpenLibrarySearchDto>(json);

            if (dto?.Docs == null)
                return new List<Livro>();

            return dto.Docs
                .Where(d => d.CoverId.HasValue && !string.IsNullOrWhiteSpace(d.Key) && !string.IsNullOrWhiteSpace(d.Title))
                .Take(16)
                .Select(d => new Livro
                {
                    Id = d.Key!.Replace("/works/", ""),
                    Titulo = d.Title!,
                    Autor = d.AuthorName != null && d.AuthorName.Any() ? d.AuthorName.First() : "Autor desconhecido",
                    Ano = d.FirstPublishYear?.ToString() ?? "N/A",
                    NumeroPaginas = d.NumberOfPagesMedian.HasValue ? $"{d.NumberOfPagesMedian.Value} págs." : "N/A",
                    ImagemUrl = $"https://covers.openlibrary.org/b/id/{d.CoverId!.Value}-M.jpg"
                })
                .ToList();
        }

        public async Task<Livro?> ObterDetalhesAsync(string id)
        {
            string workId = id.StartsWith("/works/") ? id : $"/works/{id}";
            string url = $"https://openlibrary.org{workId}.json";

            string json = await _httpClient.GetStringAsync(url);
            var dto = JsonSerializer.Deserialize<OpenLibraryWorkDetailDto>(json);

            if (dto == null)
                return null;

            string sinopse = "Sem descrição detalhada disponível na Open Library.";
            if (dto.Description.HasValue)
            {
                if (dto.Description.Value.ValueKind == JsonValueKind.String)
                {
                    sinopse = dto.Description.Value.GetString() ?? sinopse;
                }
                else if (dto.Description.Value.ValueKind == JsonValueKind.Object &&
                         dto.Description.Value.TryGetProperty("value", out var valProp))
                {
                    sinopse = valProp.GetString() ?? sinopse;
                }
            }

            string capaUrl = (dto.Covers != null && dto.Covers.Any())
                ? $"https://covers.openlibrary.org/b/id/{dto.Covers.First()}-L.jpg"
                : string.Empty;

            return new Livro
            {
                Id = id.Replace("/works/", ""),
                Titulo = dto.Title ?? "Sem título",
                Sinopse = sinopse,
                Assuntos = dto.Subjects != null && dto.Subjects.Any()
                    ? string.Join(", ", dto.Subjects.Take(6))
                    : "Literatura / HQ",
                ImagemUrl = capaUrl
            };
        }

        public async Task<List<Livro>> BuscarLivrosDoFilmeAsync(string tituloFilme)
        {
            var livros = await BuscarPorNomeAsync(tituloFilme.Split(':')[0].Split('-')[0].Trim());
            return livros.Take(6).ToList();
        }
    }
}