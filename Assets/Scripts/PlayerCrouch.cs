using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCrouch : MonoBehaviour
{
    private bool isCrouching = false;
    private float normalHeight = 1f;
    private float crouchHeight = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if(!Slide) (when i add the script)
        Crouch();
    }

    void Crouch()
        {
            float transitionSpeed = 5f;
            float oldHeight, newHeight;
            
            if (Input.GetKey(KeyCode.E))
            {
                if (!isCrouching)
                {
                    isCrouching = true;
                    speed *= 0.5f;
                }
                oldHeight = transform.localScale.y;
                newHeight = Mathf.Lerp(transform.localScale.y, crouchHeight, Time.deltaTime * transitionSpeed);
                transform.position += Vector3.up * (oldHeight - newHeight) * 0.5f;
                transform.localScale = new Vector3(transform.localScale.x, newHeight, transform.localScale.z);
                HeadCameraOffset.y = Mathf.Lerp(HeadCameraOffset.y, crouchHeight, Time.deltaTime * transitionSpeed);
            }
            else if (isCrouching || transform.localScale.y < normalHeight)
            {
                if (isCrouching)
                {
                    isCrouching = false;
                    speed *= 2f;
                }
                oldHeight = transform.localScale.y;
                newHeight = Mathf.Lerp(transform.localScale.y, normalHeight, Time.deltaTime * transitionSpeed);
                transform.position += Vector3.up * (newHeight - oldHeight) * 0.5f;
                transform.localScale = new Vector3(transform.localScale.x, newHeight, transform.localScale.z);
                HeadCameraOffset.y = Mathf.Lerp(HeadCameraOffset.y, normalHeight, Time.deltaTime * transitionSpeed);
            }
        }
}
