using UnityEngine;

public class GateAnimationTester : MonoBehaviour
{
    public Animator gateAnimator;
    public Light redLight;
    public Light greenLight;

    void Start()
    {
        // Start closed
        gateAnimator.SetBool("IsOpen", false);

        if (greenLight != null) greenLight.enabled = false;
        if (redLight != null) redLight.enabled = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
            OpenGate();

        if (Input.GetKeyDown(KeyCode.C))
            CloseGate();
    }

    public void OpenGate()
    {
        gateAnimator.SetBool("IsOpen", true);

        if (greenLight != null) greenLight.enabled = true;
        if (redLight != null) redLight.enabled = false;

        Debug.Log("Gate Open");
    }

    public void CloseGate()
    {
        gateAnimator.SetBool("IsOpen", false);

        if (greenLight != null) greenLight.enabled = false;
        if (redLight != null) redLight.enabled = true;

        Debug.Log("Gate Close");
    }
}