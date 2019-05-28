public static class NetOperationCode {

    public const int None = 0;
    public const int CreateAccount = 1;
    public const int LoginRequest = 2;

    public const int OnCreateAccount = 3;
    public const int OnLoginRequest = 4;

    public const int AddFollow = 5;
    public const int RemoveFollow = 6;
    public const int RequestFollow = 7;

    public const int OnAddFollow = 8;
    public const int OnRequestFollow = 9;

    public const int FollowUpdate = 10;

    public const int SendMessage = 28;

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
