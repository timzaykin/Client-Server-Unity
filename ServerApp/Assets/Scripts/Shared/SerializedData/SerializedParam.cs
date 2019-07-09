public static class NetParamCode {
    public const int None = 0;
    public const int toInt = 1;
    public const int toFloat = 2;
    public const int toString = 3;
    public const int toVector2 = 4;
    public const int toVector3 = 5;
}

[System.Serializable]
public abstract class SerialzedPrarm
{
    public byte ParamCode { get; set; }
    public SerialzedPrarm()
    {
        ParamCode = NetParamCode.None;
    }
}