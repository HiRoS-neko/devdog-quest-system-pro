using UnityEngine;

namespace Devdog.QuestSystemPro.Dialogue.Editors
{
    public abstract class DialogueEditorSidebarBase
    {
        protected DialogueEditorSidebarBase(string name)
        {
            this.name = name;
        }

        public string name { get; protected set; }


        public abstract void Draw(Rect rect, DialogueEditorWindow editor);
    }
}