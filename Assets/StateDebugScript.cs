using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StateDebugScript : MonoBehaviour
{

    public GameBallManager gb;
    public TextMeshPro debug_state;

    private void OnEnable(){
        gb.OnGameBallStateChange.AddListener(OnStageChange);
    }

    private void OnDisable(){
        gb.OnGameBallStateChange.RemoveListener(OnStageChange);
    }

    private void OnStageChange(GameBallManager.GameBallState state){
        debug_state.text = state.ToString();
    }
}
