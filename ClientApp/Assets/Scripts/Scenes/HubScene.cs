using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HubScene : MonoBehaviour
{
    public static HubScene Instance { get; set; }

    [SerializeField]
    private TextMeshProUGUI selfInformation;
    [SerializeField]
    private TMP_InputField addFollowInput;
    [SerializeField] private GameObject followPrefab;
    [SerializeField] private Transform followContainer;


    private Dictionary<string, GameObject> uiFollows = new Dictionary<string, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        selfInformation.text = NetworkClient.Instance.self.Username + "#" + NetworkClient.Instance.self.Discriminator;
        NetworkClient.Instance.SendRequestFollow();
    }

    public void AddFollowTOUi(Account follow) {
        GameObject followItem = Instantiate(followPrefab, followContainer);
        followItem.GetComponentInChildren<TextMeshProUGUI>().text = follow.Username + "#" + follow.Discriminator;
        followItem.transform.GetChild(1).GetComponent<Image>().color = (follow.Status != 0) ? Color.green : Color.gray;
        followItem.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate { Destroy(followItem); });
        followItem.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate { OnClickRemoveFollow(follow.Username, follow.Discriminator);});

        uiFollows.Add(follow.Username + "#" + follow.Discriminator, followItem);
    }
    public void UpdateFollow(Account follow) {
        uiFollows[follow.Username + "#" + follow.Discriminator].transform.GetChild(1).GetComponent<Image>().color = (follow.Status != 0) ? Color.green : Color.gray;
    } 

    #region Button
    public void OnClickAddFollow() {

        string usernameDiscriminator = addFollowInput.text;

        if (!Utility.IsUsernameAndDiscriminator(usernameDiscriminator) && !Utility.IsEmail(usernameDiscriminator)) {
            Debug.Log("InvalidFormat");
            return;
        }

        NetworkClient.Instance.SendAddFollow(usernameDiscriminator);
    }
    public void OnClickRemoveFollow(string username, string discriminator) {

        NetworkClient.Instance.SendRemoveFollow(username+"#"+discriminator);
        uiFollows.Remove(username + "#" + discriminator);
    }
    #endregion
}
