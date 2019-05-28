[System.Serializable]
public class Net_FollowUpdate : NetMsg
{
    public Net_FollowUpdate()
    {
        OperationCode = NetOperationCode.FollowUpdate;
    }

    public byte Success { get; set; }
    public Account Follow { get; set; }

}
