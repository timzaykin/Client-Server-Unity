using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class NetView : MonoBehaviour
{
    [SerializeField]
    public int ViewId { get; set; }
    [SerializeField]
    public int OwnerId { get; set; }
    [SerializeField]
    public ObservableObject[] observables;

    public bool isMine;

    public void Awake()
    {
        ViewId = 0;
        foreach (var observable in observables)
        {
            observable.RegisterObserver(this);
        }

        //if (ViewId == 0 & OwnerId == NetworkClient.Instance.GetConectionId())
        //{
        //    NetworkClient.Instance.CurrrentClientViewsCount++;
        //    ViewId = NetworkClient.Instance.GetConectionId() * 10000 + NetworkClient.Instance.CurrrentClientViewsCount;
        //}
    }


    public void Start()
    {
        if (ViewId == 0 && OwnerId == NetworkClient.Instance.GetClientId())
        {
            NetworkClient.Instance.CurrrentClientViewsCount++;
            ViewId = NetworkClient.Instance.GetClientId() * 10000 + NetworkClient.Instance.CurrrentClientViewsCount;
            ReristerViewId();
        }

    }

    public void SetMessageToView(NetMsg msg) {
        switch (msg.OperationCode)  
        {
            case NetOperationCode.SendPosition:
                foreach (var observable in observables)
                {
                    Debug.Log(observable.GetType().ToString() + " is " + typeof(TransformPositionView));
                    if (observable.GetType() == typeof(TransformPositionView)) {
                        observable.SetParams(msg);
                    }
                }
                break;
            default:
                break;
        }
    }

    public int[] GetChildViewId()
    {
        List<int> ids = new List<int>();

        NetView[] views = GetComponentsInChildren<NetView>();
        foreach (var view in views)
        {
            int id = view.GetViewId();
            ids.Add(id);
        }
        return ids.ToArray();
    }

    public int GetViewId()
    {
        if (ViewId != 0)
        {
            return ViewId;
        }
        else if(OwnerId == NetworkClient.Instance.GetClientId())
        {
            NetworkClient.Instance.CurrrentClientViewsCount++;
            ViewId = NetworkClient.Instance.GetClientId() * 10000 + NetworkClient.Instance.CurrrentClientViewsCount;
            ReristerViewId();
            return ViewId;
        }
        else
        {
            throw new Exception("Undifiend View ID");
        }
    }

    public void ReristerViewId() {
        NetworkClient.Instance.RegisterView(ViewId, this);
        isMine = NetworkClient.Instance.GetClientId() == OwnerId;
    }

    #region RPC_calls
    public void RPC(string methodName) {
        NetworkClient.Instance.CallRPC(ViewId, methodName);
    }

    public void RPC(string methodName,int param)
    {
        NetworkClient.Instance.CallRPC(ViewId, methodName, new int[] {param});
    }

    public void RPC(string methodName, int[] param)
    {
        NetworkClient.Instance.CallRPC(ViewId, methodName, param);
    }

    public void RPC(string methodName, float param)
    {
        NetworkClient.Instance.CallRPC(ViewId, methodName, new float[] {  param });
    }

    public void RPC(string methodName, float[] param)
    {
        NetworkClient.Instance.CallRPC(ViewId, methodName, param);
    }

    public void RPC(string methodName, string param)
    {
        NetworkClient.Instance.CallRPC(ViewId, methodName,new string[] { param });
    }

    public void RPC(string methodName, string[] param)
    {
        NetworkClient.Instance.CallRPC(ViewId, methodName, param);
    }

    public void RPC(string methodName, Vector2 param)
    {
        NetworkClient.Instance.CallRPC(ViewId, methodName, param);
    }

    public void RPC(string methodName, Vector3 param)
    {
        NetworkClient.Instance.CallRPC(ViewId, methodName, param);
    }
    #endregion

    #region RPC_recive
    public void PrepareToReciveRPC(Net_CallRPC msg) {

        if (msg.parametres != null)
        {
            SerialzedPrarm deserializedData = ParamsSerializer.DeserializeParametr(msg.parametres);

            switch (deserializedData.ParamCode)
            {
                case NetParamCode.None:
                    throw new Exception("Undifined param code");

                case NetParamCode.toInt:
                    int[] i = ((Net_ToInt)deserializedData).param;
                    if (i.Count() < 1)
                    {
                        throw new Exception("Reciving param can't be null");
                    }
                    else if (i.Count() == 1)
                    {
                        ReciveRPC(msg.MethodName, new object[] { i[0] });
                    }
                    else {
                        object[] obj = new object[i.Count()];
                        for (int j = 0; j <i.Count() ; j++)
                        {
                            obj[j] = i[j];
                        }
                        ReciveRPC(msg.MethodName, obj);
                    }


                    break;

                case NetParamCode.toFloat:
                    float[] f = ((Net_ToFloat)deserializedData).param;

                    if (f.Count() < 1)
                    {
                        throw new Exception("Reciving param can't be null");
                    }
                    else if (f.Count() == 1)
                    {

                        ReciveRPC(msg.MethodName, new object[] { f[0] });
                    }
                    else
                    {
                        object[] obj = new object[f.Count()];
                        for (int j = 0; j < f.Count(); j++)
                        {
                            obj[j] = f[j];
                        }
                        ReciveRPC(msg.MethodName, obj);
                    }
                    break;
                case NetParamCode.toString:
                    string[] s = ((Net_ToString)deserializedData).param;
                    if (s.Count() < 1)
                    {
                        throw new Exception("Reciving param can't be null");
                    }
                    else if (s.Count() == 1)
                    {
                        ReciveRPC(msg.MethodName, new object[] { s[0] });
                    }
                    else
                    {
                        object[] obj = new object[s.Count()];
                        for (int j = 0; j < s.Count(); j++)
                        {
                            obj[j] = s[j];
                        }
                        ReciveRPC(msg.MethodName, obj);
                    }
                    break;
                case NetParamCode.toVector2:
                    Net_ToVector2 V2 =((Net_ToVector2)deserializedData);
                    ReciveRPC(msg.MethodName, new object[] { new Vector2(V2.x,V2.y)});
                    break;
                case NetParamCode.toVector3:
                    Net_ToVector3 V3 = ((Net_ToVector3)deserializedData);
                    ReciveRPC(msg.MethodName, new object[] { new Vector3(V3.x, V3.y, V3.z) });
                    break;
                default:
                    throw new Exception("Undifined param code");

            }
        }
        else {
            ReciveRPC(msg.MethodName);
        }
    }

    public void ReciveRPC(string methodName) {
        MonoBehaviour[] components = GetComponents<MonoBehaviour>();
        foreach (var component in components)
        {
            
            MethodInfo[] methods = component.GetType().GetMethods();

            foreach (var method in methods)
            {
                if (method.ToString() == "Void " + methodName + "()")
                {
                    object[] attributes = method.GetCustomAttributes(true);
                    if ((from attribute in attributes where attribute is NetRPCAttribute select attribute).Count() > 0) {
                        method.Invoke(component, null);

                    }
                }
            }
            
        }
    }
    public void ReciveRPC(string methodName, object[] param )
    {
        MonoBehaviour[] components = GetComponents<MonoBehaviour>();
        foreach (var component in components)
        {

            MethodInfo[] methods = component.GetType().GetMethods();

            foreach (var method in methods)
            {

                if (method.Name == methodName)
                {
                    object[] attributes = method.GetCustomAttributes(true);
                    if ((from attribute in attributes where attribute is NetRPCAttribute select attribute).Count() > 0)
                    {
                        method.Invoke(component,param);
                    }
                }
            }

        }
    }
    #endregion

    private void OnDestroy()
    {
        NetworkClient.Instance.UnregisterView(ViewId);
    }
}


