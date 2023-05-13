using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using System.Windows.Input;
using Tetris.Models;
using Tetris.Utils;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Tetris.ViewModels
{
    public enum Direction
    {
        Left = 0,
        Right,
        Down,
        Top,
        Bottom,
    }

    public class MainViewModel : NotifyPropertyChanged
    {
        private int contentWidth => (int)(App.Current.MainPage.Width);
        private int contentHeight => (int)(App.Current.MainPage.Height - 100);  //Oříznuto o navigation bar
        private Timer timer = new Timer();
        private int countItemsY = 10;
        private int countItemsX = 10;
        private static Random rnd = new Random();
        private bool runningElement;
        private int CurElPosX = 0;
        private int CurElPosY = 0;
        private bool gameover;
        private bool timerRunning;

        public ICommand MoveLeftCommand { get; set; }
        public ICommand MoveRightCommand { get; set; }
        public ICommand MoveDownCommand { get; set; }

        public Item CurrentItem => RowItems[CurElPosY].ColumnItems[CurElPosX];

        private int score;
        public int Score
        {
            get { return score; }
            set { SetProperty(ref score, value); }
        }

        private int scoreContentHeight;
        public int ScoreContentHeight
        {
            get { return scoreContentHeight; }
            set { SetProperty(ref scoreContentHeight, value); }
        }

        private int playContentHeight;
        public int PlayContentHeight
        {
            get { return playContentHeight; }
            set { SetProperty(ref playContentHeight, value); }
        }

        private int playContentWidth;
        public int PlayContentWidth
        {
            get { return playContentWidth; }
            set { SetProperty(ref playContentWidth, value); }
        }

        private int buttonsContentHeight;
        public int ButtonsContentHeight
        {
            get { return buttonsContentHeight; }
            set { SetProperty(ref buttonsContentHeight, value); }
        }

        ObservableCollection<RowItem> rowItems;
        public ObservableCollection<RowItem> RowItems
        {
            get { return rowItems; }
            set { SetProperty(ref rowItems, value); }
        }

        public MainViewModel()
        {
            MoveLeftCommand = new Command(x => MoveLeft());
            MoveRightCommand = new Command(x => MoveRight());
            MoveDownCommand = new Command(x => MoveDown());

            //Příprava velikosti hrací plochy a plochy pro tlačítka
            PlayContentHeight = (int)(contentHeight * 0.9);
            PlayContentHeight = PlayContentHeight - 50 - (PlayContentHeight % countItemsY);
            PlayContentWidth = contentWidth - (contentWidth % countItemsX);
            ScoreContentHeight = 50;
            ButtonsContentHeight = (int)(contentHeight) - PlayContentHeight - ScoreContentHeight - 20;

            //init elementů
            RowItems = new ObservableCollection<RowItem>();
            for (int i = 0; i < countItemsY; i++)
            {
                var element = new ObservableCollection<Item>();
                for (int j = 0; j < countItemsX; j++)
                {
                    element.Add(new Item(playContentWidth / 10));
                }
                RowItems.Add(new RowItem(playContentHeight / 10, element));
            }

            timer.Interval = 250;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

        }

        /// <summary>
        /// Je element blokován k posunu?
        /// </summary>
        private bool IsBlocked(int x, int y)
        {
            return RowItems[y].ColumnItems[x].Color != StaticData.DefaultItemColor;
        }

        /// <summary>
        /// Změna polohy elementu
        /// </summary>
        private void ChangeItemPosition(Direction direction)
        {
            var currentColor = StaticData.Colors.IndexOf(CurrentItem.Color);
            CurrentItem.Color = StaticData.DefaultItemColor;

            switch (direction)
            {
                case Direction.Left:
                    CurElPosX--;
                    break;
                case Direction.Right:
                    CurElPosX++;
                    break;
                case Direction.Down:
                    CurElPosY++;
                    break;
                case Direction.Bottom:
                    {
                        for (int i = countItemsY - 1; i > 0; i--)
                        {
                            if (RowItems[i].ColumnItems[CurElPosX].Color == StaticData.DefaultItemColor)
                            {
                                CurElPosY = i;
                                break;
                            }
                        }
                        break;
                    }
            }
            CurrentItem.Color = StaticData.Colors[currentColor];
        }

        /// <summary>
        /// Posun elementu vlevo
        /// </summary>
        public void MoveLeft()
        {
            if (CurElPosX != 0 && CurElPosY != (countItemsY - 1) && !IsBlocked(CurElPosX - 1, CurElPosY) && !IsBlocked(CurElPosX, CurElPosY + 1))
            {
                timer.Stop();
                ChangeItemPosition(Direction.Left);
                timer.Start();
            }
        }

        /// <summary>
        /// Posun elementu vpravo
        /// </summary>
        public void MoveRight()
        {
            if (CurElPosX != countItemsX - 1 && CurElPosY != (countItemsY - 1) && !IsBlocked(CurElPosX + 1, CurElPosY) && !IsBlocked(CurElPosX, CurElPosY + 1))
            {
                timer.Stop();
                ChangeItemPosition(Direction.Right);
                timer.Start();
            }
        }

        /// <summary>
        /// Posun elementu na dno
        /// </summary>
        public void MoveDown()
        {
            if (CurElPosY != (countItemsY - 1) && !IsBlocked(CurElPosX, CurElPosY + 1))
            {
                timer.Stop();
                ChangeItemPosition(Direction.Bottom);
                timer.Start();
            }
        }

        /// <summary>
        /// Sestaví novou strukturu po změně elementů
        /// </summary>
        public void ReorderItems()
        {
            for (int y = countItemsY - 1; y > 0; y--)
            {
                for (int x = 0; x < countItemsX; x++)
                {
                    if (RowItems[y].ColumnItems[x].Color == StaticData.DefaultItemColor && RowItems[y - 1].ColumnItems[x].Color != StaticData.DefaultItemColor)
                    {
                        var color = StaticData.Colors.IndexOf(RowItems[y - 1].ColumnItems[x].Color);
                        RowItems[y].ColumnItems[x].Color = StaticData.Colors[color];
                        RowItems[y - 1].ColumnItems[x].Color = StaticData.DefaultItemColor;
                    }
                }
            }
        }

        /// <summary>
        /// Vrátí počet stejných elementů vedle
        /// </summary>
        public int Search(int x, int y, int count, Direction direction)
        {
            try
            {
                int incrementX = 0;
                int incrementY = 0;

                switch (direction)
                {
                    case Direction.Left:
                        incrementX--;
                        break;
                    case Direction.Right:
                        incrementX++;
                        break;
                    case Direction.Down:
                        incrementY++;
                        break;
                    case Direction.Top:
                        incrementY--;
                        break;
                }

                if (RowItems[y].ColumnItems[x].Color == RowItems[y + incrementY].ColumnItems[x + incrementX].Color)
                    count = Search(x + incrementX, y + incrementY, ++count, direction);
            }
            catch (ArgumentOutOfRangeException)
            {
                return count;
            }

            return count;
        }

        /// <summary>
        /// Zpracování elementů, pokud dojde ke změně struktury vrátí true
        /// </summary>
        public bool ProcessElements(int x, int y)
        {
            if (RowItems[y].ColumnItems[x].Color == StaticData.DefaultItemColor)
                return false;

            int countLeft = Search(x, y, 0, Direction.Left);
            int countRight = Search(x, y, 0, Direction.Right);
            int countTop = Search(x, y, 0, Direction.Top);
            int countBottom = Search(x, y, 0, Direction.Down);

            if (countLeft + countRight > 1)
            {
                for (int i = x - countLeft; i <= x + countRight; i++)
                {
                    RowItems[y].ColumnItems[i].Color = StaticData.DefaultItemColor;
                }
            }

            if (countBottom + countTop > 1)
            {
                for (int j = y - countTop; j <= y + countBottom; j++)
                {
                    RowItems[j].ColumnItems[x].Color = StaticData.DefaultItemColor;
                }
            }

            if (countLeft + countRight > 1 || countBottom + countTop > 1)
            {
                Score += (countLeft + countRight + countBottom + countTop) * 5;
                ReorderItems();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Nekonečná smyčka při hře
        /// </summary>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (timerRunning) return;

            try
            {
                timerRunning = true;

                //Hra stále běží
                if (!gameover)
                {
                    //Element se posouvá
                    if (runningElement)
                    {
                        //Element je na dně nebo nad jiným
                        if (CurElPosY == (countItemsY - 1) || IsBlocked(CurElPosX, CurElPosY + 1))
                        {
                            //Zpracování, pokud dojde ke změně prochází se rekurzivně celá struktura
                            if (ProcessElements(CurElPosX, CurElPosY))
                            {
                                for (int y = countItemsY - 1; y > 0; y--)
                                {
                                    for (int x = 0; x < countItemsX; x++)
                                    {
                                        if (ProcessElements(x, y))
                                        {
                                            y = countItemsY - 1;
                                            x = 0;
                                        }
                                    }
                                }
                            }

                            runningElement = false;
                            return;
                        }

                        //Přesun běžícího elementu
                        ChangeItemPosition(Direction.Down);
                    }
                    else
                    {
                        //výběr náhodného začátku elementu
                        var x = rnd.Next(countItemsX);

                        //Kontrola jestli se má kam posunout, jinak konec hry
                        if (IsBlocked(x, 0))
                        {
                            gameover = true;
                            ShowMessage("Game over, repeat?");
                            return;
                        }

                        RowItems[0].ColumnItems[x].Color = StaticData.GenerateColor();
                        CurElPosX = x;
                        CurElPosY = 0;
                        runningElement = true;
                    }
                }
            }
            catch (Exception ex)
            {
                gameover = true;
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                ShowMessage("Uknown error, repeat?");
            }
            finally
            {
                timerRunning = false;
            }
        }

        /// <summary>
        /// Zobrazí dialog se zprávou a nabídne novou hru
        /// </summary>
        /// <param name="message"></param>
        public void ShowMessage(string message)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var result = await App.Current.MainPage.DisplayAlert("Alert", $"Score: {score}{Environment.NewLine}{message}", "Yes", "No");
                if (result)
                {
                    //Nová hra
                    foreach (var item in RowItems.SelectMany(y => y.ColumnItems))
                    {
                        item.Color = StaticData.DefaultItemColor;
                    }
                    gameover = false;
                    Score = 0;
                }
                else
                {
                    //končíme
                    timer.Stop();
                }
            });
        }
    }
}
