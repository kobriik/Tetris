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
        private readonly Timer timer = new Timer();
        private readonly Random rnd = new Random();
        private readonly int countItemsY = 10;
        private readonly int countItemsX = 10;
        private bool runningElement;
        private int curPosX = 0;
        private int curPosY = 0;
        private bool gameover;
        private bool timerRunning;

        public ICommand MoveLeftCommand { get; set; }
        public ICommand MoveRightCommand { get; set; }
        public ICommand MoveDownCommand { get; set; }

        public Models.Element CurrentItem => RowElements[curPosY].ColumnElements[curPosX];

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

        ObservableCollection<RowElement> rowElements;
        public ObservableCollection<RowElement> RowElements
        {
            get { return rowElements; }
            set { SetProperty(ref rowElements, value); }
        }

        /// <summary>
        /// Jen pro unit Testy
        /// </summary>
        public MainViewModel(ObservableCollection<RowElement> elements, int x = 0, int y = 0)
        {
            RowElements = elements;
            curPosX = x;
            curPosY = y;
        }

        public MainViewModel()
        {
            MoveLeftCommand = new Command(x => MoveLeft());
            MoveRightCommand = new Command(x => MoveRight());
            MoveDownCommand = new Command(x => MoveDown());

            //Příprava velikosti hrací plochy a plochy pro tlačítka a score
            PlayContentHeight = (int)(contentHeight * 0.9);
            PlayContentHeight = PlayContentHeight - 50 - (PlayContentHeight % countItemsY);
            PlayContentWidth = contentWidth - (contentWidth % countItemsX);
            ScoreContentHeight = 50;
            ButtonsContentHeight = (int)(contentHeight) - PlayContentHeight - ScoreContentHeight - 20;

            //init elementů
            RowElements = new ObservableCollection<RowElement>();
            for (int i = 0; i < countItemsY; i++)
            {
                var element = new ObservableCollection<Models.Element>();
                for (int j = 0; j < countItemsX; j++)
                {
                    element.Add(new Models.Element(playContentWidth / countItemsX));
                }
                RowElements.Add(new RowElement(playContentHeight / countItemsY, element));
            }

            timer.Interval = 250;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        /// <summary>
        /// Je element blokován k posunu?
        /// </summary>
        public bool IsElementBlocked(int x, int y)
        {
            return RowElements[y].ColumnElements[x].Color != StaticData.DefaultItemColor;
        }

        /// <summary>
        /// Změna polohy elementu
        /// </summary>
        public void ChangeElementPosition(Direction direction)
        {
            var currentColor = StaticData.Colors.IndexOf(CurrentItem.Color);
            CurrentItem.Color = StaticData.DefaultItemColor;

            switch (direction)
            {
                case Direction.Left:
                    curPosX--;
                    break;
                case Direction.Right:
                    curPosX++;
                    break;
                case Direction.Down:
                    curPosY++;
                    break;
                case Direction.Bottom:
                    {
                        for (int i = countItemsY - 1; i > 0; i--)
                        {
                            if (RowElements[i].ColumnElements[curPosX].Color == StaticData.DefaultItemColor)
                            {
                                curPosY = i;
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
            if (curPosX != 0 && curPosY != (countItemsY - 1) && !IsElementBlocked(curPosX - 1, curPosY) && !IsElementBlocked(curPosX, curPosY + 1))
            {
                ChangeElementPosition(Direction.Left);
            }
        }

        /// <summary>
        /// Posun elementu vpravo
        /// </summary>
        public void MoveRight()
        {
            if (curPosX != countItemsX - 1 && curPosY != (countItemsY - 1) && !IsElementBlocked(curPosX + 1, curPosY) && !IsElementBlocked(curPosX, curPosY + 1))
            {
                ChangeElementPosition(Direction.Right);
            }
        }

        /// <summary>
        /// Posun elementu na dno
        /// </summary>
        public void MoveDown()
        {
            if (curPosY != (countItemsY - 1) && !IsElementBlocked(curPosX, curPosY + 1))
            {
                ChangeElementPosition(Direction.Bottom);
            }
        }

        /// <summary>
        /// Sestaví novou strukturu po změně elementů
        /// </summary>
        public void ReorderElements()
        {
            for (int y = countItemsY - 1; y > 0; y--)
            {
                for (int x = 0; x < countItemsX; x++)
                {
                    if (RowElements[y].ColumnElements[x].Color == StaticData.DefaultItemColor && RowElements[y - 1].ColumnElements[x].Color != StaticData.DefaultItemColor)
                    {
                        var color = StaticData.Colors.IndexOf(RowElements[y - 1].ColumnElements[x].Color);
                        RowElements[y].ColumnElements[x].Color = StaticData.Colors[color];
                        RowElements[y - 1].ColumnElements[x].Color = StaticData.DefaultItemColor;
                    }
                }
            }
        }

        /// <summary>
        /// Vrátí počet stejných elementů vedle sebe
        /// </summary>
        public int SearchSiblings(int x, int y, int count, Direction direction)
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

                if (RowElements[y].ColumnElements[x].Color == RowElements[y + incrementY].ColumnElements[x + incrementX].Color)
                    count = SearchSiblings(x + incrementX, y + incrementY, ++count, direction);
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
            if (RowElements[y].ColumnElements[x].Color == StaticData.DefaultItemColor)
                return false;

            int countLeft = SearchSiblings(x, y, 0, Direction.Left);
            int countRight = SearchSiblings(x, y, 0, Direction.Right);
            int countTop = SearchSiblings(x, y, 0, Direction.Top);
            int countBottom = SearchSiblings(x, y, 0, Direction.Down);

            if (countLeft + countRight > 1)
            {
                for (int i = x - countLeft; i <= x + countRight; i++)
                {
                    RowElements[y].ColumnElements[i].Color = StaticData.DefaultItemColor;
                }
            }

            if (countBottom + countTop > 1)
            {
                for (int j = y - countTop; j <= y + countBottom; j++)
                {
                    RowElements[j].ColumnElements[x].Color = StaticData.DefaultItemColor;
                }
            }

            if (countLeft + countRight > 1 || countBottom + countTop > 1)
            {
                Score += (countLeft + countRight + countBottom + countTop) * 5;
                ReorderElements();
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
                        if (curPosY == (countItemsY - 1) || IsElementBlocked(curPosX, curPosY + 1))
                        {
                            //Zpracování, pokud dojde ke změně, prochází se znovu celá struktura
                            if (ProcessElements(curPosX, curPosY))
                            {
                                for (int y = countItemsY - 1; y > 0; y--)
                                {
                                    for (int x = 0; x < countItemsX; x++)
                                    {
                                        if (ProcessElements(x, y))
                                        {
                                            //opět byla změna a je potřeba začít znovu
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
                        ChangeElementPosition(Direction.Down);
                    }
                    else
                    {
                        //výběr náhodného začátku elementu
                        var x = rnd.Next(countItemsX);

                        //Kontrola jestli se má kam posunout, jinak konec hry
                        if (IsElementBlocked(x, 0))
                        {
                            gameover = true;
                            ShowMessage("Game over, repeat?");
                            return;
                        }

                        RowElements[0].ColumnElements[x].Color = StaticData.GenerateColor();
                        curPosX = x;
                        curPosY = 0;
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
                    foreach (var item in RowElements.SelectMany(y => y.ColumnElements))
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
