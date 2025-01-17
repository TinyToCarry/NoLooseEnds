using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorButton : MonoBehaviour
{
    public Animator leftDoorAnimator;
    public Animator rightDoorAnimator;

    private void OnMouseDown()
    {
        // Trigger the opening of the doors when the button is clicked
        leftDoorAnimator.SetTrigger("Open");
        rightDoorAnimator.SetTrigger("Open");
    }
}
