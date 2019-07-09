using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformPositionView : ObservableObject
{
    private Vector3 position;
    private void Start()
    {
        position = transform.position;
    }

    public void Update()
    {
        if (transform.hasChanged && position != transform.position && view.isMine) {
            position = transform.position;
            Net_SendPosition msg = new Net_SendPosition();
            msg.OvnerId = NetworkClient.Instance.GetClientId();
            msg.ViewId = view.ViewId;
            msg.X = position.x;
            msg.Y = position.y;
            msg.Z = position.z;
            NetworkClient.Instance.SendServer(msg);
            
        }
    }

    public override void SetParams(NetMsg msg)
    {
        Net_SendPosition sp = (Net_SendPosition)msg;
        if (!view.isMine) { 
            transform.position = new Vector3(sp.X, sp.Y, sp.Z);
        }
    }


}
