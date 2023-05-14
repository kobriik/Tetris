using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using System.Windows.Input;
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

        public Models.Element CurrentItem => Elements[curPosY][curPosX];

        private int score;
        public int Score
        {
            get { return score; }
            set { SetProperty(ref score, value); }
        }

        ObservableCollection<ObservableCollection<Models.Element>> elements;
        public ObservableCollection<ObservableCollection<Models.Element>> Elements
        {
            get { return elements; }
            set { SetProperty(ref elements, value); }
        }

        /// <summary>
        /// Jen pro unit Testy
        /// </summary>
        public MainViewModel(ObservableCollection<ObservableCollection<Models.Element>> rowElements, int x = 0, int y = 0)
        {
            Elements = rowElements;
            curPosX = x;
            curPosY = y;
        }

        public MainViewModel()
        {
            MoveLeftCommand = new Command(x => MoveLeft());
            MoveRightCommand = new Command(x => MoveRight());
            MoveDownCommand = new Command(x => MoveDown());

            //init elementů
            Elements = new ObservableCollection<ObservableCollection<Models.Element>>();
            for (int i = 0; i < countItemsY; i++)
            {
                var elements = new ObservableCollection<Models.Element>();
                for (int j = 0; j < countItemsX; j++)
                {
                    elements.Add(new Models.Element());
                }
                Elements.Add(elements);
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
            return Elements[y][x].Color != StaticData.DefaultItemColor;
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
                            if (Elements[i][curPosX].Color == StaticData.DefaultItemColor)
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
                    if (Elements[y][x].Color == StaticData.DefaultItemColor && Elements[y - 1][x].Color != StaticData.DefaultItemColor)
                    {
                        var color = StaticData.Colors.IndexOf(Elements[y - 1][x].Color);
                        Elements[y][x].Color = StaticData.Colors[color];
                        Elements[y - 1][x].Color = StaticData.DefaultItemColor;
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

                if (Elements[y][x].Color == Elements[y + incrementY][x + incrementX].Color)
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
            if (Elements[y][x].Color == StaticData.DefaultItemColor)
                return false;

            int countLeft = SearchSiblings(x, y, 0, Direction.Left);
            int countRight = SearchSiblings(x, y, 0, Direction.Right);
            int countTop = SearchSiblings(x, y, 0, Direction.Top);
            int countBottom = SearchSiblings(x, y, 0, Direction.Down);

            if (countLeft + countRight > 1)
            {
                for (int i = x - countLeft; i <= x + countRight; i++)
                {
                    Elements[y][i].Color = StaticData.DefaultItemColor;
                }
            }

            if (countBottom + countTop > 1)
            {
                for (int j = y - countTop; j <= y + countBottom; j++)
                {
                    Elements[j][x].Color = StaticData.DefaultItemColor;
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

                        Elements[0][x].Color = StaticData.GenerateColor();
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
                    foreach (var item in Elements.SelectMany(y => y))
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
