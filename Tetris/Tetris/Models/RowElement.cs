using System.Collections.ObjectModel;
using Tetris.ViewModels;

namespace Tetris.Models
{
    public class RowElement : NotifyPropertyChanged
    {
        ObservableCollection<Element> elements;
        public ObservableCollection<Element> ColumnElements
        {
            get { return elements; }
            set { SetProperty(ref elements, value); }
        }

        public RowElement(ObservableCollection<Element> elements)
        {
            ColumnElements = elements;
        }
    }
}
