using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Devdog.General;
using Devdog.General.Editors;
using UnityEditor;
using UnityEngine;
using EditorStyles = Devdog.General.Editors.EditorStyles;
using EditorUtility = UnityEditor.EditorUtility;

namespace Devdog.QuestSystemPro.Editors
{
    public class AchievementsEditor : EditorCrudBase<Achievement>
    {
        private static Achievement _previousItem;

        private List<TypeFilter> _allItemTypes;
        private Editor _editor;

        private string _previouslySelectedGUIItemName;


        private Achievement _selectedItem;


        //private Editor previewEditor;


        public AchievementsEditor(string singleName, string pluralName, EditorWindow window)
            : base(singleName, pluralName, window)
        {
            window.autoRepaintOnSceneChange = false;
        }

        protected override Achievement selectedItem
        {
            get => _selectedItem;
            set
            {
                _editor = null;
                if (value != null) _editor = Editor.CreateEditor(value);

                _selectedItem = value;
            }
        }

        protected override List<Achievement> crudList
        {
            get => new(QuestManager.instance.achievements);
            set
            {
                QuestManager.instance.achievements = value.ToArray();
                EditorUtility.SetDirty(QuestManager.instance.questDatabase);
            }
        }

        protected List<TypeFilter> allItemTypes
        {
            get
            {
                if (_allItemTypes == null)
                    _allItemTypes = GetAllItemTypes();

                return _allItemTypes;
            }
            set => _allItemTypes = value;
        }

        protected virtual List<TypeFilter> GetAllItemTypes()
        {
            var types = new List<TypeFilter>(16);
            foreach (var type in ReflectionUtility.GetAllTypesThatImplement(typeof(Achievement)))
                types.Add(new TypeFilter(type, false));
            return types;
        }

        protected override bool MatchesSearch(Achievement achievement, string searchQuery)
        {
            var search = searchQuery.ToLower();
            return achievement.name.message.ToLower().Contains(search) ||
                   achievement.description.message.ToLower().Contains(search) ||
                   achievement.GetType().Name.ToLower().Contains(search);
        }

        protected override void CreateNewItem()
        {
            var picker = ScriptPickerEditor.Get(typeof(Achievement));
            picker.OnPickObject = type =>
            {
                var savePath = EditorPrefs.GetString(SettingsEditor.PrefabSaveKey);
                if (savePath == string.Empty ||
                    Directory.Exists(savePath) == false)
                {
                    Debug.LogWarning("The directory you're trying to save to doesn't exist! (" + savePath + ")");
                    return;
                }

                var asset = (Achievement)ScriptableObject.CreateInstance(type);
                AssetDatabase.CreateAsset(asset, savePath + "/" + GetAssetName(asset) + ".asset");
                AssetDatabase.SetLabels(asset, new[] { "Achievement" });
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                AddItem(asset);
                picker.Close();
            };

            picker.Show();
        }

        public override void DuplicateItem(int index)
        {
            var source = crudList[index];
            var newPath = AssetDatabase.GetAssetPath(source).Replace(".asset", "") + "_duplicate.asset";

            var copied = AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(source), newPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (copied)
            {
                var asset = AssetDatabase.LoadAssetAtPath<Achievement>(newPath);
                EditorUtility.SetDirty(asset); // To save it.
                EditorUtility.SetDirty(QuestManager.instance); // To save it.

                AddItem(asset);
            }

            window.Repaint();
        }

        public override void AddItem(Achievement item, bool editOnceAdded = true)
        {
            base.AddItem(item, editOnceAdded);

            UpdateAssetName(item);
            UpdateAssetID(item);

            EditorUtility.SetDirty(QuestManager.instance);
        }

        private void UpdateAssetID(Achievement item)
        {
            item.ID = crudList.Max(o => o.ID) + 1;
            EditorUtility.SetDirty(item);
        }

        public override void RemoveItem(int i)
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(QuestManager.instance.achievements[i]));
            base.RemoveItem(i);
        }

        public override void EditItem(Achievement achievement, int index)
        {
            base.EditItem(achievement, index);

            Undo.ClearUndo(_previousItem);
            Undo.RecordObject(achievement, QuestSystemPro.ProductName + "_item");

            _previousItem = achievement;
        }

        protected override void DrawSidebar()
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();

            var i = 0;
            foreach (var type in allItemTypes)
            {
                if (i % 3 == 0)
                    GUILayout.BeginHorizontal();

                type.enabled = GUILayout.Toggle(type.enabled, type.type.Name, "OL Toggle");

                if (i % 3 == 2 || i == allItemTypes.Count - 1)
                    GUILayout.EndHorizontal();

                i++;
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            base.DrawSidebar();
        }

        protected override void DrawSidebarRow(Achievement item, int i)
        {
            var checkedCount = 0;
            foreach (var type in allItemTypes)
                if (type.enabled)
                    checkedCount++;

            if (checkedCount > 0)
                if (allItemTypes.FirstOrDefault(o => o.type == item.GetType() && o.enabled) == null)
                    return;

            BeginSidebarRow(item, i);

            DrawSidebarRowElement(item.name.message, 160);
            DrawSidebarRowElement(item.GetType().Name, 120);
            var t = DrawSidebarRowElementToggle(true, "", "AssetLabel Icon", 20);
            if (t == false) // User clicked view icon
                AssetDatabase.OpenAsset(selectedItem);

            EndSidebarRow(item, i);
        }

        protected override void DrawDetail(Achievement item, int index)
        {
            EditorGUIUtility.labelWidth = EditorStyles.labelWidth;

            General.Editors.EditorUtility.ErrorIfEmpty(
                EditorPrefs.GetString(SettingsEditor.PrefabSaveKey) == string.Empty,
                QuestSystemPro.ProductName +
                " prefab folder is not set, items cannot be saved! Please go to settings and define the " +
                QuestSystemPro.ProductName + " prefab folder.");
            if (EditorPrefs.GetString(SettingsEditor.PrefabSaveKey) == string.Empty)
            {
                canCreateItems = false;
                return;
            }

            canCreateItems = true;

//            GUILayout.Label("Use the inspector if you want to add custom components.", EditorStyles.titleStyle);
//            EditorGUILayout.Space();
//            EditorGUILayout.Space();
//            if (GUILayout.Button("Convert type"))
//            {
//                var typePicker = ObjectPickerEditor.Get(typeof(Affector));
//                typePicker.Show(true);
//                typePicker.OnPickObject += type =>
//                {
//                    ConvertThisToNewType(item, type);
//                };
//
//                return;
//            }

            if (_editor != null) _editor.OnInspectorGUI();

            if (_previouslySelectedGUIItemName == "Achievement_Name" &&
                GUI.GetNameOfFocusedControl() != _previouslySelectedGUIItemName) UpdateAssetName(item);

            _previouslySelectedGUIItemName = GUI.GetNameOfFocusedControl();
            EditorGUIUtility.labelWidth = 0;
        }

        public static string GetAssetName(Achievement achievement)
        {
            return DateTime.Now.ToFileTime() + (string.IsNullOrEmpty(achievement.name.message)
                ? "Unnamed_"
                : achievement.name.message.ToLower().Replace(" ", "_")) + "_PFB";
        }

        public static void UpdateAssetName(Achievement item)
        {
            var newName = GetAssetName(item);
            if (AssetDatabase.GetAssetPath(item).EndsWith(newName + ".prefab") == false)
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(item), newName);
        }

//        public void ConvertThisToNewType(Affector affector, Type type)
//        {
//            selectedItem = null;
//
//            var asset = (Affector)ScriptableObject.CreateInstance(type);
//            var newPath = AssetDatabase.GetAssetPath(affector);
//            AssetDatabase.CreateAsset(asset, newPath);
//            AssetDatabase.SaveAssets();
//            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
//
//            var toFields = new List<FieldInfo>(64);
//            EditorUtility.GetAllFieldsInherited(asset.GetType(), toFields);
//
//            var currentFields = new List<FieldInfo>(64);
//            EditorUtility.GetAllFieldsInherited(affector.GetType(), currentFields);
//            foreach (var fieldInfo in currentFields)
//            {
//                var toField = toFields.FirstOrDefault(o => o.Name == fieldInfo.Name);
//                if (toField != null)
//                {
//                    toField.SetValue(asset, fieldInfo.GetValue(affector));
//                }
//            }
//
//            for (int i = 0; i < AffectorProManager.instance.affectors.Length; i++)
//            {
//                if (AffectorProManager.instance.affectors[i] == null)
//                {
//                    AffectorProManager.instance.affectors[i] = asset;
//                    break;
//                }
//            }
//
//            selectedItem = asset;
//            UnityEditor.EditorUtility.SetDirty(asset);
//            UnityEditor.EditorUtility.SetDirty(AffectorProManager.instance);
//
//            GUI.changed = true;
//            window.Repaint();
//        }

        protected override bool IDsOutOfSync()
        {
            return crudList.Contains(null);
        }

        protected override void SyncIDs()
        {
            var l = new List<Achievement>(crudList);
            l.RemoveAll(o => o == null);
            crudList = l;

            EditorUtility.SetDirty(QuestManager.instance);
        }

        protected class TypeFilter
        {
            public TypeFilter(Type type, bool enabled)
            {
                this.type = type;
                this.enabled = enabled;
            }

            public Type type { get; set; }
            public bool enabled { get; set; }
        }
    }
}