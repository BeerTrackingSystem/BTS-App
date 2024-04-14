namespace BeerTrackingSystem
{
    public partial class App : Application
    {
        protected override async void OnStart()
        {
            await ApiGets.GetSessionState();
            Misc.GetDarkMode();
        }
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }
    }
}
