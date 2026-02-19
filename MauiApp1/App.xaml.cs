namespace MauiApp1
{
    public partial class App : Application
    {
        private readonly IServiceProvider _services;

        public App(IServiceProvider services)
        {
            InitializeComponent();

            // Сохраняем DI-контейнер приложения (созданный в MauiProgram)
            _services = services;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Получаем MainPage из DI
            // Стартуем с LoginPage
            var loginPage = _services.GetRequiredService<Auth.LoginPage>();

            // Включаем навигацию, чтобы работал PushAsync
            return new Window(new NavigationPage(loginPage));

        }
    }
}
