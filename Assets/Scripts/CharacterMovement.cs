using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour {
    public GameObject CameraObject;
    public GameObject CameraPlace;
    public GameObject CameraParent;
    public GameObject cameraFinalPos;
    public GameObject neck;
    Animator animator;
    public GameObject spine;
    Quaternion spineInitialLocalRotation;
    public static bool leftHanded { get; private set; }

    public bool isDead;

    // Use this for initialization
    void Start () {
        
        animator = GetComponent<Animator>(); //runs animations
        // Initializing animator values
        animator.SetFloat("walk_forward", 0.0f);
        animator.SetFloat("walk_backward", 0.0f);
        animator.SetFloat("walk_right", 0.0f);
        animator.SetFloat("walk_left", 0.0f);

        // Setting Initial rotation of spine to make it 
        spineInitialLocalRotation = Quaternion.Euler(new Vector3(0.0f, 40.0f, 0.0f));
        

    }

    // Update is called once per frame
    void Update () {
        OVRInput.Update();
        // Getting touch-pad touch position
        Vector2 touchPos = OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad, OVRInput.Controller.RTrackedRemote);
        // getting touch pad pos in vetor 2
        
        // <0.1, no move
        if (!isDead)
        {
            // The player should not move
            if (touchPos.magnitude < 0.1f)
            {
                animator.SetFloat("walk_forward", -1f);
                animator.SetFloat("walk_backward", -1f);
                animator.SetFloat("walk_right", -1f);
                animator.SetFloat("walk_left", -1f);
                animator.SetFloat("animation_speed", 0.0f);
            }
            else // The player should move
            {
                float forwardSpeed = touchPos.y;
                if (forwardSpeed > 0) // making forward walking speed faster
                {
                    forwardSpeed = forwardSpeed * 2;
                }

                // Running the correct animation
                animator.SetFloat("walk_forward", forwardSpeed);
                animator.SetFloat("walk_backward", -touchPos.y);
                animator.SetFloat("walk_right", touchPos.x);
                animator.SetFloat("walk_left", -touchPos.x);

                // Setting animation running speed
                animator.SetFloat("animation_speed", Mathf.Sqrt(Mathf.Pow(touchPos.x, 2f) + Mathf.Pow(forwardSpeed, 2f)));
            }   
        }
    }



    private void LateUpdate()
    {

        // If the handset is held with right hand or left hand
        leftHanded = OVRInput.GetControllerPositionTracked(OVRInput.Controller.LTouch);
        OVRInput.Controller c = leftHanded ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;

        if (OVRInput.GetControllerPositionTracked(c) && !isDead)
        {
            // Rotating the player spine according to handset rotation
            spine.transform.rotation = OVRInput.GetLocalControllerRotation(c);
            spine.transform.localRotation = spineInitialLocalRotation * spine.transform.localRotation;
           

            if (Quaternion.Angle(Quaternion.identity, spine.transform.localRotation) > 30.0f) // Limiting the spine rotation  
            {
                Quaternion currentLocalRotation = spine.transform.localRotation;
                spine.transform.localRotation = Quaternion.Slerp(Quaternion.identity, currentLocalRotation, 30.0f / Quaternion.Angle(Quaternion.identity, spine.transform.localRotation)); //Spherical interpolation of the spine rotation 
            }
        }
        

        if (!isDead) // Rotating the character according to headset rotation.
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(CameraObject.transform.forward.x, 0.0f, CameraObject.transform.forward.z).normalized, Vector3.up);
        }


        if (!isDead) // Moving the camera to a predifined position according to spine position and rotation
        {
            CameraParent.transform.position = CameraPlace.transform.position;
        }
        else // Moving camera to a fixed point when the player is killed
        {
            CameraParent.transform.position = cameraFinalPos.transform.position;
        }
    }

}
