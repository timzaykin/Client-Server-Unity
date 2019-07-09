using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class ParamsSerializer
{
    private const int BYTE_SIZE = 512;

    public static byte[] SerializeParametr(SerialzedPrarm param) {

        byte[] buffer = new byte[BYTE_SIZE];
        BinaryFormatter foramtter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        foramtter.Serialize(ms, param);

        return buffer;
    }

    public static SerialzedPrarm DeserializeParametr (byte[] data) {

        BinaryFormatter foramtter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(data);
        SerialzedPrarm deserializedData = (SerialzedPrarm)foramtter.Deserialize(ms);
        return deserializedData;
    }
}
