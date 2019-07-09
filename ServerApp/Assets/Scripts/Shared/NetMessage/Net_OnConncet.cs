[System.Serializable]
public class Net_OnConnect : NetMsg
{
    public Net_OnConnect()
    {
        OperationCode = NetOperationCode.OnConnect;
    }

    public int ConnID { get; set; }
}