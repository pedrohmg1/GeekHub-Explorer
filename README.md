# 🚀 GeekHub Explorer

Aplicação multiplataforma desenvolvida em **.NET MAUI** que reúne o universo geek em um só lugar: **Cinema & Séries**, **Videogames** e **Cartas Colecionáveis Pokémon TCG**.

O projeto foi projetado aplicando rigorosamente os pilares da **Orientação a Objetos (O.O.)**, arquitetura em camadas, persistência local de dados, integração bidirecional entre APIs externas e quatro modos distintos de navegação.

---

## ✨ Funcionalidades Principais

* **🎬 Catálogo de Filmes e Séries (Cinema Noir & Ouro):**
  * Busca em tempo real com *debounce* (evita requisições desnecessárias enquanto o usuário digita).
  * Ficha técnica completa (sinopse, diretor, elenco, duração, bilheteria e nota IMDb).
  * **Integração Cross-API (Filme → Jogo):** Ao abrir um filme ou série, o sistema extrai a franquia e exibe automaticamente os jogos relacionados disponíveis na RAWG API.
  * **Watchlist ("Assistir no Futuro"):** Permite salvar e remover filmes localmente para assistir mais tarde.

* **🎮 Central Gamer & Descobertas (Cyber-Tech HUD):**
  * Pesquisa otimizada com filtro de relevância (`search_precise` + popularidade) que elimina resultados aleatórios e *fangames* amadores.
  * **Filtro de Segurança (Anti-NSFW):** Bloqueia automaticamente jogos com classificação `Adults Only` e tags de conteúdo explícito, mantendo títulos tradicionais (+18 como *GTA* e *The Witcher*).
  * **Recomendação Inteligente de Jogos Similares:** Cruza o gênero principal mais específico e as tags temáticas do jogo selecionado para sugerir títulos semelhantes.
  * **Integração Cross-API (Jogo → Filme):** Exibe filmes e séries ligados à franquia do jogo selecionado.
  * **Lista de Desejos (Wishlist):** Armazenamento local dos jogos salvos pelo usuário.

* **🃏 Enciclopédia Pokémon TCG (Pokédex Collector):**
  * Busca rápida de cartas oficiais via **TCGdex API** (sem necessidade de chave e com alta estabilidade).
  * Visualização detalhada com imagem em alta resolução, HP, tipos, coleção, ilustrador, ataques e raridade.
  * **Fichário de Cartas Favoritas:** Permite guardar e gerenciar suas cartas preferidas localmente.

---

## 🧱 Arquitetura e Orientação a Objetos (O.O.)

A estrutura do código separa responsabilidades entre modelos de domínio, serviços de comunicação/persistência e interface XAML:

```text
OMDb_API/
├── Models/
│   └── MidiaModels.cs          # Entidades de domínio (MidiaBase, Filme, Jogo, CartaPokemon) e DTOs
├── Services/
│   └── MidiaServices.cs        # Serviços das APIs, CrossMediaService e Repositórios Locais
├── AppShell.xaml               # Configuração de navegação Flyout e TabBar
├── MainPage.xaml(.cs)          # Catálogo de Filmes (OMDb)
├── FilmeDetalhesPage.xaml(.cs) # Detalhes do Filme + Jogos da franquia
├── JogosPage.xaml(.cs)         # Catálogo de Jogos (RAWG)
├── JogoDetalhesPage.xaml(.cs)  # Detalhes do Jogo + Jogos Similares + Filmes relacionados
├── PokemonPage.xaml(.cs)       # Catálogo Pokémon TCG (TCGdex)
├── PokemonDetalhesPage.xaml(.cs) # Ficha da Carta Pokémon
├── FavoritosPage.xaml(.cs)     # Lista local: Assistir no Futuro (Filmes)
├── WishlistPage.xaml(.cs)      # Lista local: Lista de Desejos (Jogos)
├── PokemonFavoritosPage.xaml(.cs) # Lista local: Cartas Favoritas (Pokémon)
└── SobrePage.xaml(.cs)         # Documentação interativa da arquitetura
