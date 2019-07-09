using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToTestScene : MonoBehaviour
{
    public void ToScene() {
        SceneManager.LoadScene("TestNetwork");
    }
}
