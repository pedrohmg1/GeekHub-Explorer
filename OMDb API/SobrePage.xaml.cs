using Microsoft.Maui.Controls;

namespace OMDb_API
{
    public partial class SobrePage : ContentPage
    {
        public SobrePage()
        {
            InitializeComponent();
        }

        private async void AbrirModal_Clicked(object sender, EventArgs e)
        {
            var modalPage = new ContentPage
            {
                BackgroundColor = Color.FromArgb("#1E1E1E"),
                Content = new VerticalStackLayout
                {
                    Padding = 40,
                    Spacing = 20,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Label { Text = "Arquitetura de Navegação", FontSize = 28, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center },
                        new Label { Text = "• Flyout: Menu lateral no AppShell\n• Barra (TabBar): Abas de Filmes, Jogos e Pokémon\n• Pilha (PushAsync): Telas de detalhes de cada item\n• Modal (PushModalAsync): Esta janela sobreposta", FontSize = 16, TextColor = Color.FromArgb("#B3B3B3") },
                        new Button
                        {
                            Text = "Fechar Modal (PopModalAsync)",
                            BackgroundColor = Color.FromArgb("#EF4444"),
                            TextColor = Colors.White,
                            Command = new Command(async () => await Navigation.PopModalAsync())
                        }
                    }
                }
            };

            await Navigation.PushModalAsync(modalPage);
        }
    }
}