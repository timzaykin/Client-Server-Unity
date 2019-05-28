[System.Serializable]
public class Net_RequestFollow : NetMsg
{
    public Net_RequestFollow()
    {
        OperationCode = NetOperationCode.RequestFollow;
    }

    public string Token { get; set; }


}
