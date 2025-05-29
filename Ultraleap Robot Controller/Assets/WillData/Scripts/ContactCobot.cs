using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine.UI;

public class ContactCobot : MonoBehaviour
{
    public TMP_InputField IPLabel;
    public TMP_Text portLabel;
    public TextMeshProUGUI outputLabel;
    public Button enableButton;

    public string host = "10.225.240.187";
    private int port = 0000;

    public GetHandPos handData;
    public Cobot3DIK cobotIK;
    public Transform target;

    public Transform J1;
    public Transform J2;
    public Transform J3;
    public Transform J4;

    private TcpClient client;
    private string previousData;

    private bool started;

    private bool cobotEnabled = true;

    private string[] warnings = { "", "", "", "", "" };

    private int coolFrames = 250;
    private int currentCoolFrames = 0;


    void Start()
    {
        previousData = (handData.GetHandPose()) + ";" +
                J1.rotation.y.ToString() + "," +
                J2.rotation.x.ToString() + "," +
                J3.rotation.x.ToString() + "," +
                J4.rotation.x.ToString();

        OpenConnection();
    }

    void Update()
    {
        currentCoolFrames++; 
    }


    private void OpenConnection()
    {
        Debug.Log("Trying connection...");

        host = IPLabel.text;




        if (IPLabel.text != "")
        {
            portLabel.transform.parent.gameObject.SetActive(true);

            if (portLabel.text == "5000")
            {
                port = 5000;
                warnings[1] = "";
            }
            else if (portLabel.text == "8080")
            {
                port = 8080;
                warnings[1] = "";
            }
            else
            {
                warnings[1] = "-> Please select a valid port!\n";
            }
        }
        else
        {
            portLabel.transform.parent.gameObject.SetActive(false);
            warnings[0] = "-> IP is null\n";
            warnings[1] = "";
        }


        new Thread(() =>
        {
            try
            {
                client = new TcpClient(host, port);

                client.SendTimeout = 1000;
                client.ReceiveTimeout = 1000;
                //error = "";

                warnings[0] = "";
            }
            catch
            {
                warnings[0] = "-> Connection refused! (Did you start the server, is the IP and port correct?)\n";
                //error = e.Message;
                new WaitForSeconds(1);
            }
        }).Start(); // Start the Thread
    }

    public void SendData()
    {
        if (client != null)
        {
            NetworkStream stream = null;

            try
            {
                stream = client.GetStream();
                warnings[2] = "Connected!\n";
            }
            catch
            {
                warnings[2] = "-> Disconnected!\n";
            }

            if (stream == null)
            {
                warnings[2] = "-> Disconnected!\n";
                return;
            }
            
            string data = handData.GetHandPose() + ";" +
                Mathf.Clamp(Mathf.Round(cobotIK.GetJoint1Angle()), -150, 150).ToString() + "," +
                Mathf.Round(cobotIK.J2Angle).ToString() + "," +
                Mathf.Round(cobotIK.J3Angle).ToString() + "," +
                Mathf.Round(cobotIK.J4Angle).ToString() + ", 0";

            byte[] responseData = new byte[1024];
            StringBuilder responseMessage = new StringBuilder();

            while (stream.DataAvailable)
            {
                int bytesRead = stream.Read(responseData, 0, responseData.Length);
                responseMessage.Append(Encoding.ASCII.GetString(responseData, 0, bytesRead));

                //Debug.Log(responseMessage);
            }

            if (responseMessage != null)
            {
                if (data != previousData && cobotEnabled == true && currentCoolFrames >= coolFrames)
                {
                    byte[] encodedData = Encoding.ASCII.GetBytes(data);
                    stream.Write(encodedData, 0, encodedData.Length);
                    Debug.Log("Message sent to server: " + data);

                    previousData = data;
                    coolFrames = 0;
                }
            }
            else
            {
                OpenConnection();
            }

            responseMessage = null;
        }
        else
        {
            warnings[2] = "-> Disconnected!\n";
            OpenConnection();
        }

        outputLabel.text = warnings[0] + warnings[1] + warnings[2] + warnings[3];
    }


    public void ToggleEnable()
    {
        cobotEnabled = !cobotEnabled;

        if (cobotEnabled)
        {
            enableButton.GetComponentInChildren<TextMeshProUGUI>().text = "Disable";
            enableButton.GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
        }
        else
        {
            enableButton.GetComponentInChildren<TextMeshProUGUI>().text = "Enable";
            enableButton.GetComponentInChildren<TextMeshProUGUI>().color = Color.green;
        }
    }

    private void OnApplicationQuit()
    {
        if (client != null)
        {
            client.Close();
        }
    }
}