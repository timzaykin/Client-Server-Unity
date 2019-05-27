[System.Serializable]
public class Net_LoginRequest : NetMsg
{
    public Net_LoginRequest()
    {
        OperationCode = NetOperationCode.LoginRequest;
    }

    public string UsernameOrEmail { get; set; }
    public string Password { get; set; }



}
