using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCube : MonoBehaviour
{
    public bool spawned;

    public NetView view;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1) && !spawned) {
            NetworkClient.Instance.SendNetInstantiate("Cube", Vector3.zero);
        }


    }
}
