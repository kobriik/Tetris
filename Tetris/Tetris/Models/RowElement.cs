using System.Collections.ObjectModel;
using Tetris.ViewModels;

namespace Tetris.Models
{
    public class RowElement : NotifyPropertyChanged
    {
        private int sizeY;
        public int SizeY
        {
            get { return sizeY; }
            set { SetProperty(ref sizeY, value); }
        }

        ObservableCollection<Element> elements;
        public ObservableCollection<Element> ColumnElements
        {
            get { return elements; }
            set { SetProperty(ref elements, value); }
        }

        public RowElement(int sizeY, ObservableCollection<Element> elements)
        {
            SizeY = sizeY;
            ColumnElements = elements;
        }
    }
}
