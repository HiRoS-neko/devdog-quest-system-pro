using Devdog.QuestSystemPro.Editors;
using UnityEditor;

namespace Devdog.QuestSystemPro
{
    [CustomPropertyDrawer(typeof(Achievement))]
    public class AchievementPropertyDrawer : QuestPropertyDrawerBase<Achievement>
    {
    }
}