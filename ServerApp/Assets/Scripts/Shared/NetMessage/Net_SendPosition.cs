[System.Serializable]
public class Net_SendPosition : NetMsg
{
    public Net_SendPosition()
    {
        OperationCode = NetOperationCode.SendPosition;
    }

    public int OvnerId { get; set; }
    public int ViewId { get; set; }

    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

}