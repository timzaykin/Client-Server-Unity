#pragma warning disable CS0618 // Тип или член устарел
using System;
using System.Collections.Generic;
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

    private int ClientID;

    public Account self;

    private string token;
    private bool isStarted;

    public int CurrrentClientViewsCount = 0;

    private Dictionary<int, NetView> activeViews;
    // Start is called before the first frame update

    void Start()
    {
        activeViews = new Dictionary<int, NetView>();
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
                Debug.Log(string.Format("Connected to the server. Host:{0}, ConnID:{1}, Channel:{2}", recHostId,connectionId,channelId));
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("Disconnected from server");
                break;
            case NetworkEventType.DataEvent:
                Debug.Log("OnData");
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

            case NetOperationCode.OnConnect:
                OnConnect((Net_OnConnect)msg);
                break;

            case NetOperationCode.OnCreateAccount:
                OnCreateAccount((Net_OnCreateAccount)msg);
                break;

            case NetOperationCode.OnLoginRequest:
                OnLoginRequest((Net_OnLoginRequest)msg);
                break;

            case NetOperationCode.OnAddFollow:
                OnAddFollow((Net_OnAddFollow)msg);
                break;

            case NetOperationCode.OnRequestFollow:
                OnRequestFollow((Net_OnRequestFollow)msg);
                break;
            case NetOperationCode.FollowUpdate:
                Net_FollowUpdate((Net_FollowUpdate)msg);
                break;
            case NetOperationCode.SendMessage:
                OnSendMessage((Net_SendMessage)msg);
                break;

            case NetOperationCode.SendPosition:
                OnSendPosition((Net_SendPosition)msg);
                break;
            case NetOperationCode.Instantiate:
                OnInstantiate((Net_Instantiate)msg);
                break;
            case NetOperationCode.CallRPC:
                OnRPC((Net_CallRPC)msg);
                break;
        }

    }


    private void OnConnect(Net_OnConnect msg)
    {
        Debug.Log("My ID is :" + msg.ConnID);
        ClientID = msg.ConnID;
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
            UnityEngine.SceneManagement.SceneManager.LoadScene("Hub");
            self = new Account();
            self.ActiveConnection = olr.ConnectionId;
            self.Username = olr.Username;
            self.Discriminator = olr.Discriminator;
            token = olr.Token;

        }
    }
    private void OnAddFollow(Net_OnAddFollow oaf) {

        if(oaf.Success == 1) HubScene.Instance.AddFollowTOUi(oaf.Follow);

    }
    private void OnRequestFollow(Net_OnRequestFollow orf)
    {
        foreach (var follow in orf.Follows)
        {
            HubScene.Instance.AddFollowTOUi(follow);
        }
    }
    private void Net_FollowUpdate(Net_FollowUpdate fu)
    {
        HubScene.Instance.UpdateFollow(fu.Follow);
    }

    private void OnSendMessage(Net_SendMessage msg)
    {
        Debug.Log(string.Format("messgae: {0}", msg.Message));
    }

    private void OnSendPosition(Net_SendPosition msg)
    {
        Debug.Log(string.Format("OvnerID:{0},myReliableChannelId:{4},ConnetctionID:{5}, Vector3({1},{2},{3})", msg.OvnerId, msg.X, msg.Y, msg.Z, myReliableChannelId,connectionId));
        activeViews[msg.ViewId].SetMessageToView(msg);
    }

    private void OnInstantiate(Net_Instantiate msg)
    {
       GameObject Instance = Instantiate(Resources.Load(msg.PrefabPath) as GameObject);
        NetView[] views = Instance.GetComponentsInChildren<NetView>();
        for (int i = 0; i < views.Length; i++)
        {
            views[i].ViewId = msg.ViewId[i];
            views[i].OwnerId = msg.OvnerId;
            views[i].ReristerViewId();
        }
        Instance.transform.position = new Vector3(msg.X, msg.Y, msg.Z);
    }
    private void OnRPC(Net_CallRPC msg)
    {

        activeViews[msg.ViewId].PrepareToReciveRPC(msg);
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

    public void SendAddFollow(string usernameOrEmail) {
        Net_AddFollow af = new Net_AddFollow();

        af.Token = token;
        af.UsernameDiscriminatorOrEmail = usernameOrEmail;

        SendServer(af);
    }
    public void SendRemoveFollow(string username)
    {
        Net_RemoveFollow rf = new Net_RemoveFollow();

        rf.Token = token;
        rf.UsernameDiscriminator = username;

        SendServer(rf);
    }
    public void SendRequestFollow() {
        Net_RequestFollow rf = new Net_RequestFollow();

        rf.Token = token;
        SendServer(rf);
    }

    public void SendNetInstantiate(string prefabPath, Vector3 position) {
        Net_Instantiate instMsg = new Net_Instantiate();
        GameObject Instance = Instantiate(Resources.Load(prefabPath) as GameObject, position, Quaternion.identity);
        Instance.GetComponent<NetView>().OwnerId = ClientID;
        instMsg.PrefabPath = prefabPath;
        instMsg.OvnerId = ClientID;
        instMsg.ViewId = Instance.GetComponent<NetView>().GetChildViewId();
        instMsg.X = position.x;
        instMsg.Y = position.y;
        instMsg.Z = position.z;
        SendServer(instMsg);
    }

    public void CallRPC(int viewId, string methodName) {
        Net_CallRPC rpc = new Net_CallRPC();
        rpc.MethodName = methodName;
        rpc.OvnerId = ClientID;
        rpc.ViewId = viewId;
        rpc.parametres = null;
        SendServer(rpc);
    }

    public void CallRPC(int viewId, string methodName, int[] param)
    {
        Net_CallRPC rpc = new Net_CallRPC();
        rpc.MethodName = methodName;
        rpc.OvnerId = ClientID;
        rpc.ViewId = viewId;
        Net_ToInt toInt = new Net_ToInt();
        toInt.param = param;
        rpc.parametres = ParamsSerializer.SerializeParametr(toInt);
        SendServer(rpc);
    }

    public void CallRPC(int viewId, string methodName, float[] param)
    {
        Net_CallRPC rpc = new Net_CallRPC();
        rpc.MethodName = methodName;
        rpc.OvnerId = ClientID;
        rpc.ViewId = viewId;
        Net_ToFloat toFloat = new Net_ToFloat();
        toFloat.param = param;
        rpc.parametres = ParamsSerializer.SerializeParametr(toFloat);
        SendServer(rpc);
    }

    public void CallRPC(int viewId, string methodName, string[] param)
    {
        Net_CallRPC rpc = new Net_CallRPC();
        rpc.MethodName = methodName;
        rpc.OvnerId = ClientID;
        rpc.ViewId = viewId;
        Net_ToString toString = new Net_ToString();
        toString.param = param;
        rpc.parametres = ParamsSerializer.SerializeParametr(toString);
        SendServer(rpc);
    }

    public void CallRPC(int viewId, string methodName, Vector2 param)
    {
        Net_CallRPC rpc = new Net_CallRPC();
        rpc.MethodName = methodName;
        rpc.OvnerId = ClientID;
        rpc.ViewId = viewId;
        Net_ToVector2 toVector2 = new Net_ToVector2();
        toVector2.x = param.x;
        toVector2.y = param.y;
        rpc.parametres = ParamsSerializer.SerializeParametr(toVector2);
        SendServer(rpc);
    }

    public void CallRPC(int viewId, string methodName, Vector3 param)
    {
        Net_CallRPC rpc = new Net_CallRPC();
        rpc.MethodName = methodName;
        rpc.OvnerId = ClientID;
        rpc.ViewId = viewId;
        Net_ToVector3 toVector3 = new Net_ToVector3();
        toVector3.x = param.x;
        toVector3.y = param.y;
        toVector3.z = param.z;
        rpc.parametres = ParamsSerializer.SerializeParametr(toVector3);
        SendServer(rpc);
    }


    #endregion

    #region Getters
    public int GetClientId() {
        return ClientID;
    }
    #endregion

    #region WorkWithViews
    public void RegisterView(int id, NetView view) {
        activeViews.Add(id, view);
    }

    public void UnregisterView(int id)
    {
        activeViews.Remove(id);
    }
    #endregion
}
#pragma warning restore CS0618 // Тип или член устарел