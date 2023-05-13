using System.Collections.ObjectModel;
using Tetris.ViewModels;

namespace Tetris.Models
{
    public class RowItem : NotifyPropertyChanged
    {
        private int sizeY;
        public int SizeY
        {
            get { return sizeY; }
            set { SetProperty(ref sizeY, value); }
        }

        ObservableCollection<Item> items;
        public ObservableCollection<Item> ColumnItems
        {
            get { return items; }
            set { SetProperty(ref items, value); }
        }

        public RowItem(int sizeY, ObservableCollection<Item> items)
        {
            SizeY = sizeY;
            ColumnItems = items;
        }
    }
}
