using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://forum.unity.com/threads/simple-udp-implementation-send-read-via-mono-c.15900/

public class NetworkScript : MonoBehaviour {
    private UdpConnection connection;
    public bool isServer = true;
    int myID;

    bool upKey, downKey, leftKey, rightKey;


    public PlayerScript[] players = new PlayerScript[2];
    Sendable sendData = new Sendable();

    void Start() {
        
        string sendIp = "127.0.0.1";
        
        int sendPort, receivePort;
        if (isServer) {
            sendPort = 8881;
            receivePort = 11000;
            myID = 0;
        } else {
            sendPort = 11000;
            receivePort = 8881;
            myID = 1;
        }

        connection = new UdpConnection();
        connection.StartConnection(sendIp, sendPort, receivePort);
    }
 
    void FixedUpdate() {
        //Check input...
        if (upKey) {
            players[myID].transform.Translate(0, .1f, 0);
            UpdatePositions(myID);
        }
        if (downKey)
        {
            players[myID].transform.Translate(0, -.1f, 0);
            UpdatePositions(myID);
        }
        if (leftKey)
        {
            players[myID].transform.Translate(-.1f, 0, 0);
            UpdatePositions(myID);
        }
        if (rightKey)
        {
            players[myID].transform.Translate(.1f, 0, 0);
            UpdatePositions(myID);
        }

        //network stuff:
        CheckIncomingMessages();
            
    }

    public void Update()
    {
        //handling keyboard (in Update, because FixedUpdate isnt meant for that(!))
        if (Input.GetKeyDown("w")) upKey = true;       
        if (Input.GetKeyUp("w")) upKey = false;
        if (Input.GetKeyDown("s")) downKey = true;
        if (Input.GetKeyUp("s")) downKey = false;
        if (Input.GetKeyDown("a")) leftKey = true;
        if (Input.GetKeyUp("a")) leftKey = false;
        if (Input.GetKeyDown("d")) rightKey = true;
        if (Input.GetKeyUp("d")) rightKey = false;
    }

    void CheckIncomingMessages()
    {
        //Do the networkStuff:
        string[] o = connection.getMessages();
        if (o.Length > 0)
        {
            foreach (var json in o)
            {
                JsonUtility.FromJsonOverwrite(json, sendData);
                //now, check its id..
                int i = sendData.id;
                //..and update the right player_entity:
                players[i].transform.position = new Vector3(sendData.x, sendData.y, 0);
            }
        }

    }
    public void UpdatePositions(int id)
    {
        //update sendData-object
        sendData.id = id;
        sendData.x = players[id].transform.position.x;
        sendData.y = players[id].transform.position.y;
        
        string json = JsonUtility.ToJson(sendData); //Convert to String
        Debug.Log(json);
        
        connection.Send(json); //send the string

    }
 
    void OnDestroy() {
        connection.Stop();
    }
}

