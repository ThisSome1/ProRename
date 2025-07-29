#if UNITY_EDITOR
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace ThisSome1.ProRename
{
    internal class RenamePopup : PopupWindowContent
    {
        #region Fields
        internal static readonly Vector2 WindowSize = new(200, 50);
        private string _newName;
        private GameObject[] _targetGOs;
        private string[] _targetGUIDs;
        #endregion

        #region Properties
        private int RenamingObjectsCount => _targetGOs.Length > 0 ? _targetGOs.Length : _targetGUIDs.Length;
        #endregion

        #region Methods
        public RenamePopup(GameObject[] targets)
        {
            _targetGOs = targets;
            _newName = targets[0].name;
            _targetGUIDs = new string[0];
        }
        public RenamePopup(string[] targets)
        {
            _targetGUIDs = targets;
            _targetGOs = new GameObject[0];
            string assetPath = AssetDatabase.GUIDToAssetPath(targets[0]);
            _newName = AssetDatabase.IsValidFolder(assetPath) ? Path.GetFileName(assetPath) : Path.GetFileNameWithoutExtension(assetPath);
        }

        public override Vector2 GetWindowSize() => WindowSize;
        public override void OnGUI(Rect rect)
        {
            var defColor = GUI.color;
            GUI.color = Color.blue;
            GUI.Box(rect, GUIContent.none, new() { normal = new() { background = Texture2D.whiteTexture } });
            GUI.color = defColor;

            GUILayout.Space(2);
            GUILayout.Label($"Rename {RenamingObjectsCount} item{(RenamingObjectsCount > 1 ? "s" : "")} to:", new GUIStyle(GUI.skin.GetStyle("Label")) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter });
            GUILayout.Space(3);
            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            GUI.SetNextControlName("RenameField");
            _newName = EditorGUILayout.TextField(_newName);

            if (GUILayout.Button("OK") || Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
            {
                RenameTargets();
                editorWindow.Close();
            }
            else if (Event.current.keyCode == KeyCode.Escape)
                editorWindow.Close();

            EditorGUI.FocusTextInControl("RenameField");
            GUILayout.Space(5);
            GUILayout.EndHorizontal();
        }

        private void RenameTargets()
        {
            if (!string.IsNullOrEmpty(_newName))
            {
                for (int i = 0; i < _targetGOs.Length; i++)
                {
                    Undo.RecordObject(_targetGOs[i], "Rename Object");
                    _targetGOs[i].name = ParseName(i + 1, _targetGOs[i].name);
                }

                string prevName = _newName;
                for (int i = 0; i < _targetGUIDs.Length; i++)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(_targetGUIDs[i]);
                    string thisName = ParseName(i + 1, AssetDatabase.IsValidFolder(assetPath) ? Path.GetFileName(assetPath) : Path.GetFileNameWithoutExtension(assetPath));
                    typeof(Undo).GetMethod("RegisterAssetsMoveUndo", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).Invoke(null, new object[] { new string[] { assetPath } });
                    string res = AssetDatabase.RenameAsset(assetPath, prevName == thisName ? $"{thisName} {i + 1}" : thisName);
                    if (!string.IsNullOrEmpty(res))
                        Debug.LogError(res);
                    prevName = thisName;
                }
            }
        }
        private string ParseName(int num, string oldName)
        {
            string generated = _newName;
            foreach (Match match in Regex.Matches(generated, @"<name(?:\[(\^?\d+)?..(\^?\d+)?\])?>"))
                generated = generated.Replace(match.Value, oldName[(string.IsNullOrEmpty(match.Groups[1].Value) ?
                    0 :
                    (int.TryParse(match.Groups[1].Value, out int s) ? s : ^int.Parse(match.Groups[1].Value[1..])))..(string.IsNullOrEmpty(match.Groups[2].Value) ?
                        ^0 :
                        (int.TryParse(match.Groups[2].Value, out int e) ? e : ^int.Parse(match.Groups[2].Value[1..])))]);
            foreach (Match match in Regex.Matches(generated, @"<\name(?:\[(\^?\d+)?..(\^?\d+)?\])?>"))
                generated = generated.Replace(match.Value, match.Value[0] + match.Value[2..]);
            foreach (Match match in Regex.Matches(generated, @"<num(?:([+-]\d+))?>"))
                generated = generated.Replace(match.Value, (num + (int.TryParse(match.Groups[1].Value, out int offset) ? offset : 0)).ToString());
            foreach (Match match in Regex.Matches(generated, @"<\num(?:([+-]\d+))?>"))
                generated = generated.Replace(match.Value, match.Value[0] + match.Value[2..]);
            return generated;
        }
    }
    #endregion
}
#endif