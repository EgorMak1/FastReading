namespace MauiApp1
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Получаем MainPage из контейнера зависимостей
            var mainPage = MauiProgram
                .CreateMauiApp()
                .Services
                .GetService<MainPage>();

            return new Window(mainPage);
        }
    }
}
