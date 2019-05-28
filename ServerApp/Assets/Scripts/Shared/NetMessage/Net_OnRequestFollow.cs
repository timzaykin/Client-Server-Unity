using System.Collections.Generic;

[System.Serializable]
public class Net_OnRequestFollow : NetMsg
{
    public Net_OnRequestFollow()
    {
        OperationCode = NetOperationCode.OnRequestFollow;
    }

    public List<Account> Follows { get; set; }
}
