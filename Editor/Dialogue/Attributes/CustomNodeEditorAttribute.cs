using System;

namespace Devdog.QuestSystemPro.Dialogue.Editors
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CustomNodeEditorAttribute : Attribute
    {
        public CustomNodeEditorAttribute(Type type)
        {
            this.type = type;
        }

        public Type type { get; set; }
    }
}