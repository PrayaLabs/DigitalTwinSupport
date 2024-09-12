using UnityEngine;

public class RotateObjects_Top : MonoBehaviour
{
    // Rotation increment (in degrees per second)
    public float rotationSpeed = 50f;

    // The target rotation angles on the Z-axis
    private float minRotationAngle = -90f;
    private float maxRotationAngle = 0f;

    // The current direction of rotation (true = rotating towards -90, false = back to 0)
    private bool rotateToMin = true;

    void Update()
    {
        // Get the current local rotation around the Z-axis
        float currentZRotation = transform.localEulerAngles.z;

        // Normalize the angle to be between -180 and 180 for easier comparison
        if (currentZRotation > 180) currentZRotation -= 360;

        // Check the direction of rotation and update the rotation
        if (rotateToMin)
        {
            // Rotate towards -90 degrees on the Z-axis
            if (currentZRotation > minRotationAngle)
            {
                transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime, Space.Self);
            }
            else
            {
                // Switch direction when reaching -90 degrees
                rotateToMin = false;
            }
        }
        else
        {
            // Rotate back towards 0 degrees on the Z-axis
            if (currentZRotation < maxRotationAngle)
            {
                transform.Rotate(0, 0, rotationSpeed * Time.deltaTime, Space.Self);
            }
            else
            {
                // Switch direction when reaching 0 degrees
                rotateToMin = true;
            }
        }
    }
}
