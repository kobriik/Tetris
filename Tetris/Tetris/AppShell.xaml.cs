using Tetris.Views;
using Xamarin.Forms;

namespace Tetris
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        }

    }
}
