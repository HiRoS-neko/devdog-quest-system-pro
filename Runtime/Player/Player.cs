using System.Collections.Generic;
using Devdog.QuestSystemPro;

namespace Devdog.General
{
    /// <summary>
    /// Partial class for the Devdog.General.Player
    /// </summary>
    public static class PlayerExtensions
    {
        private static readonly Dictionary<Player, QuestSystemPlayer> _questSystemPlayer = new();

        public static QuestSystemPlayer QuestSystemPlayer(this Player player)
        {
            if (!_questSystemPlayer.ContainsKey(player))
                _questSystemPlayer.Add(player, player.GetComponent<QuestSystemPlayer>());

            return _questSystemPlayer[player];
        }
    }
}