using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

public class MqttControl : MonoBehaviour
{
    // UI Elements
    public Button upButton, downButton, leftButton, rightButton;

    private MqttClient mqttClient;
    public string mqttHost = ""; // MQTT Server IP
    private int mqttPort = 1883; // Default MQTT Port
    private string clientId;

    private float servo1Angle = 0; // Initial angle for Servo1
    private float servo2Angle = 0; // Initial angle for Servo2

    void Start()
    {
        // Generate a unique client ID
        clientId = "unity_client_" + System.Guid.NewGuid().ToString();

        // Assign button click listeners
        upButton.onClick.AddListener(() => SendCommand("up"));
        downButton.onClick.AddListener(() => SendCommand("down"));
        leftButton.onClick.AddListener(() => SendCommand("left"));
        rightButton.onClick.AddListener(() => SendCommand("right"));

        // Connect to MQTT broker
        ConnectToMqtt();
    }

    private void ConnectToMqtt()
    {
        // Check if MQTT host is set
        if (string.IsNullOrEmpty(mqttHost))
        {
            Debug.LogError("MQTT server IP address is not set.");
            return;
        }

        try
        {
            // Initialize MQTT client and connect
            mqttClient = new MqttClient(mqttHost, mqttPort, false, null, null, MqttSslProtocols.None);
            mqttClient.MqttMsgPublished += OnMqttMsgPublished;
            mqttClient.Connect(clientId);

            if (mqttClient.IsConnected)
            {
                Debug.Log("Connected to MQTT broker successfully!");
            }
            else
            {
                Debug.LogError("Failed to connect to MQTT broker.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Exception occurred while connecting to MQTT broker: " + ex.Message);
        }
    }

    private void SendCommand(string direction)
    {
        // Check if MQTT client is connected
        if (mqttClient == null || !mqttClient.IsConnected)
        {
            Debug.LogError("Not connected to any MQTT broker. Please connect first.");
            return;
        }

        // Determine the topic and angle based on the direction
        string topic = "";
        float angle = 0;

        switch (direction)
        {
            case "up":
                servo2Angle = Mathf.Min(servo2Angle + 10, 180); // Increment
                topic = "Servo2";
                angle = servo2Angle;
                break;
            case "down":
                servo2Angle = Mathf.Max(servo2Angle - 10, 0); // Decrement
                topic = "Servo2";
                angle = servo2Angle;
                break;
            case "left":
                servo1Angle = Mathf.Min(servo1Angle + 10, 180); // Increment
                topic = "Servo1";
                angle = servo1Angle;
                break;
            case "right":
                servo1Angle = Mathf.Max(servo1Angle - 10, 0); // Decrement
                topic = "Servo1";
                angle = servo1Angle;
                break;
            default:
                Debug.LogError("Unknown direction: " + direction);
                return;
        }

        // Debug logs to verify angle values
        Debug.Log($"Direction: {direction}, Angle: {angle}");

        AnglePayload payloadObject = new AnglePayload { angle = angle };
        string payload = JsonUtility.ToJson(payloadObject);


        // Debug log to verify the payload
        Debug.Log($"Payload: {payload}");

        // Publish message to MQTT topic
        mqttClient.Publish(topic, System.Text.Encoding.UTF8.GetBytes(payload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
    }

    private void OnMqttMsgPublished(object sender, MqttMsgPublishedEventArgs e)
    {
        Debug.Log($"Message published with ID: {e.MessageId}");
    }
}

[System.Serializable]
public class AnglePayload
{
    public float angle;
}

