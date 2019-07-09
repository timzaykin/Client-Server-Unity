[System.Serializable]
public class Net_Instantiate : NetMsg
{
    public Net_Instantiate()
    {
        OperationCode = NetOperationCode.Instantiate;
    }

    public string PrefabPath { get; set; }

    public int OvnerId { get; set; }
    public int[] ViewId { get; set; }

    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}
