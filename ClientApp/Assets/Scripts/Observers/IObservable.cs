using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObservable
{
    void SetParams(NetMsg msg);
    void RegisterObserver(NetView view);
}
