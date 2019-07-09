using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObservableObject : MonoBehaviour, IObservable
{
    protected NetView view;

    public void RegisterObserver(NetView _view)
    {
        view = _view;
    }

    public virtual void SetParams(NetMsg msg)
    {
        throw new System.NotImplementedException();
    }
}
