[System.Serializable]
public class Net_OnCreateAccount : NetMsg
{
    public Net_OnCreateAccount()
    {
        OperationCode = NetOperationCode.OnCreateAccount;
    }
 
    public byte Success { get; set; }
    public string Informatoion { get; set;  }
}
