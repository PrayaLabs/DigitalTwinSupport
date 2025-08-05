using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class ThingSpeakControl : MonoBehaviour
{
    [Header("ThingSpeak Settings")]
    public string channelID = "2194612";
    public string writeAPIKey = "YOUR_WRITE_API_KEY";
    public string readAPIKey = "A90G508S2955AUKI";

    [Header("Target GameObject")]
    public GameObject targetObject;

    [Header("Options")]
    public bool autoRead = true;
    public float readInterval = 15f;

    private Animator animator;

    void Start()
    {
        if (targetObject != null)
        {
            animator = targetObject.GetComponent<Animator>();
            if (animator != null)
            {
                animator.enabled = false;
            }
        }

        if (autoRead)
        {
            InvokeRepeating(nameof(BeginRead), 2f, readInterval);
        }
        else
        {
            BeginRead();
        }
    }

    public void SendOn()
    {
        StartCoroutine(SendToThingSpeak(1));
    }

    public void SendOff()
    {
        StartCoroutine(SendToThingSpeak(0));
    }

    private void BeginRead()
    {
        StartCoroutine(ReadFromThingSpeak());
    }

    private IEnumerator SendToThingSpeak(int value)
    {
        string url = $"https://api.thingspeak.com/update?api_key={writeAPIKey}&field1={value}";
        Debug.Log($"Sending to ThingSpeak: {url}");

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Data sent successfully: {value}");
        }
        else
        {
            Debug.LogError("Failed to send data to ThingSpeak: " + request.error);
        }
    }

    private IEnumerator ReadFromThingSpeak()
    {
        string url = $"https://api.thingspeak.com/channels/{channelID}/fields/1.json?api_key={readAPIKey}&results=1";
        Debug.Log($"Reading from ThingSpeak: {url}");

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            ThingSpeakFeedResponse response = JsonUtility.FromJson<ThingSpeakFeedResponse>(json);
            Debug.Log(response);

            if (response != null && response.feeds != null && response.feeds.Count > 0)
            {
                string field1Value = response.feeds[0].field1;
                Debug.Log(field1Value);

                if (int.TryParse(field1Value, out int fieldValue))
                {
                    Debug.Log($"Received field1: {fieldValue}");
                    UpdateAnimator(fieldValue);
                }
                else
                {
                    Debug.LogWarning("Field1 value is invalid or not an integer.");
                }
            }
            else
            {
                Debug.LogWarning("No feeds received from ThingSpeak.");
            }
        }
        else
        {
            Debug.LogError($"Failed to read from ThingSpeak: {request.responseCode} - {request.error}");
        }
    }

    private void UpdateAnimator(int value)
    {
        if (animator == null)
        {
            Debug.LogWarning("Animator is null.");
            return;
        }

        animator.enabled = (value == 1);
        Debug.Log("Animator " + (animator.enabled ? "enabled (ON)" : "disabled (OFF)"));
    }

    [System.Serializable]
        private class ThingSpeakFeedResponse
    {
        public Channel channel;
        public List<Feed> feeds;
    }

    [System.Serializable]
    private class Channel
    {
        public int id;
        public string name;
        public string latitude;
        public string longitude;
        public string field1;
        public string created_at;
        public string updated_at;
    }

    [System.Serializable]
    private class Feed
    {
        public string created_at;
        public int entry_id;
        public string field1;
    }

}
