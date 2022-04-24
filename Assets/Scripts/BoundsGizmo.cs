using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundsGizmo : MonoBehaviour
{

    public float gizmoRadio = 1;

    public Color wireColor = Color.red;

    public Color fillColor = new Color( 1, .6f, .3f, .3f);

    private void OnDrawGizmos(){
        Gizmos.color = wireColor;
        Gizmos.DrawWireCube(gameObject.transform.position , Vector3.one);

        Gizmos.color = fillColor;
        Gizmos.DrawCube(gameObject.transform.position , Vector3.one);

    }
}
