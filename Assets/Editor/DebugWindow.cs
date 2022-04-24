using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DebugWindow : EditorWindow
{


    [MenuItem ("Window/DebugWindow")]

    public static void  ShowWindow () {
        EditorWindow.GetWindow(typeof(DebugWindow));
    }

    void OnGUI () {

        if (Application.isPlaying) {

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Debug Variables");

            GameManagerScript gameManagerScript = FindObjectOfType<GameManagerScript>();
            DebugManager debugManager = FindObjectOfType<DebugManager>();

            EditorGUILayout.LabelField("Game State:", gameManagerScript.GameState.ToString());
            EditorGUILayout.LabelField("Assist Mode:", gameManagerScript.IsCheatMode.ToString() );

            if(GUILayout.Button("AssistMode"))
            {
                gameManagerScript.ToggleCheatMode();
            }

            if(GUILayout.Button("SlowMode"))
            {
                debugManager.ToggleSlowMotion();
            }

            if(GUILayout.Button("Throw Ball"))
            {
                debugManager.ThrowDebugBall();
            }

        } else{
            EditorGUILayout.LabelField("Start the game to look at controls");

        }
    }
}
