using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DebugManager : MonoBehaviour
{
    public GameBallManager debugBall;

    public bool SlowMode { get; private set; }

    public Vector3 ThrowPosition;
    public Vector3 ThrowDirection;
    private float TimeStep = 0.03f;

    public float slowdownFactor = 0.05f;

    void OnGUI()
    {


        // if (GUILayout.Button("I am a regular Automatic Layout Button"))
        // {
        //     Debug.Log("Clicked Button");
        // }

    }

    public void OnEnable(){
        this.SlowMode = false;
        this.TimeStep = Time.fixedDeltaTime;  
    }

    public void ToggleSlowMotion(){
        if(SlowMode){
            Time.timeScale = 1f;
            Time.fixedDeltaTime = this.TimeStep ;

            SlowMode = false;
        } else{
            Time.timeScale = this.slowdownFactor;
            Time.fixedDeltaTime = Time.timeScale * this.TimeStep;

            SlowMode = true;
        }
    }


    public void ThrowDebugBall(){
        debugBall.ThrowAsDebug(ThrowPosition, ThrowDirection);
        // debugBall.GetComponent<GameBallManager>()
        // debugBall.SetActive(false);

        // debugBall.SetActive(true);
    }

    private void OnDrawGizmos(){
        Gizmos.color = Color.white;
        Gizmos.DrawLine(ThrowPosition, ThrowDirection);
    }
}
