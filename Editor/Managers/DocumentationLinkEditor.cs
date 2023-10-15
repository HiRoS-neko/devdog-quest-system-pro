using UnityEditor;
using UnityEngine;

namespace Devdog.QuestSystemPro.Editors
{
    public class DocumentationLinkEditor : EditorWindow
    {
        [MenuItem(QuestSystemPro.ToolsMenuPath + "Documentation", false, 99)] // Always at bottom
        public static void ShowWindow()
        {
            Application.OpenURL(QuestSystemPro.ProductUrl);
        }
    }
}