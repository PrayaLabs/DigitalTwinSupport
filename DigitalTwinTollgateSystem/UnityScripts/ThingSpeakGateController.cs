using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ThingSpeakGateController : MonoBehaviour
{
    [Header("ThingSpeak Settings")]
    public string channelId = "";
    public int fieldNumber = 1;
    public string readApiKey = "";
    public string writeApiKey = "";
    public float pollInterval = 5f;

    [Header("Gate Animator")]
    public Animator gateAnimator;
    public string boolParameterName = "IsOpen";

    [Header("Traffic Lights")]
    public Light redLight;
    public Light greenLight;

    [Header("Options")]
    public bool startClosed = true;
    public bool debugLogs = true;

    private int lastFieldValue = -9999;
    private bool isBusyReading = false;

    private void Start()
    {
        if (gateAnimator == null)
        {
            Debug.LogError("Gate Animator is not assigned.");
            return;
        }

        if (startClosed)
        {
            ApplyClosedState();
            lastFieldValue = 0;
        }

        StartCoroutine(PollThingSpeak());
    }

    private IEnumerator PollThingSpeak()
    {
        while (true)
        {
            if (!isBusyReading)
            {
                yield return StartCoroutine(ReadThingSpeakField());
            }

            yield return new WaitForSeconds(pollInterval);
        }
    }

    private IEnumerator ReadThingSpeakField()
    {
        isBusyReading = true;

        if (string.IsNullOrWhiteSpace(channelId))
        {
            Debug.LogError("ThingSpeak Channel ID is empty.");
            isBusyReading = false;
            yield break;
        }

        string url = string.IsNullOrWhiteSpace(readApiKey)
            ? $"https://api.thingspeak.com/channels/{channelId}/fields/{fieldNumber}/last.json"
            : $"https://api.thingspeak.com/channels/{channelId}/fields/{fieldNumber}/last.json?api_key={readApiKey}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
#else
            if (request.isHttpError || request.isNetworkError)
#endif
            {
                Debug.LogError("ThingSpeak Read Error: " + request.error);
                isBusyReading = false;
                yield break;
            }

            string json = request.downloadHandler.text;

            if (debugLogs)
                Debug.Log("ThingSpeak Response: " + json);

            int value = ParseFieldValue(json, fieldNumber);

            if (value == -9999)
            {
                Debug.LogWarning("Could not parse ThingSpeak field value.");
                isBusyReading = false;
                yield break;
            }

            if (value != lastFieldValue)
            {
                lastFieldValue = value;

                if (value == 1)
                    ApplyOpenState();
                else if (value == 0)
                    ApplyClosedState();
                else if (debugLogs)
                    Debug.Log("Ignoring unsupported field value: " + value);
            }
        }

        isBusyReading = false;
    }

    private int ParseFieldValue(string json, int fieldNum)
    {
        string key = $"\"field{fieldNum}\":\"";
        int startIndex = json.IndexOf(key);

        if (startIndex < 0)
            return -9999;

        startIndex += key.Length;
        int endIndex = json.IndexOf("\"", startIndex);

        if (endIndex < 0)
            return -9999;

        string valueText = json.Substring(startIndex, endIndex - startIndex).Trim();

        if (int.TryParse(valueText, out int parsedValue))
            return parsedValue;

        return -9999;
    }

    public void ApplyOpenState()
    {
        if (gateAnimator == null)
        {
            Debug.LogError("Gate Animator is not assigned.");
            return;
        }

        gateAnimator.SetBool(boolParameterName, true);

        if (greenLight != null) greenLight.enabled = true;
        if (redLight != null) redLight.enabled = false;

        if (debugLogs)
            Debug.Log("Gate OPEN - Green ON");
    }

    public void ApplyClosedState()
    {
        if (gateAnimator == null)
        {
            Debug.LogError("Gate Animator is not assigned.");
            return;
        }

        gateAnimator.SetBool(boolParameterName, false);

        if (greenLight != null) greenLight.enabled = false;
        if (redLight != null) redLight.enabled = true;

        if (debugLogs)
            Debug.Log("Gate CLOSED - Red ON");
    }

    public void ResetEnvironmentToZero()
    {
        StartCoroutine(SendFieldValueToThingSpeak(0));
    }

    public void SendOneToThingSpeak()
    {
        StartCoroutine(SendFieldValueToThingSpeak(1));
    }

    public void SendZeroToThingSpeak()
    {
        StartCoroutine(SendFieldValueToThingSpeak(0));
    }

    private IEnumerator SendFieldValueToThingSpeak(int value)
    {
        if (string.IsNullOrWhiteSpace(writeApiKey))
        {
            Debug.LogError("ThingSpeak Write API Key is empty.");
            yield break;
        }

        string url = "https://api.thingspeak.com/update.json";
        WWWForm form = new WWWForm();
        form.AddField("api_key", writeApiKey);
        form.AddField("field" + fieldNumber, value);

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            yield return request.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
#else
            if (request.isHttpError || request.isNetworkError)
#endif
            {
                Debug.LogError("ThingSpeak Write Error: " + request.error);
                yield break;
            }

            if (debugLogs)
                Debug.Log("ThingSpeak Write Success: " + request.downloadHandler.text);

            lastFieldValue = value;

            if (value == 1)
                ApplyOpenState();
            else
                ApplyClosedState();
        }
    }

    // Optional keyboard testing
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
            ApplyOpenState();

        if (Input.GetKeyDown(KeyCode.C))
            ApplyClosedState();

        if (Input.GetKeyDown(KeyCode.Alpha1))
            SendOneToThingSpeak();

        if (Input.GetKeyDown(KeyCode.Alpha0))
            ResetEnvironmentToZero();
    }
}