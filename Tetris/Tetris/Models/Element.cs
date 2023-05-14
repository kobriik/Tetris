using Tetris.Utils;
using Tetris.ViewModels;
using Xamarin.Forms;

namespace Tetris.Models
{
    public class Element : NotifyPropertyChanged
    {
        private Color color;

        public Color Color
        {
            get { return color; }
            set { SetProperty(ref color, value); }
        }

        public Element()
        {
            Color = StaticData.DefaultItemColor;
        }
    }
}
