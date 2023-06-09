﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tetris.ViewModels;
using Xamarin.Forms;

namespace Tests
{
    [TestClass]
    public class GameTests
    {
        private readonly Color testColor = Color.Purple;

        private MainViewModel Init(int x = 0, int y = 0)
        {
            var rowElements = Tetris.Models.Element.GenerateMatrix(10);
            rowElements[2][2].Color = testColor;
            rowElements[3][2].Color = testColor;
            rowElements[3][3].Color = testColor;
            rowElements[3][4].Color = testColor;

            return new MainViewModel(rowElements, x, y);
        }

        /// <summary>
        /// otestuje stejné elementy v okolí
        /// </summary>
        [TestMethod]
        public void TestSiblings()
        {
            var vm = Init();
            var leftCount = vm.SearchSiblings(3, 3, 0, Direction.Left);
            var rightCount = vm.SearchSiblings(3, 3, 0, Direction.Right);
            Assert.AreEqual(2, leftCount + rightCount);
        }

        /// <summary>
        /// otestuje blokování elementů
        /// </summary>
        [TestMethod]
        public void TestBlockedElement()
        {
            var vm = Init();
            Assert.AreEqual(true, vm.IsElementBlocked(2, 3));
        }

        /// <summary>
        /// otestuje reorganizaci struktury
        /// </summary>
        [TestMethod]
        public void TestReorderElements()
        {
            var vm = Init();
            vm.ReorderElements();
            Assert.AreEqual(Color.White, vm.Elements[2][2].Color);
            Assert.AreEqual(testColor, vm.Elements[3][2].Color);
        }

        /// <summary>
        /// otestuje zpracování elementů
        /// </summary>
        [TestMethod]
        public void TestProcessElements()
        {
            var vm = Init();
            vm.ProcessElements(3, 3);
            Assert.AreEqual(Color.White, vm.Elements[2][2].Color);
            Assert.AreEqual(testColor, vm.Elements[3][2].Color);
        }

        /// <summary>
        /// otestuje změnu polohy
        /// </summary>
        [TestMethod]
        public void TestChangePosition()
        {
            var vm = Init(3, 3);
            vm.ChangeElementPosition(Direction.Bottom);

            Assert.AreEqual(testColor, vm.Elements[9][3].Color);
            Assert.AreEqual(Color.White, vm.Elements[3][3].Color);
        }

        /// <summary>
        /// otestuje změnu směru
        /// </summary>
        [TestMethod]
        public void TestChangeDirections()
        {
            var vm = Init(2, 3);

            vm.MoveLeft();
            Assert.AreEqual(Color.White, vm.Elements[3][2].Color);
            Assert.AreEqual(testColor, vm.Elements[3][1].Color);

            vm.MoveRight();
            Assert.AreEqual(testColor, vm.Elements[3][2].Color);
            Assert.AreEqual(Color.White, vm.Elements[3][1].Color);

            vm.MoveDown();
            Assert.AreEqual(Color.White, vm.Elements[3][2].Color);
            Assert.AreEqual(testColor, vm.Elements[9][2].Color);
        }
    }
}
