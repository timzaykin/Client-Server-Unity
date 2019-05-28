[System.Serializable]
public class Net_OnAddFollow : NetMsg
{
    public Net_OnAddFollow()
    {
        OperationCode = NetOperationCode.OnAddFollow;
    }

    public byte Success { get; set; }
    public Account Follow{ get; set; }

}
