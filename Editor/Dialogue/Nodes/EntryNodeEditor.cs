using Devdog.General.Editors;
using UnityEditor;
using UnityEngine;
using EditorStyles = UnityEditor.EditorStyles;

namespace Devdog.QuestSystemPro.Dialogue.Editors
{
    [CustomNodeEditor(typeof(EntryNode))]
    public class EntryNodeEditor : NodeEditorBase
    {
        protected NodeStyleElement nodeStyle => new("flow node hex 3", "flow node hex 3 on", Color.white);

        protected GUIStyle textStyle =>
            new(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter
            };

        public override void Init(NodeBase node, DialogueEditorWindow editor)
        {
            base.Init(node, editor);
            nodeSize = new Vector2(200f, 50f);
        }

        public override void Draw(bool drawContents)
        {
            GUI.contentColor = nodeStyle.contentColor;

            using (new GroupBlock(GetNodeRect(), GUIContent.none, isSelected ? nodeStyle.active : nodeStyle.normal))
            {
                if (drawContents)
                    EditorGUI.LabelField(new Rect(0, 9f, nodeSize.x, nodeSize.y / 2f), "Entry (start)", textStyle);
            }

            if (drawContents)
            {
                DrawEdgeConnectors();
                DrawReceivingEdgeConnectors();
            }

            GUI.contentColor = Color.white;
        }

        public override void DrawSidebar(ref Rect rect)
        {
            // Nothing to see here...
        }

        public override EdgeConnector[] GetReceivingEdgeConnectors()
        {
            return new EdgeConnector[0]; // Entry node can't have incoming edges.
        }
    }
}