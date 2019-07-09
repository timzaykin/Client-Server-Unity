using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCube : MonoBehaviour
{
    public float speed;
    NetView view;
    public void Start()
    {
        view = GetComponent<NetView>();
    }

    void Update()
    {
        if (view.isMine) { 
            if (Input.GetAxis("Vertical") != 0) {
                transform.position += new Vector3(0,0,Input.GetAxis("Vertical") * speed);
            }

            if (Input.GetAxis("Horizontal") != 0)
            {
                transform.position += new Vector3(Input.GetAxis("Horizontal")* speed, 0, 0);
            }
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            view.RPC("TestRPC",54f);
        }
    }



    [NetRPC]
    public void TestRPC(float i)
    {
        Debug.Log("DONE!!!! Result : "+i);
    }
}
