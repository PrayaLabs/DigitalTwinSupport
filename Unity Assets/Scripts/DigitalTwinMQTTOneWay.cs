using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Collections.Generic;

public class DigitalTwinMQTTControl : MonoBehaviour
{
    private MqttClient client;

    public string brokerAddress = "91.121.93.94"; // MQTT broker IP
    public int brokerPort = 1883; // MQTT port
    public string[] topics = { "Servo1", "Servo2" }; // Topics to subscribe to
    public float rotationSpeed = 10f; // Speed for rotation

    public GameObject object1; // Object controlled by "Servo1" (Y-axis)
    public GameObject object2; // Object controlled by "Servo2" (Z-axis)
   // public GameObject child1; // Child1 which should lock its Y rotation
    //public GameObject child2; // Child2 which should follow object1

    private float currentYRotation = 0f; // Rotation of object1 on Y-axis
    private float currentZRotation = 0f; // Rotation of object2 on Z-axis
    private const float maxRotation = 180f; // Maximum rotation limit
    private const float minRotation = 0f;   // Minimum rotation limit

    private Queue<System.Action> actionQueue = new Queue<System.Action>();

    void Start()
    {
        // Initialize MQTT client
        client = new MqttClient(brokerAddress, brokerPort, false, null, null, MqttSslProtocols.None);
        client.MqttMsgPublishReceived += OnMessageReceived;

        // Connect to the broker
        string clientId = System.Guid.NewGuid().ToString();
        client.Connect(clientId);

        // Subscribe to topics
        foreach (var topic in topics)
        {
            client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
        }
    }

    void OnMessageReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string message = System.Text.Encoding.UTF8.GetString(e.Message);
        ServoData data = JsonUtility.FromJson<ServoData>(message);

        lock (actionQueue)
        {
            // Check which topic the message was received from
            if (e.Topic == "Servo1")
            {
                // Enqueue rotation change for object1 (Y-axis)
                actionQueue.Enqueue(() => RotateObject1(data.angle));
            }
            else if (e.Topic == "Servo2")
            {
                // Enqueue rotation change for object2 (Z-axis)
                actionQueue.Enqueue(() => RotateObject2(data.angle));
            }
        }
    }

    void Update()
    {
        // Process the actions in the queue (MQTT rotation)
        lock (actionQueue)
        {
            while (actionQueue.Count > 0)
            {
                actionQueue.Dequeue()?.Invoke();
            }
        }

        // Apply smooth rotation to object1 and object2
        if (object1 != null)
        {
           
            // Rotate object1
            object1.transform.rotation = Quaternion.Euler(0, currentYRotation, 0);
            



        }

        if (object2 != null)
        {
            // Rotate object2 based on Z-axis
            object2.transform.rotation = Quaternion.Euler(0, currentYRotation, -currentZRotation);
        }
    }

    // Rotate object1 (Y-axis) based on "Servo1" data
    public void RotateObject1(int angle)
    {
        // Clamp the angle between the minimum and maximum rotation limits
        currentYRotation = Mathf.Clamp(angle, minRotation, maxRotation);
    }

    // Rotate object2 (Z-axis) based on "Servo2" data
    public void RotateObject2(int angle)
    {
        // Clamp the angle between the minimum and maximum rotation limits
        currentZRotation = Mathf.Clamp(angle, minRotation, maxRotation);
    }

    private void OnDestroy()
    {
        if (client != null)
        {
            client.Disconnect();
        }
    }
}

[System.Serializable]
public class ServoData
{
    public int angle; // Angle from the MQTT message
}
