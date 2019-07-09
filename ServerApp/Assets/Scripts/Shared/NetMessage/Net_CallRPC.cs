[System.Serializable]
public class Net_CallRPC : NetMsg
{
    public Net_CallRPC() {
        OperationCode = NetOperationCode.CallRPC;
    }

    public int OvnerId { get; set; }
    public int ViewId { get; set; }

    public string MethodName{ get; set; }

    public byte[] parametres;
}
