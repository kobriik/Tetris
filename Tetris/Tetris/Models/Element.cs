using System.Collections.ObjectModel;
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

        /// <summary>
        /// Vytvoří pole na hru
        /// </summary>
        public static ObservableCollection<ObservableCollection<Element>> GenerateMatrix(int count)
        {
            var rowElements = new ObservableCollection<ObservableCollection<Tetris.Models.Element>>();
            for (int i = 0; i < count; i++)
            {
                var elements = new ObservableCollection<Tetris.Models.Element>();
                for (int j = 0; j < count; j++)
                {
                    elements.Add(new Tetris.Models.Element());
                }
                rowElements.Add(elements);
            }

            return rowElements;
        }
    }
}
