using Microsoft.Maui.Controls;
using OMDb_API.Models;
using OMDb_API.Services;

namespace OMDb_API
{
    public partial class LivrosPage : ContentPage
    {
        private readonly OpenLibraryService _openLibraryService = new();

        public LivrosPage()
        {
            InitializeComponent();
        }

        private async void BotaoBuscarLivros_Clicked(object sender, EventArgs e)
        {
            string termo = CampoBuscaLivros.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(termo)) return;

            LoadingLivros.IsVisible = true;
            LoadingLivros.IsRunning = true;
            MensagemStatusLivros.IsVisible = false;
            ListaResultadosLivros.ItemsSource = null;

            try
            {
                var resultados = await _openLibraryService.BuscarPorNomeAsync(termo);
                if (resultados.Any())
                {
                    ListaResultadosLivros.ItemsSource = resultados;
                }
                else
                {
                    MensagemStatusLivros.Text = $"Nenhum livro ou HQ encontrado para \"{termo}\".";
                    MensagemStatusLivros.IsVisible = true;
                }
            }
            catch
            {
                MensagemStatusLivros.Text = "Erro ao conectar com a Open Library API.";
                MensagemStatusLivros.IsVisible = true;
            }
            finally
            {
                LoadingLivros.IsVisible = false;
                LoadingLivros.IsRunning = false;
            }
        }

        private async void ListaResultadosLivros_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Livro livroSelecionado)
            {
                ListaResultadosLivros.SelectedItem = null;
                await Navigation.PushAsync(new LivroDetalhesPage(livroSelecionado));
            }
        }
    }
}