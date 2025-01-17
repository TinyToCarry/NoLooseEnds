using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public Camera cam;
    [SerializeField]
    private float distance = 3f;
    [SerializeField]
    private LayerMask mask;
    private PlayerUI playerUI;

    void Start()
    {
        playerUI = GetComponent<PlayerUI>();
    }

    void Update()
    {
        playerUI.UpdateText(string.Empty);  // Clear UI prompt by default

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * distance);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, distance, mask))
        {
            Interactable interactable = hitInfo.collider.GetComponent<Interactable>();

            if (interactable != null)
            {
                // Show prompt message
                playerUI.UpdateText(interactable.promptMessage);

                // Check for "E" key press to interact
                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactable.BaseInteract(); // Trigger the interaction
                }
            }
        }
    }
}

