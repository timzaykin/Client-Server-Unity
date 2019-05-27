using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyScene : Singleton<LobbyScene>
{

    public void OnClicCreateAccount() {

        DisableInputs();

        string username = GameObject.Find("CreateUsername").GetComponent<TMP_InputField>().text;
        string password = GameObject.Find("CreatePassword").GetComponent<TMP_InputField>().text;
        string email = GameObject.Find("CreateEmail").GetComponent<TMP_InputField>().text;

        NetworkClient.Instance.SendCreateAccount(username, password, email);
    }

    public void OnClickLoginRequest() {

        DisableInputs();

        string usernameOrEmail = GameObject.Find("LoginUsernameOrEmail").GetComponent<TMP_InputField>().text;
        string password = GameObject.Find("LoginPassword").GetComponent<TMP_InputField>().text;

        NetworkClient.Instance.SendLoginRequest(usernameOrEmail, password);
    }

    public void ChangeWelcomeMessage(string msg) {
        GameObject.Find("WelcomeMessageText").GetComponent<TextMeshProUGUI>().text = msg;
    }

    public void ChangeAuthenticationMessage(string msg)
    {
        Debug.Log(msg);
        GameObject.Find("AuthenticationMessageText").GetComponent<TextMeshProUGUI>().text = msg;
    }

    public void EnableInputs() {
        GameObject.Find("Canvas").GetComponent<CanvasGroup>().interactable = true;
    }

    public void DisableInputs()
    {
        GameObject.Find("Canvas").GetComponent<CanvasGroup>().interactable = false;
    }
}
