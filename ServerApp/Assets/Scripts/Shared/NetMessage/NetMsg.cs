public static class NetOperationCode {

    public const int None = 0;
    public const int OnConnect = 1;
    public const int CreateAccount = 2;
    public const int LoginRequest = 3;

    public const int OnCreateAccount = 4;
    public const int OnLoginRequest = 5;

    public const int AddFollow = 6;
    public const int RemoveFollow = 7;
    public const int RequestFollow = 8;

    public const int OnAddFollow = 9;
    public const int OnRequestFollow = 10;

    public const int FollowUpdate = 11;


    public const int SendMessage = 28;


    public const int Instantiate = 30;
    public const int SendPosition = 31;
    public const int SendRotation = 32;
    public const int SendScale = 33;

    public const int SendAnimation = 34;
    public const int SendRigitbody = 35;

    public const int CallRPC = 40;

}

[System.Serializable]
public abstract class NetMsg
{
    public byte OperationCode { get; set; }
    public NetMsg()
    {
        OperationCode = NetOperationCode.None;
    }
}
