# GeekHub Explorer

Aplicação multiplataforma desenvolvida em .NET MAUI que reúne quatro frentes da cultura geek em um único ambiente: Cinema & Séries, Videogames, Cartas Colecionáveis Pokémon TCG e Livros & HQs.

O projeto aplica pilares de Orientação a Objetos (abstração, herança, polimorfismo e generics), arquitetura em camadas, persistência local de dados, integração cruzada entre APIs externas e build automatizado via GitHub Actions.

## Como Baixar e Executar (Sem Visual Studio)

Os executáveis prontos para uso são gerados automaticamente pela pipeline de CI/CD e publicados na aba **Releases** deste repositório:

1. Acesse a seção **Releases** na página principal do repositório.
2. Baixe o pacote correspondente ao seu sistema:
   - **Windows (x64):** baixe `GeekHub-Explorer-windows-x64.zip`, extraia todo o conteúdo para uma pasta e execute `OMDb API.exe`.
   - **Android:** baixe `GeekHub-Explorer-android.apk` no celular e autorize a instalação de fontes desconhecidas quando solicitado.

## Funcionalidades Principais

- **Catálogo de Filmes e Séries (OMDb API):**
  - Busca em tempo real com debounce e suporte a títulos em português (dicionário de adaptações brasileiras + tradução automática para inglês com busca paralela).
  - Ficha técnica completa com sinopse, diretor, elenco, duração, bilheteria e nota IMDb.
  - Integração Cross-API (Filme para Jogo): exibe automaticamente jogos relacionados à franquia via RAWG API.
  - Watchlist local ("Assistir no Futuro").

- **Central Gamer & Descobertas (RAWG API):**
  - Pesquisa com filtro de relevância (`search_precise` + popularidade) e bloqueio automático de fangames e conteúdo adulto explícito (Anti-NSFW).
  - Recomendação de jogos similares cruzando gênero principal e tags específicas.
  - Integração Cross-API (Jogo para Filme): exibe filmes e séries ligados à franquia selecionada.
  - Lista de Desejos (Wishlist) armazenada localmente.

- **Enciclopédia Pokémon TCG (TCGdex API):**
  - Busca de cartas oficiais sem necessidade de chave de API.
  - Visualização em alta resolução com HP, tipos, coleção, ilustrador, ataques e raridade.
  - Fichário local de cartas favoritas.

- **Acervo de Livros e HQs (Open Library API):**
  - Pesquisa de obras literárias, quadrinhos e mangás com capas, autores, ano de publicação e assuntos.
  - Placeholder visual automático para obras sem capa oficial cadastrada.

- **Meu Perfil Geek (Dashboard & Backup):**
  - Contadores dinâmicos por categoria, cálculo percentual de afinidade (DNA Geek) e carrossel de itens adicionados recentemente.
  - **Roleta GeekHub ("Surpreenda-me"):** sorteia recomendações inéditas fora da coleção salva do usuário diretamente nas APIs.
  - **Exportação e Importação:** salva o backup completo da coleção em arquivo `.json` diretamente na pasta Downloads e permite restaurar (mesclando ou substituindo) em outro dispositivo.

## Arquitetura e Estrutura do Projeto

```text
OMDb_API/
├── .github/workflows/
│   └── build.yml                  # Pipeline CI/CD para geração de .exe (Windows) e .apk (Android)
├── Models/
│   └── MidiaModels.cs             # Hierarquia MidiaBase (Filme, Jogo, CartaPokemon, Livro) e DTOs
├── Services/
│   └── MidiaServices.cs           # Serviços HTTP, tradutor PT-BR, CrossMediaService e repositórios locais
├── AppShell.xaml(.cs)             # Navegação principal (Flyout e TabBar)
├── MainPage.xaml(.cs)             # Busca de Filmes e Séries
├── FilmeDetalhesPage.xaml(.cs)    # Ficha do Filme + Jogos da franquia
├── JogosPage.xaml(.cs)            # Busca de Videogames
├── JogoDetalhesPage.xaml(.cs)     # Ficha do Jogo + Similares + Filmes relacionados
├── PokemonPage.xaml(.cs)          # Busca de Cartas Pokémon TCG
├── PokemonDetalhesPage.xaml(.cs)  # Ficha detalhada da Carta
├── LivrosPage.xaml(.cs)           # Busca de Livros e HQs
├── LivroDetalhesPage.xaml(.cs)    # Ficha detalhada do Livro/HQ
├── FavoritosPage.xaml(.cs)        # Watchlist local de Filmes
├── WishlistPage.xaml(.cs)         # Lista de Desejos local de Jogos
├── PokemonFavoritosPage.xaml(.cs) # Fichário local de Cartas Pokémon
├── PerfilGeekPage.xaml(.cs)       # Dashboard, Roleta de descobertas e Backup JSON
└── SobrePage.xaml(.cs)            # Resumo técnico da arquitetura
