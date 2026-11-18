using System.Text.Json;
using System.Text.Json.Serialization;

namespace OMDb_API.Models
{
    // ==========================================
    // CLASSES DE DOMÍNIO (ORIENTAÇÃO A OBJETOS)
    // ==========================================
    public abstract class MidiaBase
    {
        public string Id { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Ano { get; set; } = "N/A";
        public string ImagemUrl { get; set; } = string.Empty;
        public string Nota { get; set; } = "N/A";
        public string DataAdicionado { get; set; } = string.Empty;
        public abstract string Categoria { get; }

        public virtual string ObterResumoExibicao()
        {
            return $"{Titulo} ({Ano}) - Nota: {Nota}";
        }
    }

    public class Filme : MidiaBase
    {
        public override string Categoria => "Filme / Série";
        public string Duracao { get; set; } = "N/A";
        public string Genero { get; set; } = "N/A";
        public string Diretor { get; set; } = "N/A";
        public string Elenco { get; set; } = "N/A";
        public string Sinopse { get; set; } = "Sem sinopse.";
        public string Bilheteria { get; set; } = "N/A";

        public override string ObterResumoExibicao()
        {
            return $"{Ano} • {Duracao} • {Genero}";
        }
    }

    public class Jogo : MidiaBase
    {
        public override string Categoria => "Videogame";
        public string Desenvolvedora { get; set; } = "N/A";
        public string Plataformas { get; set; } = "N/A";
        public string Generos { get; set; } = "N/A";
        public string GenerosSlugs { get; set; } = string.Empty;
        public string TagsSlugs { get; set; } = string.Empty;
        public string Descricao { get; set; } = "Sem descrição.";
        public string Metacritic { get; set; } = "N/A";

        public override string ObterResumoExibicao()
        {
            return $"{Ano} • {Generos} • Metacritic: {Metacritic}";
        }
    }

    public class CartaPokemon : MidiaBase
    {
        public override string Categoria => "Carta Pokémon TCG";
        public string Hp { get; set; } = "N/A";
        public string Tipos { get; set; } = "Incolor";
        public string Colecao { get; set; } = "N/A";
        public string Raridade { get; set; } = "Padrão";
        public string Artista { get; set; } = "Desconhecido";
        public string FlavorText { get; set; } = "Carta oficial do Pokémon Trading Card Game.";
        public string Ataques { get; set; } = "Nenhum ataque listado";
        public string PrecoMercado { get; set; } = "Indisponível";

        public override string ObterResumoExibicao()
        {
            return $"{Colecao} • {Raridade} • Tipo: {Tipos}";
        }
    }

    // ==========================================
    // DTOs — OMDb API (FILMES)
    // ==========================================
    public class OmdbSearchResponseDto
    {
        [JsonPropertyName("Search")] public List<OmdbItemDto>? Search { get; set; }
        [JsonPropertyName("Response")] public string? Response { get; set; }
    }

    public class OmdbItemDto
    {
        [JsonPropertyName("Title")] public string? Title { get; set; }
        [JsonPropertyName("Year")] public string? Year { get; set; }
        [JsonPropertyName("imdbID")] public string? ImdbID { get; set; }
        [JsonPropertyName("Poster")] public string? Poster { get; set; }
    }

    public class OmdbDetailDto
    {
        [JsonPropertyName("imdbID")] public string? ImdbID { get; set; }
        [JsonPropertyName("Title")] public string? Title { get; set; }
        [JsonPropertyName("Year")] public string? Year { get; set; }
        [JsonPropertyName("Runtime")] public string? Runtime { get; set; }
        [JsonPropertyName("Genre")] public string? Genre { get; set; }
        [JsonPropertyName("Director")] public string? Director { get; set; }
        [JsonPropertyName("Actors")] public string? Actors { get; set; }
        [JsonPropertyName("Plot")] public string? Plot { get; set; }
        [JsonPropertyName("Poster")] public string? Poster { get; set; }
        [JsonPropertyName("imdbRating")] public string? ImdbRating { get; set; }
        [JsonPropertyName("BoxOffice")] public string? BoxOffice { get; set; }
        [JsonPropertyName("Response")] public string? Response { get; set; }
    }

    // ==========================================
    // DTOs — RAWG.io API (JOGOS)
    // ==========================================
    public class RawgSearchResponseDto
    {
        [JsonPropertyName("results")] public List<RawgItemDto>? Results { get; set; }
    }

    public class RawgItemDto
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("released")] public string? Released { get; set; }
        [JsonPropertyName("background_image")] public string? BackgroundImage { get; set; }
        [JsonPropertyName("rating")] public double Rating { get; set; }
        [JsonPropertyName("added")] public int Added { get; set; }
        [JsonPropertyName("esrb_rating")] public RawgNamedDto? EsrbRating { get; set; }
        [JsonPropertyName("tags")] public List<RawgNamedDto>? Tags { get; set; }
    }

    public class RawgDetailDto
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("released")] public string? Released { get; set; }
        [JsonPropertyName("background_image")] public string? BackgroundImage { get; set; }
        [JsonPropertyName("description_raw")] public string? DescriptionRaw { get; set; }
        [JsonPropertyName("rating")] public double Rating { get; set; }
        [JsonPropertyName("metacritic")] public int? Metacritic { get; set; }
        [JsonPropertyName("genres")] public List<RawgNamedDto>? Genres { get; set; }
        [JsonPropertyName("tags")] public List<RawgNamedDto>? Tags { get; set; }
        [JsonPropertyName("developers")] public List<RawgNamedDto>? Developers { get; set; }
        [JsonPropertyName("platforms")] public List<RawgPlatformWrapperDto>? Platforms { get; set; }
    }

    public class RawgNamedDto
    {
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("slug")] public string? Slug { get; set; }
    }

    public class RawgPlatformWrapperDto
    {
        [JsonPropertyName("platform")] public RawgNamedDto? Platform { get; set; }
    }

    // ==========================================
    // DTOs — TCGDEX API (POKÉMON)
    // ==========================================
    public class TcgdexCardResumoDto
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("image")] public string? Image { get; set; }
    }

    public class TcgdexCardDetalheDto
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("image")] public string? Image { get; set; }
        [JsonPropertyName("hp")] public int? Hp { get; set; }
        [JsonPropertyName("types")] public List<string>? Types { get; set; }
        [JsonPropertyName("rarity")] public string? Rarity { get; set; }
        [JsonPropertyName("illustrator")] public string? Illustrator { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
        [JsonPropertyName("set")] public TcgdexSetDto? Set { get; set; }
        [JsonPropertyName("attacks")] public List<TcgdexAttackDto>? Attacks { get; set; }
    }

    public class TcgdexSetDto
    {
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("releaseDate")] public string? ReleaseDate { get; set; }
    }

    public class TcgdexAttackDto
    {
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("damage")] public JsonElement? Damage { get; set; }
    }
}