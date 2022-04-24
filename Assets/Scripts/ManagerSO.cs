using UnityEngine;
using UnityEngine.Events;


[CreateAssetMenu(fileName = "GameData", menuName = "ScriptableObjects/ManagerSO", order = 1)]
public class ManagerSO : ScriptableObject
{
    // Game Preferences
    public int score_multiplier__unused_ball = 5000;
    public int score_multiplier__target_travel_distance = 20;
    public int score_multiplier__target_hit = 50;

    // --------- Cheat mode variables ------------
    [SerializeField, Range(-5f, -9.81f)]
    public float CheatGravity = -6f;

    [SerializeField, Range(0, 1)]
    public float CenterOfMassDistance = 0.3f;

}
