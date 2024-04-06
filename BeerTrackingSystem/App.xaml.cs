namespace BeerTrackingSystem
{
    public partial class App : Application
    {
        protected override async void OnStart()
        {
            await ApiGets.GetSessionState();
        }
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }
    }
}
