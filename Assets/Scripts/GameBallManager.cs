using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

using Oculus.Interaction;
using Oculus.Interaction.HandPosing;

public class GameBallManager : MonoBehaviour
{
    // Getting useful Scene objects and scripts
    public GameManagerScript gameManagerScript;

    // [SerializeField]
    public HandGrabInteractable handGrabInteractable;
    // [SerializeField]
    public GrabInteractable GrabInteractable;

    [HideInInspector]
    public ManagerSO managerSO;

    // Saving item variables
    public Rigidbody _rigidbody;
    private Material _material;
    private Vector3 _initial_position;
    private Quaternion _initial_rotation;

    // States for each ball in a game
    public enum GameBallState
    {
        Idle,
        Grabbed,
        Thrown,
        Inactive,
    }
    private GameBallState _ballState = GameBallState.Idle;
    public GameBallState ballState 
    { 
        get { 
            return _ballState; 
        }
        private set {
            // Watching and reacting to the state change [ Like a state machine ]
            this._ballState = value;

            OnGameBallStateChange.Invoke(value);
            
            switch(value){
                case GameBallState.Idle: 

                    UpdateMaterialColor(true);

                    break;
                case GameBallState.Grabbed:

                    break;
                case GameBallState.Thrown:

                    if(gameManagerScript.IsCheatMode)
                        Task.Run(() => CalculateCheatModeSettings());

                    break;
                case GameBallState.Inactive:
                    _centerOfMass = Vector3.zero;
                    if(gameManagerScript.IsCheatMode)
                        Task.Run(() => gameManagerScript.VisualizeCenterOfMass(false));

                    UpdateMaterialColor(false);



                    break;

                }
        }  
    }
    public UnityEvent<GameBallState> OnGameBallStateChange;

    // -- Variables used in the cheatmode attraction
    private Vector3 _centerOfMass = new Vector3(0,0,0);
    private bool _assistModeVariables_ready = false;

    private Vector3 Orthonormal_to_direction;

    //Audio variables
    // [SerializeField]
    // public AudioTrigger triggerScript_flyingball;
    // [SerializeField]
    // public AudioTrigger triggerScript_collision;



    private void Awake()
    {
        if (OnGameBallStateChange == null)
            OnGameBallStateChange = new UnityEvent<GameBallState>();

        // Reference to rigidbody for collision velocity calculations
        _rigidbody = gameObject.GetComponent<Rigidbody>();

        // Initial position and rotation for reset
        _initial_position = gameObject.transform.position;
        _initial_rotation = gameObject.transform.rotation;

        _material = gameObject.GetComponentInChildren<Renderer>().material;

        Orthonormal_to_direction = new Vector3();
    }


    public void InteractableBallGrabbed(){
        ballState = GameBallState.Grabbed;
    }

    public void InteractableBallThrown(){
        ballState = GameBallState.Thrown;
    }

    private void HandleStateChange(InteractableStateChangeArgs args)
    {

        // Oculus signal of ball grabbed
        if(args.Equals(InteractorState.Select)){
            
        }else if( args.PreviousState.Equals(InteractorState.Select)){
            ballState = GameBallState.Thrown;

            // Pragmatically, we can trigger the effect sound of the ball swishing when it leaves the throw-area
            // if(triggerScript_flyingball != null && _rigidbody.velocity.sqrMagnitude >= .5)
            //     triggerScript_flyingball.PlayAudio();

        }

    }

    public void Reset()
    {
        // Making it not move when reseting!!
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        // Reseting the object to the initial position
        gameObject.transform.position = _initial_position;
        gameObject.transform.rotation = _initial_rotation;

        // Resetting assist mode related variables
        _assistModeVariables_ready = false;
        _centerOfMass = new Vector3(0,0,0);
        
        // Setting as active
        ballState = GameBallState.Idle;   
    }

    private void FixedUpdate() 
    {

        if(ballState == GameBallState.Thrown){

            if(gameManagerScript.IsCheatMode)
                AttractBallToTarget();

            // TODO: Determine if sleeping rigidbody also works
            if(_rigidbody.velocity.sqrMagnitude <= 0.002f)
                StartCoroutine(SetInactive());
                
        } 

    }

    IEnumerator SetInactive()
    {
        // Wait 1 second
        yield return new WaitForSeconds(0.5f);

        // Check again for the ball state and speed
        if(ballState == GameBallState.Thrown && _rigidbody.velocity.sqrMagnitude <= 0.002f)
            ballState = GameBallState.Inactive;

    }

    void OnTriggerExit(Collider other)
    {
        // Ball respawn when it leaves the ball idle zone without being grabbed
        if(ballState == GameBallState.Idle && other.gameObject.CompareTag("GameBallIdleBounds"))
            Reset();

    }

    private void CalculateCheatModeSettings()
    {
        _centerOfMass = gameManagerScript.GetMeanTargetCenterOfMass();

        _centerOfMass = Vector3.Lerp(_centerOfMass, transform.position, .3f);
                    
        // Ignoring gravity; only interested in direction on the plane x,z
        Vector3 ballDirection = _rigidbody.velocity;
        ballDirection.y = 0;

        // Cross product of x&z velocity components with the up vector returns the orthonormal vector pointing right to the ball throw
        Orthonormal_to_direction = Vector3.Cross( ballDirection, Vector3.up ).normalized;

        _assistModeVariables_ready = true;
                
        // Run parallel task that shows the "black whole" for the assist mode
        Task.Run(() => gameManagerScript.VisualizeCenterOfMass(true, _centerOfMass));
    }
    

    // void OnDrawGizmos() 
    // {
    //     if (Application.isPlaying) {

    //         if(gameManagerScript.IsCheatMode){


    //             Gizmos.color = Color.blue;
    //             Gizmos.DrawSphere( _centerOfMass, 0.4f);

    //             Vector3 handle_shift = new Vector3(0 , .2f, 0);
    //             Handles.Label(transform.position + handle_shift, "pull: " + debug_value );
    

    //             Gizmos.color = Color.cyan;
    //             Gizmos.DrawLine(transform.position, transform.position + debug_value * Orthonormal_to_direction);

    //         }


    //     }

    // }


    void AttractBallToTarget ()
    {
        // Assist mode variables are calculated parallely, if they are not ready, there is no attraction
        if( ! _assistModeVariables_ready)
            return;
        
        // Getting the distance vector of the ball and the center of mass
        float pull_magnitude = (_rigidbody.position - _centerOfMass).sqrMagnitude;

        // Being carefull not to devidee by cero (even when highly improbable)
        // Equation design to work towards going near the raycast between the thrown ball and the targets instead of a point.
        // pull_magnitude = (float)(30 * ( pull_magnitude - 6 * pull_magnitude* pull_magnitude )/ (12.5 * gameManagerScript.CheatModePower));

        pull_magnitude = (gameManagerScript.CheatModePower / 5) / (pull_magnitude + .01f);
        
        if( pull_magnitude < 0) pull_magnitude = 0;
        
        //Creating the force accordingly
        Vector3 CheatModeForce = Orthonormal_to_direction * IsRightFromDirection(_centerOfMass) * pull_magnitude  ;

        // Finally adding the force to the object
        _rigidbody.AddForce(CheatModeForce);

    }

    private float IsRightFromDirection(Vector3 _centerOfMass){
        float angle = Vector3.SignedAngle(
                            new Vector3(_rigidbody.position.x + _rigidbody.velocity.x, 0, _rigidbody.position.z + _rigidbody.velocity.z), 
                            new Vector3(_centerOfMass.x, 0, _centerOfMass.z),
                            new Vector3(_rigidbody.position.x, 0 ,_rigidbody.position.z)
                            );

        return Mathf.Sign(angle);
    }




    private void UpdateMaterialColor(bool active){

        // Change color of the gameball when thrown
        if(_material != null)
            _material.color = (active)? Color.white: Color.black;

    }

    // private void PlayCollisionAudio(Collision collision){

    //     //TODO: Determine a proper magnitude to signal colission sound (ping pong ball sound)
    //     // Change color of the gameball when thrown
    //     if(triggerScript_collision != null && collision.relativeVelocity.sqrMagnitude > gameManagerScript.GameBallCollisionAudioThreshold)
    //         triggerScript_collision.PlayAudio();
    // }

    public bool IsActive(){
        return (ballState != GameBallManager.GameBallState.Inactive);
    }

    public void ThrowAsDebug(Vector3 ThrowPosition, Vector3 ThrowDirection){
        Reset();
        // Mimic grabbed
        ballState = GameBallState.Grabbed;

        //Make the ball static at the throw position
        transform.position = ThrowPosition;
        _rigidbody.velocity = Vector3.zero;

        //Add throwing force and mark as thrown
        _rigidbody.velocity = ThrowDirection;

        ballState = GameBallState.Thrown;
        
    }


}
