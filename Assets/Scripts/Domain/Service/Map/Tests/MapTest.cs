using System;
using System.Collections.Generic;
using Domain.Model.Map;
using NUnit.Framework;
using UnityEngine;

namespace Domain.Service.Map.Tests
{
    internal class MapTest
    {
        private static IEnumerable<TestCaseData> IndexTestCases
        {
            get
            {
                yield return new TestCaseData(10, 10, new Vector2Int(0, 0));
                yield return new TestCaseData(10, 10, new Vector2Int(9, 0));
                yield return new TestCaseData(10, 10, new Vector2Int(9, 9));
            }
        }

        private static IEnumerable<TestCaseData> IndexTest2Cases
        {
            get { yield return new TestCaseData(10, 10, new Vector2Int(9, 10)); }
        }

        [TestCaseSource(nameof(IndexTestCases))]
        public void IndexTest1(int width, int height, Vector2Int position)
        {
            var map = new Tilemap(width, height);
            Assert.AreEqual(new TileData(TileData.Build(MapType.Cave, TileCategory.Blank, false)), map.GetTile(position));
        }

        [TestCaseSource(nameof(IndexTest2Cases))]
        public void IndexTest2(int width, int height, Vector2Int position)
        {
            var map = new Tilemap(width, height);
            Assert.Throws<ArgumentOutOfRangeException>(() => map.GetTile(position));
        }
    }
}