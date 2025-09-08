using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Concurrent;

using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

// TTS (Windows Editor/Standalone)
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using System.Speech.Synthesis;
#endif

public class SimpleMQTTWordSpeaker : MonoBehaviour
{
    [Header("MQTT")]
    public string brokerAddress = "91.121.93.94";
    public int brokerPort = 1883;
    public string topic = "/funobotz.kuttybot";
    public byte qos = MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE;

    [Header("Target Rotation")]
    public GameObject targetObject;     // object to rotate
    public float openAngle = 15f;       // X angle when "open"
    public float closedAngle = 0f;      // X angle when "closed"
    public float beatDuration = 0.25f;  // seconds per word (open+close)
    [Tooltip("Easing; leave linear if you want constant speed")]
    public AnimationCurve ease = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Text To Speech")]
    public bool enableTextToSpeech = true;
    [Range(-10, 10)] public int ttsRate = 2;    // speech rate (-10..10) for System.Speech
    [Range(0, 100)] public int ttsVolume = 100; // 0..100
    [Tooltip("SSML voice styling: higher pitch and a touch faster for kid-robot vibe")]
    public bool kidRobotVoice = true;

    // ---- internals ----
    private MqttClient client;
    private readonly ConcurrentQueue<string> inbox = new ConcurrentQueue<string>();
    private Coroutine rotateRoutine;

    private static readonly Regex WordRegex =
        new Regex(@"\b\w+\b", RegexOptions.Compiled);

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    private SpeechSynthesizer synthesizer;
#endif

    void Start()
    {
        if (targetObject == null)
        {
            Debug.LogError("[MQTT] Target GameObject not assigned!");
            return;
        }

        // Ensure closed pose at start
        SetAngle(closedAngle);

        // ---- MQTT ----
        client = new MqttClient(brokerAddress, brokerPort, false, null, null, MqttSslProtocols.None);
        client.MqttMsgPublishReceived += OnMsg;
        client.Connect(System.Guid.NewGuid().ToString());
        client.Subscribe(new[] { topic }, new[] { qos });

        // ---- TTS init (Windows only) ----
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        if (enableTextToSpeech)
        {
            synthesizer = new SpeechSynthesizer();
            synthesizer.Rate = Mathf.Clamp(ttsRate, -10, 10);
            synthesizer.Volume = Mathf.Clamp(ttsVolume, 0, 100);
        }
#else
        if (enableTextToSpeech)
            Debug.LogWarning("[TTS] System.Speech not available on this platform. Consider a platform TTS plugin.");
#endif
    }

    void Update()
    {
        while (inbox.TryDequeue(out var msg))
        {
            int words = CountWords(msg);
            Debug.Log($"[MQTT] {topic}: \"{msg}\" (words={words})");

            // Start rotation beats
            if (rotateRoutine != null) StopCoroutine(rotateRoutine);
            if (words > 0) rotateRoutine = StartCoroutine(RotateBeats(words));

            // Speak the text
            if (enableTextToSpeech) SpeakText(msg);
        }
    }

    void OnDestroy()
    {
        if (client != null)
        {
            try { if (client.IsConnected) client.Disconnect(); } catch { }
            client.MqttMsgPublishReceived -= OnMsg;
            client = null;
        }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        if (synthesizer != null)
        {
            try { synthesizer.SpeakAsyncCancelAll(); } catch { }
            synthesizer.Dispose();
            synthesizer = null;
        }
#endif
    }

    // -------- MQTT callback (worker thread) --------
    private void OnMsg(object s, MqttMsgPublishEventArgs e)
    {
        var text = Encoding.UTF8.GetString(e.Message);
        inbox.Enqueue(text);
    }

    // -------- Word count --------
    private int CountWords(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return 0;
        return WordRegex.Matches(s).Count;
    }

    // -------- Rotation beats (open→close per word) --------
    private IEnumerator RotateBeats(int n)
    {
        for (int i = 0; i < n; i++)
        {
            // Open (0 -> openAngle)
            yield return RotateToAngle(openAngle, beatDuration * 0.5f);
            // Close (openAngle -> 0)
            yield return RotateToAngle(closedAngle, beatDuration * 0.5f);
        }
        // Ensure closed at end
        SetAngle(closedAngle);
        rotateRoutine = null;
    }

    private IEnumerator RotateToAngle(float targetAngle, float duration)
    {
        float startAngle = targetObject.transform.localEulerAngles.x;
        if (startAngle > 180f) startAngle -= 360f; // normalize to [-180,180]

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, duration);
            float k = ease.Evaluate(Mathf.Clamp01(t));
            float newAngle = Mathf.Lerp(startAngle, targetAngle, k);
            Vector3 euler = targetObject.transform.localEulerAngles;
            euler.x = newAngle;
            targetObject.transform.localEulerAngles = euler;
            yield return null;
        }
        SetAngle(targetAngle);
    }

    private void SetAngle(float angle)
    {
        Vector3 euler = targetObject.transform.localEulerAngles;
        euler.x = angle;
        targetObject.transform.localEulerAngles = euler;
    }

    // -------- Text-to-Speech --------
    private void SpeakText(string text)
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        if (synthesizer == null) return;

        try
        {
            synthesizer.SpeakAsyncCancelAll(); // stop any prior speech

            if (kidRobotVoice)
            {
                // SSML for higher pitch & slightly faster “kid-robot” vibe
                // Note: Rate is also applied from synthesizer.Rate above
                string ssml =
                    $@"<?xml version=""1.0""?>
                    <speak version=""1.0"" xmlns=""http://www.w3.org/2001/10/synthesis"" xml:lang=""en-US"">
                        <prosody pitch=""+6st"" rate=""+10%"">{EscapeForSsml(text)}</prosody>
                    </speak>";
                synthesizer.SpeakSsmlAsync(ssml);
            }
            else
            {
                synthesizer.SpeakAsync(text);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[TTS] Speak failed: " + ex.Message);
        }
#else
        Debug.Log($"[TTS] (stub) {text}");
#endif
    }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    private static string EscapeForSsml(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
    }
#endif
}
