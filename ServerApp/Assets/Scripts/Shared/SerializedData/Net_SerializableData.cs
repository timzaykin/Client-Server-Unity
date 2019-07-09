[System.Serializable]
public class Net_ToInt : SerialzedPrarm
{
    public Net_ToInt()
    {
        ParamCode = NetParamCode.toInt;
    }
    public int[] param;
}

[System.Serializable]
public class Net_ToFloat : SerialzedPrarm
{
    public Net_ToFloat()
    {
        ParamCode = NetParamCode.toFloat;
    }
    public float[] param;
}

[System.Serializable]
public class Net_ToString : SerialzedPrarm
{
    public Net_ToString()
    {
        ParamCode = NetParamCode.toString;
    }
    public string[] param;
}

[System.Serializable]
public class Net_ToVector2 : SerialzedPrarm
{
    public Net_ToVector2()
    {
        ParamCode = NetParamCode.toVector2;
    }
    public float x;
    public float y;
}

[System.Serializable]
public class Net_ToVector3 : SerialzedPrarm
{
    public Net_ToVector3()
    {
        ParamCode = NetParamCode.toVector3;
    }
    public float x;
    public float y;
    public float z;
}
