using UnityEngine;
using UnityEditor;

namespace RLD
{
    [InitializeOnLoad]
    public class RLDLegacyUpgradeWindow : EditorWindow
    {
        private const string _newRLDUrl = "https://assetstore.unity.com/packages/tools/modeling/rld-runtime-level-designer-401648";
        private const string _dontShowAgainPrefKey = "RLD.LegacyUpgradeWindow.DontShowAgain.v1";
        private const string _shownThisSessionKey = "RLD.LegacyUpgradeWindow.ShownThisSession.v1";

        private bool _dontShowAgain;

        static RLDLegacyUpgradeWindow()
        {
            EditorApplication.delayCall += ShowOnEditorLoad;
        }

        [MenuItem("Tools/Runtime Level Design/View New RLD Version")]
        public static void ShowUpgradeWindow()
        {
            RLDLegacyUpgradeWindow window = GetWindow<RLDLegacyUpgradeWindow>(true, "RLD Legacy", true);
            window.minSize = new Vector2(460.0f, 210.0f);
            window.maxSize = new Vector2(460.0f, 210.0f);
            window.Show();
        }

        private static void ShowOnEditorLoad()
        {
            if (Application.isBatchMode) return;
            if (EditorPrefs.GetBool(_dontShowAgainPrefKey, false)) return;
            if (SessionState.GetBool(_shownThisSessionKey, false)) return;

            SessionState.SetBool(_shownThisSessionKey, true);
            ShowUpgradeWindow();
        }

        private void OnEnable()
        {
            _dontShowAgain = EditorPrefs.GetBool(_dontShowAgainPrefKey, false);
        }

        private void OnGUI()
        {
            GUILayout.Space(14.0f);

            EditorGUILayout.LabelField("A new RLD is available", EditorStyles.boldLabel);
            GUILayout.Space(6.0f);

            GUIStyle messageStyle = new GUIStyle(EditorStyles.label);
            messageStyle.wordWrap = true;

            GUILayout.Label(
                "This package is the legacy version of Runtime Level Design. A new version, RLD - Runtime Level Designer, is now available on the Unity Asset Store.",
                messageStyle);

            GUILayout.Space(6.0f);

            GUILayout.Label(
                "If you would like to upgrade, use the button below to view the new RLD page.",
                messageStyle);

            GUILayout.Space(12.0f);

            if (GUILayout.Button("View RLD - Runtime Level Designer", GUILayout.Height(30.0f)))
                Application.OpenURL(_newRLDUrl);

            GUILayout.FlexibleSpace();

            bool dontShowAgain = EditorGUILayout.ToggleLeft("Don't show this message again", _dontShowAgain);
            if (dontShowAgain != _dontShowAgain)
            {
                _dontShowAgain = dontShowAgain;
                EditorPrefs.SetBool(_dontShowAgainPrefKey, _dontShowAgain);
            }

            GUILayout.Space(10.0f);
        }
    }
}
