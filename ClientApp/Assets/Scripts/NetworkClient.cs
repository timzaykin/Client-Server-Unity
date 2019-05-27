#pragma warning disable CS0618 // Тип или член устарел
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkClient : Singleton<NetworkClient>
{

    private const int MAX_USERS = 2;
    private const int PORT = 8000;
    private const int WEB_PORT = 8001;
    private const int BYTE_SIZE = 1024;
    private const string SERVER_IP = "127.0.0.1";

    private int myReliableChannelId;
    private int connectionId; 
    private int hostId;
    private int webHostId;
    private byte error;


    private bool isStarted;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        Init();
    }

    void Update()
    {
        UpdateMessagePump();
    }

    public void Init()
    {


        NetworkTransport.Init();

        ConnectionConfig config = new ConnectionConfig();

        myReliableChannelId = config.AddChannel(QosType.Reliable);
        HostTopology topology = new HostTopology(config, MAX_USERS);


        hostId = NetworkTransport.AddHost(topology, 0);

#if UNITY_WEBGL && !UNITY_EDITOR
        connectionId = NetworkTransport.Connect(hostId, SERVER_IP, WEB_PORT, 0, out error);
#else
        connectionId = NetworkTransport.Connect(hostId, SERVER_IP, PORT, 0, out error);
#endif



        Debug.Log(string.Format("Attempting to connect on {0}",SERVER_IP));
        isStarted = true;
    }

    public void Shutdown()
    {
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

        NetworkEventType type = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, BYTE_SIZE, out dataSize, out error);

        switch (type)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log("Connected to the server");
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("Disconnected from server");
                break;
            case NetworkEventType.DataEvent:
                Debug.Log("Data");
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
    #region onData

    private void OnData(int connectionId , int channelId, int recHostId, NetMsg msg) {

        switch (msg.OperationCode)
        {
            case NetOperationCode.None:
                Debug.Log("Unexpected <NetOperationCode>");
                break;

            case NetOperationCode.OnCreateAccount:
                OnCreateAccount((Net_OnCreateAccount)msg);
                break;

            case NetOperationCode.OnLoginRequest:
                OnLoginRequest((Net_OnLoginRequest)msg);
                break;
        }

    }

    private void OnCreateAccount(Net_OnCreateAccount oca) {
        LobbyScene.Instance.EnableInputs();
        LobbyScene.Instance.ChangeAuthenticationMessage(oca.Informatoion);
    }

    private void OnLoginRequest(Net_OnLoginRequest olr)
    {
        LobbyScene.Instance.ChangeAuthenticationMessage(olr.Informatoion);


        if (olr.Success != 1) { 
            LobbyScene.Instance.EnableInputs();
        }
        else{
            //seccessfull Login
        }
    }

    
    #endregion

    #region Send
    public void SendServer( NetMsg msg) {
        byte[] buffer = new byte[BYTE_SIZE];

        BinaryFormatter foramtter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        foramtter.Serialize(ms, msg); 

        NetworkTransport.Send(hostId, connectionId, myReliableChannelId, buffer, BYTE_SIZE, out error); 
    }

    public void SendCreateAccount(string username, string password, string email) {


        if (!Utility.IsUsername(username)) {
            LobbyScene.Instance.ChangeAuthenticationMessage("Username is invalid");
            LobbyScene.Instance.EnableInputs();
            return;
        }

        if(!Utility.IsEmail(email))
        {
            LobbyScene.Instance.ChangeAuthenticationMessage("Email is invalid");
            LobbyScene.Instance.EnableInputs();
            return;
        }

        if (password == null || password =="")
        {
            LobbyScene.Instance.ChangeAuthenticationMessage("Password is empty");
            LobbyScene.Instance.EnableInputs();
            return;
        }

        Net_CreateAccount ca = new Net_CreateAccount();
        ca.Username = username;
        ca.Password = Utility.Sha256FromSting(password);
        ca.Email = email;
        SendServer(ca);

    }
    public void SendLoginRequest(string usernameOrEmail, string password)
    {


        if (!Utility.IsUsernameAndDiscriminator(usernameOrEmail) && !Utility.IsEmail(usernameOrEmail))
        {
            LobbyScene.Instance.ChangeAuthenticationMessage("Email or Username#Discriminator is invalid");
            LobbyScene.Instance.EnableInputs();
            return;
        }


        if (password == null || password == "")
        {
            LobbyScene.Instance.ChangeAuthenticationMessage("Password is empty");
            LobbyScene.Instance.EnableInputs();
            return;
        }

        Net_LoginRequest lr = new Net_LoginRequest();
        lr.UsernameOrEmail = usernameOrEmail;
        lr.Password = Utility.Sha256FromSting(password);

        LobbyScene.Instance.ChangeAuthenticationMessage("Sending request...");
        SendServer(lr);
    }
    #endregion
}
#pragma warning restore CS0618 // Тип или член устарел