using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Runtime.CompilerServices;
using System.Text;

namespace Tetris.Utils
{
    public class StaticData
    {
        private static readonly Random rnd = new Random();

        public static Color DefaultItemColor = Color.White;

        public static List<Color> Colors = new List<Color>()
        {
            Color.Red, Color.Green, Color.Blue, Color.Orange, Color.Purple, Color.Yellow, Color.White
        };

        public static Color GenerateColor()
        {
           return Colors[rnd.Next(5)];
        }
    }
}
