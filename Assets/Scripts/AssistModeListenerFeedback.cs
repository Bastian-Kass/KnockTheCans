using UnityEngine;
using Oculus.Interaction;

public class AssistModeListenerFeedback : MonoBehaviour
{
    public GameManagerScript gameManager;
    public InteractableColorVisual button_visual;

    [SerializeField, Optional]
    public InteractableColorVisual.ColorState stateColor_true;
    [SerializeField, Optional]
    public InteractableColorVisual.ColorState stateColor_false;


    void OnEnable()
    {
        if(gameManager != null)
            gameManager.OnAssistModeChange.AddListener(ChangeButtonColor);
    }

    void OnDisable()
    {
        if(gameManager != null)
            gameManager.OnAssistModeChange.RemoveListener(ChangeButtonColor);
            
    }

    private void Start(){
 
        if(stateColor_true == null){
            stateColor_true = new InteractableColorVisual.ColorState();
            stateColor_true.Color = new Color(200, 40, 40, 40);
        }

        if(stateColor_false == null){
            stateColor_false = new InteractableColorVisual.ColorState();
            stateColor_false.Color = new Color(255, 255, 255, 255);
        }
    }

    private void ChangeButtonColor(bool state){
        
        Debug.Log(state);

        button_visual.InjectOptionalNormalColorState( state? stateColor_true: stateColor_false);

    }
}
