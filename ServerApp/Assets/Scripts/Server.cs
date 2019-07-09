#pragma warning disable CS0618 // Тип или член устарел
using System;
using System.Collections.Generic;
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

    private List<int> AllOpenConnections;
    private bool isStarted;

    private Mongo db;

    // Start is called before the first frame update
    void Start()
    {
        db = new Mongo();
        db.Init();
        AllOpenConnections = new List<int>();
        DontDestroyOnLoad(gameObject);
        Init();
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
                AllOpenConnections.Add(connectionId);
                Net_OnConnect oc = new Net_OnConnect();
                oc.ConnID = connectionId;
                SendClient(recHostId, connectionId, oc);
                break;
            case NetworkEventType.DisconnectEvent:
                DisconnectEvent(recHostId, connectionId);
                AllOpenConnections.Remove(connectionId);
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
            case NetOperationCode.AddFollow:
                AddFollow(connectionId, channelId, recHostId, (Net_AddFollow)msg);
                break;
            case NetOperationCode.RemoveFollow:
                RemoveFollow(connectionId, channelId, recHostId, (Net_RemoveFollow)msg);
                break;
            case NetOperationCode.RequestFollow:
                RequestFollow(connectionId, channelId, recHostId, (Net_RequestFollow)msg);
                break;

            case NetOperationCode.SendMessage:
                SendMessage(connectionId, channelId, recHostId, (Net_SendMessage)msg);
                break;

            case NetOperationCode.SendPosition:
                SendTransform(connectionId, channelId, recHostId, (Net_SendPosition)msg);
                break;
            case NetOperationCode.Instantiate:
                SendInstantiate(connectionId, channelId, recHostId, (Net_Instantiate)msg);
                break;
            case NetOperationCode.CallRPC:
                SendRPC(connectionId, channelId, recHostId, (Net_CallRPC)msg);
                break;

        }
    }



    private void DisconnectEvent(int recHostId,int connectionId) {
        Debug.Log(string.Format("User {0} is was disconnected!", connectionId));
        //get a reference to connected account
        Model_Account account = db.FindAccountByConnectionId(connectionId);

        //just making sure he was indeed authenticated
        if (account == null) return;

        db.UpdateAccountAfterDisconnection(account.Email);

        //Prepare and send our update message

        Net_FollowUpdate fu = new Net_FollowUpdate();
        Model_Account updatedAccount = db.FindAccountByEmail(account.Email);
        fu.Follow = updatedAccount.GetAccount();

        foreach (var f in db.FindeAllFollowBy(account.Email)) {

            if (f.ActiveConnection == 0) continue;
            SendClient(recHostId, f.ActiveConnection, fu);
        }
    }

    private void RequestFollow(int connectionId, int channelId, int recHostId, Net_RequestFollow msg)
    {
        Net_OnRequestFollow orf = new Net_OnRequestFollow();

        orf.Follows = db.FindeAllFollowFrom(msg.Token);
        SendClient(recHostId, connectionId, orf);
    }
    private void RemoveFollow(int connectionId, int channelId, int recHostId, Net_RemoveFollow msg)
    {
        db.RemoveFollow(msg.Token, msg.UsernameDiscriminator);
    }
    private void AddFollow(int connectionId, int channelId, int recHostId, Net_AddFollow msg)
    {
        Net_OnAddFollow oaf = new Net_OnAddFollow();

        if (db.InsertFollow(msg.Token, msg.UsernameDiscriminatorOrEmail)) {

            oaf.Success = 1;

            if (Utility.IsEmail(msg.UsernameDiscriminatorOrEmail))
            {
                Debug.Log("add follow mail");
                oaf.Follow = db.FindAccountByEmail(msg.UsernameDiscriminatorOrEmail).GetAccount();
            }
            else
            {
                string[] data = msg.UsernameDiscriminatorOrEmail.Split('#');
                if (data[1] == null) return;
                Debug.Log("add follow descriminator");
                oaf.Follow = db.FindAccountByUsernameAndDiscriminator(data[0], data[1]).GetAccount();
            }
        }
        SendClient(recHostId, connectionId, oaf);
    }

    private void CreateAccount(int connectionId, int channelId, int recHostId, Net_CreateAccount ca)
    {
        Net_OnCreateAccount oca = new Net_OnCreateAccount();
        if (db.InsertAccount(ca.Username, ca.Password, ca.Email)){
            oca.Success = 1;
            oca.Informatoion = "Account was Created";
        }
        else
        {
            oca.Success = 0;
            oca.Informatoion = "There was an error creating the account!";

        }

        SendClient(recHostId, connectionId, oca);
    }

    private void LoginRequest(int connectionId, int channelId, int recHostId, Net_LoginRequest lr)
    {
        string randomToken = Utility.GenerateRandom(256);

        Model_Account account = db.LoginAccount(lr.UsernameOrEmail, lr.Password, connectionId, randomToken);
        Net_OnLoginRequest olr = new Net_OnLoginRequest();

        if (account != null)
        {
            olr.Success = 1;
            olr.Informatoion = "You've been logged in as " + account.Username;
            olr.Username = account.Username;
            olr.Discriminator = account.Discriminator;
            olr.Token = randomToken;
            olr.ConnectionId = connectionId;


            //Prepare and send our update message

            Net_FollowUpdate fu = new Net_FollowUpdate();
            fu.Follow = account.GetAccount();

            foreach (var f in db.FindeAllFollowBy(account.Email))
            {

                if (f.ActiveConnection == 0) continue;
                SendClient(recHostId, f.ActiveConnection, fu);
            }

        }
        else {
            olr.Success = 0;
            olr.Informatoion = "Incorrect login or username";
        }




        SendClient(recHostId, connectionId, olr);

    }

    //тут код сгенерированный мной
    private void SendMessage(int connectionId, int channelId, int recHostId, Net_SendMessage msg)
    {
        Debug.Log(string.Format("{0}", msg.Message));
    }

    private void SendTransform(int connectionId, int channelId, int recHostId, Net_SendPosition msg) {

        foreach (var connection in AllOpenConnections)
        {
            Debug.Log("Instance: target - " + connection + ", owner - " + connectionId);
            if (connection != connectionId) SendClient(recHostId, connection, msg);
        }        
    }

    private void SendInstantiate(int connectionId, int channelId, int recHostId, Net_Instantiate msg) {
        foreach (var connection in AllOpenConnections)
        {
            Debug.Log("Instance: target - " + connection + ", owner - " + connectionId);
            if(connection != connectionId) SendClient(recHostId, connection, msg);
        }
    }

    private void SendRPC(int connectionId, int channelId, int recHostId, Net_CallRPC msg)
    {
        foreach (var connection in AllOpenConnections)
        {
            Debug.Log("Instance: target - " + connection + ", owner - " + connectionId);
            SendClient(recHostId, connection, msg);
        }
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

        Debug.Log("Send to client " + msg.GetType().ToString());
    }
    #endregion

}

#pragma warning restore CS0618 // Тип или член устарел