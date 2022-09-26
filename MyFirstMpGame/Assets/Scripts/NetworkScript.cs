using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//https://forum.unity.com/threads/simple-udp-implementation-send-read-via-mono-c.15900/

public class NetworkScript : MonoBehaviour
{
    public int delay;
    public int frameNr;
    public bool isServer = true;
    public PlayerScript[] players = new PlayerScript[2];
    public float speed;
    public TextMeshProUGUI text;
    private UdpConnection connection;
    private int myID;
    private Sendable sendData = new Sendable();
    private bool upKey, downKey, leftKey, rightKey;

    public void ChangeSpeeds(int id)
    {
        //update sendData-object
        sendData.id = id;
        sendData.x = players[id].transform.position.x;
        sendData.y = players[id].transform.position.y;
        sendData.xSpeed = players[id].xSpeed;
        sendData.ySpeed = players[id].ySpeed;
        sendData.frameNr = frameNr;
        string json = JsonUtility.ToJson(sendData);

        connection.Send(json);
    }

    public void Update()
    {
        if (Input.GetKeyDown("w")) upKey = true;
        if (Input.GetKeyUp("w")) upKey = false;
        if (Input.GetKeyDown("s")) downKey = true;
        if (Input.GetKeyUp("s")) downKey = false;
        if (Input.GetKeyDown("a")) leftKey = true;
        if (Input.GetKeyUp("a")) leftKey = false;
        if (Input.GetKeyDown("d")) rightKey = true;
        if (Input.GetKeyUp("d")) rightKey = false;
    }

    private void CheckIncomingMessages()
    {
        string[] o = connection.getMessages();
        if (o.Length > 0)
        {
            foreach (var json in o)
            {
                JsonUtility.FromJsonOverwrite(json, sendData);
                int i = sendData.id;
                players[i].transform.position = new Vector3(sendData.x, sendData.y, 0);
                players[i].xSpeed = sendData.xSpeed;
                players[i].ySpeed = sendData.ySpeed;

                Debug.Log("my Frame: " + frameNr + " Their frame: " + sendData.frameNr);
                if (!isServer && frameNr == 0)
                {
                    frameNr = sendData.frameNr;
                }

                delay = frameNr - sendData.frameNr;
            }
        }
    }

    private void FixedUpdate()
    {
        if (isServer || frameNr > 0)
        {
            frameNr++;
        }

        if (upKey)
        {
            players[myID].ySpeed = speed;
        }
        else if (downKey)
        {
            players[myID].ySpeed = -speed;
        }
        else
        {
            players[myID].ySpeed = 0;
        }

        if (leftKey)
        {
            players[myID].xSpeed = -speed;
        }
        else if (rightKey)
        {
            players[myID].xSpeed = speed;
        }
        else
        {
            players[myID].xSpeed = 0;
        }

        text.text = "Delay: " + delay;
        for (int i = 0; i < players.Length; i++)
        {
            if (delay > 0 && i != myID)
            {
                float yDelay = delay * (players[i].ySpeed * Time.fixedDeltaTime);
                float xDelay = delay * (players[i].xSpeed * Time.fixedDeltaTime);

                players[i].transform.position = Vector3.Lerp(players[i].transform.position, new Vector3(players[i].transform.position.x + xDelay, players[i].transform.position.y + yDelay, 0), .5f);
            }
            else
            {
                players[i].transform.Translate(players[i].xSpeed, players[i].ySpeed, 0);
            }
        }

        CheckIncomingMessages();

        if (players[myID].xSpeed != players[myID].oldXSpeed || players[myID].ySpeed != players[myID].oldYSpeed)
        {
            ChangeSpeeds(myID);
        }

        players[myID].oldYSpeed = players[myID].ySpeed;
        players[myID].oldXSpeed = players[myID].xSpeed;
    }

    private void OnDestroy()
    {
        connection.Stop();
    }

    private void Start()
    {
        string sendIp = "127.0.0.1";

        int sendPort, receivePort;
        if (isServer)
        {
            sendPort = 8881;
            receivePort = 11000;
            myID = 0;
        }
        else
        {
            sendPort = 11000;
            receivePort = 8881;
            myID = 1;
        }
        connection = new UdpConnection();
        connection.StartConnection(sendIp, sendPort, receivePort);
    }
}