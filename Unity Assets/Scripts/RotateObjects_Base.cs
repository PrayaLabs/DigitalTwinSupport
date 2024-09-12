using UnityEngine;

public class RotateObjects_Base : MonoBehaviour
{
    // Public references to the three GameObjects to rotate
  //  public GameObject object1;
    //public GameObject object2;
    //public GameObject object3;

    // Rotation increment (in degrees)
    public float rotationSpeed = 10f;

    void Update()
    {
        // Define the rotation increment (around Y-axis)
      //   Vector3 rotationIncrement = new Vector3(0, rotationSpeed * Time.deltaTime, 0);
        Vector3 rotationIncrement_z = new Vector3(0,rotationSpeed * Time.deltaTime,0);
        // Rotate all three objects around the Y-axis
        /* if (object1 != null) object1.transform.Rotate(rotationIncrement);
         if (object2 != null) object2.transform.Rotate(rotationIncrement);
         if (object3 != null) object3.transform.Rotate(rotationIncrement_z);*/
        transform.Rotate(rotationIncrement_z,Space.Self);
    }
}
