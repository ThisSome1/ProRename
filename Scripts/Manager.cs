using UnityEditor;
using UnityEditor.SearchService;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace ThisSome1.ProRename
{
    [InitializeOnLoad]
    public class Manager
    {
        #region Fields
        internal const string ShortcutId = "ThisSome1/Pro Rename";
        #endregion

        #region Properties
        #endregion

        #region Methods
        static Manager()
        {
            var ctrlF2 = new KeyCombination(KeyCode.F2, ShortcutModifiers.Action);
            var shortcutIds = ShortcutManager.instance.GetAvailableShortcutIds();
            foreach (string id in shortcutIds)
            {
                var bindings = ShortcutManager.instance.GetShortcutBinding(id);
                foreach (var combination in bindings.keyCombinationSequence)
                    if (combination.Equals(ctrlF2))
                        return;
            }
            StackTraceLogType stLogType = Application.GetStackTraceLogType(LogType.Log);
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            ShortcutManager.instance.RebindShortcut(ShortcutId, new(ctrlF2));
            Debug.Log("<color=#00FF00>Ctrl+F2</color> shortcut has assigned to <color=cyan>Pro Rename</color>", SceneView.currentDrawingSceneView);
            Application.SetStackTraceLogType(LogType.Log, stLogType);
        }

        [Shortcut(ShortcutId)]
        public static void RenameIfSelected()
        {
            if (CanRename())
                RenameSelected(new(Selection.activeGameObject));
        }
        [MenuItem("GameObject/ThisSome1/Pro Rename", true), MenuItem("Assets/ThisSome1/Pro Rename", true)]
        private static bool CanRename() => EditorWindow.focusedWindow != null &&
                                          (EditorWindow.focusedWindow.titleContent.text == "Hierarchy" || EditorWindow.focusedWindow.titleContent.text == "Project") &&
                                          (Selection.gameObjects.Length > 0 || Selection.assetGUIDs.Length > 0);
        [MenuItem("GameObject/ThisSome1/Pro Rename", false), MenuItem("Assets/ThisSome1/Pro Rename", false)]
        private static void RenameSelected(MenuCommand cmd)
        {
            if (cmd.context && cmd.context != Selection.activeObject)
                return;

            Vector2 mousePos;
            if (EditorWindow.focusedWindow)
            {
                Rect windowRect = EditorWindow.focusedWindow.position;
                mousePos = new Vector2(windowRect.x + windowRect.width / 2 - RenamePopup.WindowSize.x / 2, windowRect.y + windowRect.height / 2 - RenamePopup.WindowSize.y / 2 - 50);
            }
            else
                mousePos = new(Screen.width / 2 - RenamePopup.WindowSize.x / 2, Screen.height / 2 - RenamePopup.WindowSize.y / 2 - 50);

            EditorApplication.delayCall += () =>
                PopupWindow.Show(new(GUIUtility.ScreenToGUIPoint(mousePos), RenamePopup.WindowSize), Selection.gameObjects.Length > 0 ? new RenamePopup(Selection.gameObjects) : new RenamePopup(Selection.assetGUIDs));
        }
        #endregion
    }
}