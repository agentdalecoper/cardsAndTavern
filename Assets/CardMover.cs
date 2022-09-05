using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CardMover : MonoBehaviour
{
    private Camera mainCamera;
    private float CameraZDistance;
    private float y;

    void Start()
    {
        mainCamera = Camera.main;
        CameraZDistance =
            mainCamera.WorldToScreenPoint(transform.position).z; //z axis of the game object for screen view
        y = transform.position.y;
    }

    private float counter = 0;

    void OnMouseDrag()
    {
        Vector3 ScreenPosition =
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, CameraZDistance); //z axis added to screen point 
        Vector3 NewWorldPosition =
            mainCamera.ScreenToWorldPoint(ScreenPosition); //Screen point converted to world point

        transform.position = new Vector3(NewWorldPosition.x, transform.position.y, NewWorldPosition.z);
    }
    

}