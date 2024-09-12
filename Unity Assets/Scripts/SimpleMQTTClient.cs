using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

public class SimpleMQTTClient : MonoBehaviour
{
    private MqttClient client;

    public string brokerAddress = "91.121.93.94"; // Localhost IP
    public int brokerPort = 1883; // Default MQTT port
    public string[] topics = { "Servo1" ,"Servo2"}; // List of topics to subscribe to

    private void Start()
    {
        // Initialize MQTT client
        client = new MqttClient(brokerAddress, brokerPort, false, null, null, MqttSslProtocols.None);

        // Register to message received event
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

    private void OnMessageReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string message = System.Text.Encoding.UTF8.GetString(e.Message);
        Debug.Log($"Message received on topic {e.Topic}: {message}");
    }

    private void OnDestroy()
    {
        if (client != null)
        {
            client.Disconnect();
        }
    }
}
