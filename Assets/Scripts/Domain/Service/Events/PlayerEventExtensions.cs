#nullable enable
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Service.Action;

namespace Domain.Service.Events
{
    public static class PlayerEventExtensions
    {
        public static bool CanExecuteEvent(this IReadOnlyList<IPlayerEvent> playerEvents, IPlayer player, IMap map)
        {
            return playerEvents.Where(e => e.CanExecuteEvent(player, map)).Any();
        }
        public static async UniTask<bool> DoEvent(this IReadOnlyList<IPlayerEvent> playerEvents, IPlayer player, IGameManager gameManager, IMap map)
        {
            playerEvents = playerEvents.Where(e => e.CanExecuteEvent(player, map)).ToList();
            if (!playerEvents.Any())
                return false;
            var index = 0;
            while (true)
            {
                var choices = new List<string>();
                var firstChoiceIndex = 0;

                if (index > 0)
                {
                    choices.Add("前のぺージ");
                    firstChoiceIndex += 1;
                }

                var executableEvents = playerEvents[index].Events.Where(e => e.CanExecuteEvent(player, map)).ToList();
                foreach (var eventData in executableEvents)
                {
                    choices.Add(eventData.ChoiceText);
                }

                if (index < playerEvents.Count - 1)
                {
                    choices.Add("次のぺージ");
                }

                var cancelChoiceIndex = choices.Count;
                choices.Add("やめる");

                var choiceIndex = 0;
                if (choices.Count > 1)
                {
                    choiceIndex = await gameManager.GetChoice(
                        playerEvents[index].ChoiceMessage, cancelChoiceIndex, choices.ToArray());
                }

                if (choiceIndex == cancelChoiceIndex)
                    return false;

                switch (choices[choiceIndex])
                {
                    case "前のぺージ":
                        index -= 1;
                        continue;
                    case "次のぺージ":
                        index += 1;
                        continue;
                    default:
                        await executableEvents[choiceIndex - firstChoiceIndex].DoEvent(player, gameManager, map);
                        return true;
                }
            }
        }
        public static async UniTask<IAction?> DoAction(this IReadOnlyList<IPlayerEvent> playerEvents, IPlayer player, IGameManager gameManager, IMap map, IAction swap)
        {
            playerEvents = playerEvents.Where(e => e.CanExecuteEvent(player, map)).ToList();
            if (!playerEvents.Any())
                return null;
            var index = 0;
            while (true)
            {
                var choices = new List<string>();
                var firstChoiceIndex = 0;

                if (index > 0)
                {
                    choices.Add("前のぺージ");
                    firstChoiceIndex += 1;
                }

                if (swap.Doable(player.Character, map) && index == 0)
                {
                    choices.Add("入れ替わる");
                    firstChoiceIndex += 1;
                }

                var executableEvents = playerEvents[index].Events.Where(e => e.CanExecuteEvent(player, map)).ToList();
                foreach (var eventData in executableEvents)
                {
                    choices.Add(eventData.ChoiceText);
                }

                if (index < playerEvents.Count - 1)
                {
                    choices.Add("次のぺージ");
                }

                var cancelChoiceIndex = choices.Count;
                choices.Add("やめる");

                var choiceIndex = 0;
                if (choices.Count > 1)
                {
                    choiceIndex = await gameManager.GetChoice(
                        playerEvents[index].ChoiceMessage, cancelChoiceIndex, choices.ToArray());
                }

                if (choiceIndex == cancelChoiceIndex)
                    return null;

                switch (choices[choiceIndex])
                {
                    case "前のぺージ":
                        index -= 1;
                        continue;
                    case "次のぺージ":
                        index += 1;
                        continue;
                    case "入れ替わる":
                        return swap;
                    default:
                        await executableEvents[choiceIndex - firstChoiceIndex]
                            .DoEvent(player, gameManager, map);
                        return new DoNothing();
                }
            }
        }
    }
}
