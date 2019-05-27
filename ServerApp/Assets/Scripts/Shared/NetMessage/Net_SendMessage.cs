[System.Serializable]
public class Net_SendMessage : NetMsg
{
    public Net_SendMessage()
    {
        OperationCode = NetOperationCode.SendMessage;
    }

    public string Message { get; set; }

}
