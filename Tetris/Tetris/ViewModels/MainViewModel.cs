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
        private readonly int countEl = 10;
        private bool runningElement;
        private int curPosC = 0;
        private int curPosR = 0;
        private bool gameover;
        private bool timerRunning;

        public ICommand MoveLeftCommand { get; set; }
        public ICommand MoveRightCommand { get; set; }
        public ICommand MoveDownCommand { get; set; }

        public Models.Element CurrentItem => Elements[curPosR][curPosC];

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
        public MainViewModel(ObservableCollection<ObservableCollection<Models.Element>> rowElements, int c = 0, int r = 0)
        {
            Elements = rowElements;
            curPosC = c;
            curPosR = r;
        }

        public MainViewModel()
        {
            MoveLeftCommand = new Command(x => MoveLeft());
            MoveRightCommand = new Command(x => MoveRight());
            MoveDownCommand = new Command(x => MoveDown());

            //init elementů
            Elements = Tetris.Models.Element.GenerateMatrix(countEl);

            timer.Interval = 250;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        /// <summary>
        /// Je element blokován k posunu?
        /// </summary>
        public bool IsElementBlocked(int c, int r)
        {
            return Elements[r][c].Color != StaticData.DefaultItemColor;
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
                    curPosC--;
                    break;
                case Direction.Right:
                    curPosC++;
                    break;
                case Direction.Down:
                    curPosR++;
                    break;
                case Direction.Bottom:
                    {
                        for (int r = countEl - 1; r > 0; r--)
                        {
                            if (Elements[r][curPosC].Color == StaticData.DefaultItemColor)
                            {
                                curPosR = r;
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
            if (curPosC != 0 && curPosR != (countEl - 1) && !IsElementBlocked(curPosC - 1, curPosR) && !IsElementBlocked(curPosC, curPosR + 1))
            {
                ChangeElementPosition(Direction.Left);
            }
        }

        /// <summary>
        /// Posun elementu vpravo
        /// </summary>
        public void MoveRight()
        {
            if (curPosC != countEl - 1 && curPosR != (countEl - 1) && !IsElementBlocked(curPosC + 1, curPosR) && !IsElementBlocked(curPosC, curPosR + 1))
            {
                ChangeElementPosition(Direction.Right);
            }
        }

        /// <summary>
        /// Posun elementu na dno
        /// </summary>
        public void MoveDown()
        {
            if (curPosR != (countEl - 1) && !IsElementBlocked(curPosC, curPosR + 1))
            {
                ChangeElementPosition(Direction.Bottom);
            }
        }

        /// <summary>
        /// Sestaví novou strukturu po změně elementů
        /// </summary>
        public void ReorderElements()
        {
            for (int r = countEl - 1; r > 0; r--)
            {
                for (int c = 0; c < countEl; c++)
                {
                    if (Elements[r][c].Color == StaticData.DefaultItemColor && Elements[r - 1][c].Color != StaticData.DefaultItemColor)
                    {
                        var color = StaticData.Colors.IndexOf(Elements[r - 1][c].Color);
                        Elements[r][c].Color = StaticData.Colors[color];
                        Elements[r - 1][c].Color = StaticData.DefaultItemColor;
                    }
                }
            }
        }

        /// <summary>
        /// Vrátí počet stejných elementů vedle sebe
        /// </summary>
        public int SearchSiblings(int c, int r, int count, Direction direction)
        {
            try
            {
                int incrementC = 0;
                int incrementR = 0;

                switch (direction)
                {
                    case Direction.Left:
                        incrementC--;
                        break;
                    case Direction.Right:
                        incrementC++;
                        break;
                    case Direction.Down:
                        incrementR++;
                        break;
                    case Direction.Top:
                        incrementR--;
                        break;
                }

                if (Elements[r][c].Color == Elements[r + incrementR][c + incrementC].Color)
                    count = SearchSiblings(c + incrementC, r + incrementR, ++count, direction);
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
        public bool ProcessElements(int c, int r)
        {
            if (Elements[r][c].Color == StaticData.DefaultItemColor)
                return false;

            int countLeft = SearchSiblings(c, r, 0, Direction.Left);
            int countRight = SearchSiblings(c, r, 0, Direction.Right);
            int countTop = SearchSiblings(c, r, 0, Direction.Top);
            int countBottom = SearchSiblings(c, r, 0, Direction.Down);

            if (countLeft + countRight > 1)
            {
                for (int y = c - countLeft; y <= c + countRight; y++)
                {
                    Elements[r][y].Color = StaticData.DefaultItemColor;
                }
            }

            if (countBottom + countTop > 1)
            {
                for (int x = r - countTop; x <= r + countBottom; x++)
                {
                    Elements[x][c].Color = StaticData.DefaultItemColor;
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
                        if (curPosR == (countEl - 1) || IsElementBlocked(curPosC, curPosR + 1))
                        {
                            //Zpracování, pokud dojde ke změně, prochází se znovu celá struktura
                            if (ProcessElements(curPosC, curPosR))
                            {
                                for (int r = countEl - 1; r > 0; r--)
                                {
                                    for (int c = 0; c < countEl; c++)
                                    {
                                        if (ProcessElements(c, r))
                                        {
                                            //opět byla změna a je potřeba začít znovu
                                            r = countEl - 1;
                                            c = 0;
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
                        var c = rnd.Next(countEl);

                        //Kontrola jestli se má kam posunout, jinak konec hry
                        if (IsElementBlocked(c, 0))
                        {
                            gameover = true;
                            ShowMessage("Game over, repeat?");
                            return;
                        }

                        Elements[0][c].Color = StaticData.GenerateColor();
                        curPosC = c;
                        curPosR = 0;
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
