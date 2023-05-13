using Tetris.Utils;
using Tetris.ViewModels;
using Xamarin.Forms;

namespace Tetris.Models
{
    public class Item : NotifyPropertyChanged
    {
        private int sizeX;

        public int SizeX
        {
            get { return sizeX; }
            set { SetProperty(ref sizeX, value); }
        }

        private Color color;

        public Color Color
        {
            get { return color; }
            set { SetProperty(ref color, value); }
        }

        public Item(int sizeX)
        {
            SizeX = sizeX;
            Color = StaticData.DefaultItemColor;
        }
    }
}
