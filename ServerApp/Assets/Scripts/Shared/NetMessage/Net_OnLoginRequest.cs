[System.Serializable]
public class Net_OnLoginRequest : NetMsg
{
    public Net_OnLoginRequest()
    {
        OperationCode = NetOperationCode.OnLoginRequest;
    }

    public byte Success { get; set; }
    public string Informatoion { get; set; }

    public int ConnectionId { get; set; }
    public string Username { get; set; }
    public string Discriminator { get; set; }
    public string Token { get; set; }
}
