[System.Serializable]
public class Net_AddFollow : NetMsg
{
    public Net_AddFollow()
    {
        OperationCode = NetOperationCode.AddFollow;
    }

    public string Token { get; set; }
    public string UsernameDiscriminatorOrEmail { get; set; }
}
