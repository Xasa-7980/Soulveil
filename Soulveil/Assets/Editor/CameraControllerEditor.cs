#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraController))]
public sealed class CameraControllerEditor : Editor
{
    public override void OnInspectorGUI ( )
    {
        DrawDefaultInspector();

        CameraController controller = (CameraController)target;

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("CAMERA TESTS", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Entra en Play Mode para probar los efectos.",
                MessageType.Info
            );
        }

        GUI.enabled = Application.isPlaying;

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Base States", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Enter Combat", GUILayout.Height(26)))
            controller.EnterCombatCamera();
        if (GUILayout.Button("Exit Combat", GUILayout.Height(26)))
            controller.ExitCombatCamera();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Enter Umbral", GUILayout.Height(26)))
            controller.EnterUmbralCamera();
        if (GUILayout.Button("Exit Umbral", GUILayout.Height(26)))
            controller.ExitUmbralCamera();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Enter Aim", GUILayout.Height(26)))
            controller.EnterAimCamera();
        if (GUILayout.Button("Exit Aim", GUILayout.Height(26)))
            controller.ExitAimCamera();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Combat Effects", EditorStyles.boldLabel);

        if (GUILayout.Button("HEAVY ATTACK", GUILayout.Height(32)))
            controller.PlayHeavyAttackCamera();

        if (GUILayout.Button("DODGE", GUILayout.Height(30)))
            controller.PlayDodgeCamera();

        if (GUILayout.Button("PERFECT DODGE", GUILayout.Height(34)))
            controller.PlayPerfectDodgeCamera();

        if (GUILayout.Button("SHAKE", GUILayout.Height(28)))
            controller.PlayShake();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Lock-On", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("ENABLE LOCK-ON", GUILayout.Height(32)))
            controller.EnableLockOn();
        if (GUILayout.Button("DISABLE LOCK-ON", GUILayout.Height(32)))
            controller.DisableLockOn();
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("TARGET SWITCH", GUILayout.Height(34)))
            controller.TestTargetSwitch();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Cinematic", EditorStyles.boldLabel);

        if (GUILayout.Button("ANIME MOVE → TARGET", GUILayout.Height(36)))
            controller.PlayAnimeMove();

        if (GUILayout.Button("LOOK AT TARGET", GUILayout.Height(34)))
            controller.PlayLookAt();

        if (GUILayout.Button("FINISHER CAMERA", GUILayout.Height(38)))
            controller.PlayFinisherCamera();

        if (GUILayout.Button("BOSS INTRODUCTION", GUILayout.Height(38)))
            controller.PlayBossIntroduction();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("RETURN POSE", GUILayout.Height(28)))
            controller.ReturnAnimeMoveToRest();
        if (GUILayout.Button("STOP LOOK AT", GUILayout.Height(28)))
            controller.StopLookAt();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Persistent Gameplay Effects", EditorStyles.boldLabel);

        if (GUILayout.Button("APPLY TEST HEALTH", GUILayout.Height(30)))
            controller.ApplyTestHealth();

        EditorGUILayout.HelpBox(
            "Heavy Enemy Proximity y Wall/Corridor Compression se actualizan automáticamente en Play Mode. " +
            "Asigna Heavy Enemy, Proximity Origin, Wall Check Origin y Rendered Camera en el inspector.",
            MessageType.None
        );

        EditorGUILayout.Space(12);

        if (GUILayout.Button("RESET CAMERA", GUILayout.Height(42)))
            controller.ResetCamera();

        GUI.enabled = true;
    }
}

#endif
