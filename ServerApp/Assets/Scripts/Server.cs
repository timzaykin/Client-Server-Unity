#pragma warning disable CS0618 // Тип или член устарел
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking; 

public class Server : Singleton<Server>
{

    private const int MAX_USERS = 2;
    private const int PORT = 8000;
    private const int WEB_PORT = 8001;
    private const int BYTE_SIZE = 1024;

    private int myReliableChannelId;
    private int hostId;
    private int webHostId;
    private byte error;

    private bool isStarted;

    private Mongo db;

    // Start is called before the first frame update
    void Start()
    {
        db = new Mongo();
        db.Init();

        DontDestroyOnLoad(gameObject);
        Init();
        //db.InsertAccount("Foo", "Bar", "mail");
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMessagePump();
    }

    public void Init() {


        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        myReliableChannelId = config.AddChannel(QosType.Reliable);
        HostTopology topology = new HostTopology(config, MAX_USERS);


        //Server code

        hostId = NetworkTransport.AddHost(topology, PORT, null);
        webHostId = NetworkTransport.AddWebsocketHost(topology, WEB_PORT, null);


        Debug.Log(string.Format("Opening connectionon port {0} and webport {1}", PORT, WEB_PORT));
        isStarted = true;
    }

    public void Shutdown() {
        isStarted = false;
        NetworkTransport.Shutdown();
    }

    private void UpdateMessagePump()
    {
        if (!isStarted) return;

        int recHostId;
        int connectionId;
        int channelId;

        byte[] recBuffer = new byte[BYTE_SIZE];
        int dataSize;


        NetworkEventType type =  NetworkTransport.Receive(out recHostId,out connectionId, out channelId, recBuffer, BYTE_SIZE, out dataSize, out error);


        switch (type) {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log(string.Format("User {0} is connected though {1} !", connectionId, hostId));
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log(string.Format("User {0} is was disconnected!", connectionId));
                break;
            case NetworkEventType.DataEvent:
                BinaryFormatter foramtter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(recBuffer);
                NetMsg msg = (NetMsg)foramtter.Deserialize(ms);
                OnData(connectionId, channelId, recHostId, msg);
                break;
            default:
            case NetworkEventType.BroadcastEvent:
                Debug.Log("unexpected event type");
                break;
        }
    }
    #region OnData
    private void OnData(int connectionId, int channelId, int recHostId, NetMsg msg)
    {
        Debug.Log("Recive a message of type" + msg.OperationCode);

        switch (msg.OperationCode) {
            case NetOperationCode.None:
                Debug.Log("Unexpected NetOperationCode");
                break;
            case NetOperationCode.CreateAccount:
                CreateAccount(connectionId, channelId, recHostId, (Net_CreateAccount)msg);
                break;
            case NetOperationCode.LoginRequest:
                LoginRequest(connectionId, channelId, recHostId, (Net_LoginRequest)msg);
                break;
            case NetOperationCode.SendMessage:
                SendMessage(connectionId, channelId, recHostId, (Net_SendMessage)msg);
                break;
        }
    }

    private void CreateAccount(int connectionId, int channelId, int recHostId, Net_CreateAccount ca)
    {
        Debug.Log(string.Format("{0},{1},{2}", ca.Username, ca.Password, ca.Email));
        Net_OnCreateAccount oca = new Net_OnCreateAccount();

        oca.Success = 0;
        oca.Informatoion = "Account was Created";

        SendClient(recHostId, connectionId, oca);
    }

    private void LoginRequest(int connectionId, int channelId, int recHostId, Net_LoginRequest lr)
    {
        Debug.Log(string.Format("{0},{1}", lr.UsernameOrEmail, lr.Password));

        Net_OnLoginRequest olr = new Net_OnLoginRequest();

        olr.Success = 0;
        olr.Informatoion = "Everething is good";
        olr.Username = "username";
        olr.Discriminator = "0000";
        olr.Token = "TOKEN";

        SendClient(recHostId, connectionId, olr);

    }

    private void SendMessage(int connectionId, int channelId, int recHostId, Net_SendMessage msg)
    {
        Debug.Log(string.Format("{0}", msg.Message));
    }
    #endregion

    #region Send
    public void SendClient(int recHost, int connectionId, NetMsg msg)
    {
        byte[] buffer = new byte[BYTE_SIZE];

        BinaryFormatter foramtter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        foramtter.Serialize(ms, msg);
        if (recHost == 0) { NetworkTransport.Send(hostId, connectionId, myReliableChannelId, buffer, BYTE_SIZE, out error); }
        else{ NetworkTransport.Send(webHostId, connectionId, myReliableChannelId, buffer, BYTE_SIZE, out error);}

    }
    #endregion

}

#pragma warning restore CS0618 // Тип или член устарел