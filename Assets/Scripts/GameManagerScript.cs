using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Script has run priority
[DefaultExecutionOrder(-5)]
public class GameManagerScript : MonoBehaviour
{
    // --- Game Logic variables
    public ManagerSO managerSO;


    // Encapsulating the management of GameStates
    public enum GameStateType  
    {
        Awake,
        Bootstrap,
        Playing,
        Scoring,
        FinishedGame,
    }
    private GameStateType _GameState = GameStateType.Awake;
    public GameStateType GameState
    {
        get { return _GameState; }
        set { _GameState = value; OnChangeGameState.Invoke(value); }
    }

    [System.NonSerialized]
    public UnityEvent<GameStateType> OnChangeGameState;

    // --- Managing Game assets through the game manager
    private List<TargetCollisionManager> GameTargets;
    private List<GameBallManager> GameBalls;

    // ---- Audio source management ----
    [SerializeField]
    public AudioSource winSoundAudio;

    [SerializeField]
    public AudioSource loseSoundAudio;
    
    [SerializeField, Range(0, 10)]
    public float GameBallCollisionAudioThreshold = 2f;




    [System.NonSerialized]
    public UnityEvent<int> OnScoreChange;
    
    [System.NonSerialized]
    public UnityEvent<bool> OnAssistModeChange;



    private void OnEnable (){
        #if UNITY_STANDALONE
        // Activate the VR only objects
        
        #endif

        if(OnScoreChange == null)
            OnScoreChange = new UnityEvent<int>();

        if(OnChangeGameState == null)
            OnChangeGameState = new UnityEvent<GameStateType>();

        if(OnAssistModeChange == null)
            OnAssistModeChange = new UnityEvent<bool>();
        
    }


    private void Start(){
        BootstrapGame();
    }

    // ------- General Game Logic ------

    private void FixedUpdate(){
        if( IsPlaying() ){

            // GamePlay finishes when there are NO Active targets or NO active balls
            if( NoActiveTargetExist() || NoActiveBallExist() )
                CountFinalScore();

        }

    }


    public void BootstrapGame(){
        
        GameState = GameStateType.Bootstrap;
        
        ResetScore();
        
        InitGameTargets();
        InitGameBalls();
        
        GameState = GameStateType.Playing;

    }


    //--------- Reference and managing Interactable GameTargets ------------
    private void InitGameTargets(){
        // Referencing all game targets to check on them:
        //   * When they are hit, we add to the cumulative score saved as the HitScoresList
        //   * When a ball is thrown, we need to check which targets are still active to calculate everything
        //   * When the game finishes we access to the targets to add up the finally score
        //   * When reseting the game we can put the targets at their initial position

        // Removing previous listeners (if any)
        if(GameTargets != null)
            foreach(TargetCollisionManager t in GameTargets)
                t.OnTargetHit.RemoveListener(OnTargetBeingHit);
                
        // Reseting GameTargets (number may change during game in the future)
        GameTargets = new List<TargetCollisionManager>(FindObjectsOfType<TargetCollisionManager>());

        foreach(TargetCollisionManager t in  GameTargets){
            t.InitTarget();
            if(t.OnTargetHit != null)
                t.OnTargetHit.AddListener(OnTargetBeingHit);
        }

    }

    private void OnTargetBeingHit(Collision collision){
        //Calculating the Hit Score
        int mag = (int)(collision.relativeVelocity.sqrMagnitude);
        AddHit(mag);
    }

    private bool NoActiveTargetExist(){
        return ! GameTargets.Exists(e => e.InTargetZone);
    }

    //--------- Reference and managing Interactable GameBalls ------------

    private void InitGameBalls(){
        // Referencing all the game balls:
        //    * When all balls are inactive we can finish the game
        //    * When reseting the game we can put the balls at their original position

        GameBalls = new List<GameBallManager>(FindObjectsOfType<GameBallManager>());

        foreach(GameBallManager b in GameBalls)
            b.Reset();
        
    }

    private bool NoActiveBallExist(){
        // Check all active balls for a stage change
        return ! GameBalls.Exists(ball => ball.IsActive());
    }

    public Vector3 GetMeanTargetCenterOfMass(){
        Vector3 sum_vector = new Vector3(0,0,0);

        int active_target_count = 0;

        foreach (TargetCollisionManager t in GameTargets){
            if(t.InTargetZone){
                sum_vector += t.GetRigidbody().position + t.GetRigidbody().centerOfMass;
                active_target_count++;
            }
                
        }
        
        sum_vector = sum_vector/active_target_count;

        return sum_vector;
    }



    public bool IsPlaying(){
        return (GameState == GameStateType.Playing);
    }


    // --- Cheat Mode Manager ---

    [SerializeField, Range(1, 5)]
    public float CheatModePower = 5;

    [SerializeField, HideInInspector]
    private bool _IsCheatMode = false;

    public bool IsCheatMode
    {
        get { return _IsCheatMode; }
        private set {  
            _IsCheatMode = value; 
            Physics.gravity = new Vector3(0, value? -5f : -9.81f , 0);
            OnAssistModeChange.Invoke(_IsCheatMode);
            }
    }    

    public void ToggleCheatMode(){
        IsCheatMode = !IsCheatMode;
    }

    // --- Managing state Score ---

    private List<int> HitScoresList = new List<int>();

    private int _TotalScore = 0;
    public int TotalScore
    {
        get { return _TotalScore; }
        private set {  _TotalScore = value; OnScoreChange.Invoke(value);}
    }    

    public void ResetScore(){
        // Clearing previous score list
        HitScoresList.Clear();
        TotalScore = 0;
    }

    public float AddHit(int value){
        HitScoresList.Add(value);
        TotalScore += value * managerSO.score_multiplier__target_hit;
        return TotalScore;
    }

    private void CountFinalScore(){

        GameState = GameStateType.Scoring;

        int cumulative = 0;

        // Adding to the score the distance of each can
        foreach (TargetCollisionManager target in GameTargets)
            cumulative += target.GetDistanceScore();

        foreach(int hit in HitScoresList)
            cumulative += hit * managerSO.score_multiplier__target_hit;

        foreach( GameBallManager ball in GameBalls)
            if (ball.IsActive())
                cumulative += managerSO.score_multiplier__unused_ball;

        // Setting the final score!
        TotalScore = cumulative;

        // Playing win or lose tune
        if(TotalScore != 0)
            winSoundAudio.Play();
        else    
            loseSoundAudio.Play();

        GameState = GameStateType.FinishedGame;

        // TODO: Prompt interface to reset the game
    }

}

