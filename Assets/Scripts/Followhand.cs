using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
using UnityEngine.XR.WSA.Input;


public class Followhand : MonoBehaviour

{
    [SerializeField]
    public GameObject FollowCube;
    private GameObject indicator = null;
    private TextMesh textMesh = null;
    private Vector3 temp_Position = new Vector3(0, -990, 0);

    //private float MaxRayDistance = 10.0f;
    //private LayerMask RaycastLayer = Physics.DefaultRaycastLayers;


    /*private void CreateIndicator()
    {
        if (indicator == null)
        {
            indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
            indicator.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
        }
    }
    */
    private void UpdateIndicator(Vector3 position)
    {


        Debug.Log("Updated Hand Position" + position);
        FollowCube.transform.position = position;
        Debug.Log("Moving Cube with hand");


    }

    private void castChecker(Vector3 position, Vector3 forward, Vector3 velocity)
    {
        Debug.Log("Forward " + forward + " velocity " + velocity);

    }


    /*public void ShowObjects(bool show)
    {
        if (indicator != null && textMesh != null)
        {
            
        }
    }*/

    // Start is called before the first frame update
    void Start()
    {
        //CreateIndicator();
        //CreateText();
        //InteractionManager.InteractionSourceLost += InteractionManager_InteractionSourceLost;
        //InteractionManager.InteractionSourceDetected += InteractionManager_SourceDetected;
        //InteractionManager.InteractionSourceUpdated += InteractionManager_SourceUpdated;

        InteractionManager.InteractionSourceDetected += InteractionManager_InteractionSourceDetected;

        //InteractionManager.InteractionSourcePressed += InteractionManager_InteractionSourcePressed;
        //InteractionManager.InteractionSourceReleased += InteractionManager_InteractionSourceReleased;
        InteractionManager.InteractionSourceUpdated += InteractionManager_InteractionSourceUpdated;
        InteractionManager.InteractionSourceLost += InteractionManager_InteractionSourceLost;


    }



    void InteractionManager_InteractionSourceDetected(InteractionSourceDetectedEventArgs args)
    {
        Debug.Log("Inside Interaction detected");
        var interactionSourceState = args.state;
        if (interactionSourceState.source.kind == InteractionSourceKind.Hand)
        {
            Debug.Log("Detected Hand");
            //indicator.SetActive(true);
            //textMesh.gameObject.SetActive(true);
        }

    }

    void InteractionManager_InteractionSourceLost(InteractionSourceLostEventArgs args)
    {
        var interactionSourceState = args.state;
        //if (interactionSourceState.source.kind == InteractionSourceKind.Hand)
        if (interactionSourceState.source.kind == InteractionSourceKind.Hand)
        {
            FollowCube.transform.position = temp_Position;
            Debug.Log("Hand Lost");
        }
    }


    void InteractionManager_InteractionSourceUpdated(InteractionSourceUpdatedEventArgs args)
    {
        var interactionSourceState = args.state;
        if (interactionSourceState.source.kind == InteractionSourceKind.Hand)
        {
            Debug.Log("Update Hand");
            Vector3 handPosition;
            Vector3 handVelocity;
            Vector3 handForward;

            interactionSourceState.sourcePose.TryGetPosition(out handPosition);
            interactionSourceState.sourcePose.TryGetVelocity(out handVelocity);
            interactionSourceState.sourcePose.TryGetForward(out handForward);

            //UpdateText(handPosition, handVelocity);
            UpdateIndicator(handPosition);
            castChecker(handPosition, handForward, handVelocity);
        }
    }

    void OnDestroy()
    {
        InteractionManager.InteractionSourceDetected -= InteractionManager_InteractionSourceDetected;
        InteractionManager.InteractionSourceLost -= InteractionManager_InteractionSourceLost;
        //InteractionManager.InteractionSourcePressed -= InteractionManager_InteractionSourcePressed;
        //InteractionManager.InteractionSourceReleased -= InteractionManager_InteractionSourceReleased;
        InteractionManager.InteractionSourceUpdated -= InteractionManager_InteractionSourceUpdated;
    }

}
