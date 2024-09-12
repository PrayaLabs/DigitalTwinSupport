using System;
using UnityEngine;

public class SimpleRotation : MonoBehaviour
{
    // Current angle on the Y-axis
    private float currentYRotation = 0f;
    private float currentZRotation = 0f;
    // Maximum and minimum angles
    private const float maxRotation = 180f;
    private const float minRotation = 0f;
    // Rotation increment
    private const float rotationStep = 10f;

    public GameObject gameObject1,gameObject2;

    void Update()
    {
        // Rotate right when the right arrow key is pressed
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            RotateRight();
        }

        // Rotate left when the left arrow key is pressed
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            RotateLeft();
        }

        // Rotate Up when the Up arrow key is pressed
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            RotateUp();
        }

        // Rotate Down when the Down arrow key is pressed
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            RotateDown();
        }
    }

    private void RotateDown()
    {
        if (currentZRotation > minRotation)
        {
            currentZRotation -= rotationStep;
            currentZRotation = Mathf.Max(currentZRotation, minRotation); // Clamp to maxRotation
            gameObject2.transform.rotation = Quaternion.Euler(0,0,-currentZRotation);
        }
    }

    private void RotateUp()
    {
        if (currentZRotation < maxRotation)
        {
            currentZRotation += rotationStep;
            currentZRotation = Mathf.Min(currentZRotation, maxRotation); // Clamp to maxRotation
            gameObject2.transform.rotation = Quaternion.Euler(0,0,-currentZRotation);
        }
    }

    // Rotate the object to the right by 10 degrees, but don't exceed 180 degrees
    void RotateRight()
    {
        if (currentYRotation < maxRotation)
        {
            currentYRotation += rotationStep;
            currentYRotation = Mathf.Min(currentYRotation, maxRotation); // Clamp to maxRotation
            gameObject1.transform.rotation = Quaternion.Euler(0, currentYRotation, 0);
        }
    }

    // Rotate the object to the left by 10 degrees, but don't go below 0 degrees
    void RotateLeft()
    {
        if (currentYRotation > minRotation)
        {
            currentYRotation -= rotationStep;
            currentYRotation = Mathf.Max(currentYRotation, minRotation); // Clamp to minRotation
            gameObject1.transform.rotation = Quaternion.Euler(0, currentYRotation, 0);
        }
    }
}
