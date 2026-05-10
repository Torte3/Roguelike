using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Logs;
using UnityEngine;
using Utilities;

namespace Domain.Service.Rooms
{
    public class MonsterHouse : Room<RoomMemento>
    {
        public MonsterHouse(RoomMemento data, Vector2Int playerPosition) : base(data, playerPosition)
        {
        }

        public static RoomMemento Build(RectInt rect)
        {
            return new RoomMemento
            (
                rect,
                false,
                false
            );
        }

        public override RoomMemento Serialize()
        {
            return new RoomMemento
            (
                Rect,
                hasEntered,
                hasEverEntered
            );
        }

        protected override async UniTask FirstTimeEnter(IGameManager gameManager, IMap map)
        {
            gameManager.PlayBGM(BGM.MonsterHouse);
            GameLog.AddIgnoreVisibility("<color=red>モンスターハウスだ！</color>");
            var area = Rect.size.x * Rect.size.y;
            var monsterCount = area switch
            {
                < 50 => 10,
                < 100 => 15,
                < 200 => 20,
                _ => 30,
            };
            var allPositions = map
                .GetAllBlankAndStandablePositionsOn(EntityLayer.Middle)
                .In(Rect.RectRange());
            var positions = allPositions
                .GetAtRandom(Mathf.Min(allPositions.Count(), monsterCount));
            foreach (var position in positions)
            {
                map.SpawnRandomEnemy(position.Position, false);
            }

            await UniTask.Delay(1000);
        }
    }
}