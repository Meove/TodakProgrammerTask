using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Eflatun.SceneReference;

public class NetworkData : MonoBehaviour
{
    public static NetworkData instance { get; private set; }

    public GameObject MenuPage;
    public GameObject LobbyPage;
    public GameObject StartButton;
    public string playerName;

    [SerializeField] Button createLobbyButton;
    [SerializeField] Button joinLobbyButton;
    [SerializeField] Button startGameButton;
    [SerializeField] Button leaveGameButton;
    [SerializeField] SceneReference gameScene; // Guna array kalau nak lebih scene

    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        createLobbyButton.onClick.AddListener(CreateGame);
        joinLobbyButton.onClick.AddListener(JoinGame);
        startGameButton.onClick.AddListener(StartGame);
        leaveGameButton.onClick.AddListener(LeaveGame);

        ShowMenu();
        StartButton.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    async void CreateGame() // Create game after createLobbyButton clicked
    {
        await Multiplayer.Instance.CreateLobby(); // Call function from multiplayer
        HideMenu(); // Change page

        if (Multiplayer.Instance.currentLobby.HostId == Multiplayer.Instance.PlayerId) // If host...
        {
            StartButton.SetActive(true);
        }
        
    }

    async void JoinGame() // Quick join for client after joinLobbyButton clicked
    {
        await Multiplayer.Instance.QuickJoinLobby(); // Call function from multiplayer

        HideMenu(); // Change page
    }

    async void LeaveGame() // Leave game after leaveGameButton clicked
    {
        await Multiplayer.Instance.LeaveGame(); // Call function from multiplayer
    }

    void StartGame() // Start game after startGameButton clicked
    {
        Loader.LoadNetwork(gameScene); // Load new scene for player in lobby
    }

    public void HideMenu()
    {
        MenuPage.SetActive(false);
        LobbyPage.SetActive(true);
    }

    public void ShowMenu()
    {
        MenuPage.SetActive(true);
        LobbyPage.SetActive(false);
    }
}

///////////////////////////////

// CODE BAWAH CUMA UTK TEST ///

///////////////////////////////
//Method 1 (Most stable version)
/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.EventSystems;

public class LobbyTeamPick : NetworkBehaviour
{
    public static LobbyTeamPick Singleton;

    public string playerName;

    [SerializeField] GameObject buttonChild;
    // Start is called before the first frame update
    void Awake()
    {
        LobbyTeamPick.Singleton = this;
    }

    // Update is called once per frame
    void Update()
    {
        playerName = Multiplayer.Instance.PlayerName;
    }

    public void JoinTeam()
    {
        GameObject button = EventSystem.current.currentSelectedGameObject;

        if (button != null)
        {
            string newPlayerTag = button.transform.parent.gameObject.tag;
            Multiplayer.Instance.PlayerTeam = newPlayerTag;

            buttonChild = button.transform.GetChild(0).gameObject;
            //buttonChild.GetComponent<TextMeshProUGUI>().text = PlayerName;

            UpdateBoxName(button.name, playerName);
        }
        //print(PlayerTeam);
    }

    public void UpdateBoxName(string boxName, string playernameSend = null)
    {
        if (boxName != null) //TEMP
        {
            string S = playernameSend + ": " + boxName;
            SendUpdateBoxServerRpc(S);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SendUpdateBoxServerRpc(string boxName)
    {
        ReceiveUpdateBoxClientRpc(boxName);
    }

    [ClientRpc]
    void ReceiveUpdateBoxClientRpc(string boxName)
    {
        LobbyTeamPick.Singleton.UpdateName(boxName);
    }

    void UpdateName(string boxName)
    {
        SetText(boxName);
    }

    void SetText(string newName)
    {
        buttonChild.GetComponent<TextMeshProUGUI>().text = newName;
    }
}
 */

//Method 2
/*
 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.EventSystems;
using System;



public class LobbyTeamPick : NetworkBehaviour
{
    public static LobbyTeamPick Singleton;

    public string playerName;

    [NonSerialized] GameObject buttonChild;
    // Start is called before the first frame update
    void Awake()
    {
        LobbyTeamPick.Singleton = this;
    }

    // Update is called once per frame
    void Update()
    {
        playerName = Multiplayer.Instance.PlayerName;
    }

    public void JoinTeam()
    {
        GameObject button = EventSystem.current.currentSelectedGameObject;

        if (button != null)
        {
            string newPlayerTag = button.transform.parent.gameObject.tag;
            //PlayerTeam = newPlayerTag;

            buttonChild = button.transform.GetChild(0).gameObject;



            UpdateBoxName(button.name, buttonChild, playerName);
        }
        //print(PlayerTeam);
    }

    public void UpdateBoxName(string boxName, GameObject buttonObject, string playernameSend = null)
    {
        if (boxName != null) //TEMP
        {
            string S = playernameSend + ": " + boxName;
            SendUpdateBoxServerRpc(S, buttonObject);
        }
    }

    void UpdateName(string boxName, GameObject buttonObject)
    {
        SetText(boxName, buttonObject);
    }

    [ServerRpc(RequireOwnership = false)]
    void SendUpdateBoxServerRpc(string boxName, GameObject buttonObject)
    {
        ReceiveUpdateBoxClientRpc(boxName, buttonObject);
    }

    [ClientRpc]
    void ReceiveUpdateBoxClientRpc(string boxName, GameObject buttonObject)
    {
        LobbyTeamPick.Singleton.UpdateName(boxName, buttonObject);
    }

    public void SetText(string newName, GameObject buttonObject)
    {
        buttonObject.GetComponent<TextMeshProUGUI>().text = newName;
    }
}

 */

//Method 3
/*
 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.EventSystems;

public class LobbyTeamPick : NetworkBehaviour
{
    public static LobbyTeamPick Singleton;

    public LobbyUpdateTeam UpdateTeamPrefab;
    public string playerName;

    [SerializeField] GameObject buttonChild;
    // Start is called before the first frame update
    void Awake()
    {
        LobbyTeamPick.Singleton = this;
    }

    // Update is called once per frame
    void Update()
    {
        playerName = Multiplayer.Instance.PlayerName;
    }

    public void JoinTeam()
    {
        GameObject button = EventSystem.current.currentSelectedGameObject;

        if (button != null)
        {
            string newPlayerTag = button.transform.parent.gameObject.tag;
            //PlayerTeam = newPlayerTag;

            buttonChild = button.transform.GetChild(0).gameObject;
            //buttonChild.GetComponent<TextMeshProUGUI>().text = PlayerName;

            UpdateBoxName(button.name, playerName);
        }
        //print(PlayerTeam);
    }

    public void UpdateBoxName(string boxName, string playernameSend = null)
    {
        if (boxName != null) //TEMP
        {
            string S = playernameSend + ": " + boxName;
            SendUpdateBoxServerRpc(S);
        }
    }

    void UpdateName(string boxName)
    {
        LobbyUpdateTeam multiplayer = Instantiate(UpdateTeamPrefab);
        multiplayer.SetText(boxName, buttonChild);
    }

    [ServerRpc(RequireOwnership = false)]
    void SendUpdateBoxServerRpc(string boxName)
    {
        ReceiveUpdateBoxClientRpc(boxName);
    }

    [ClientRpc]
    void ReceiveUpdateBoxClientRpc(string boxName)
    {
        LobbyTeamPick.Singleton.UpdateName(boxName);
    }
}
 */

//Method 4
/*
 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.EventSystems;

public class ReferenceTextMeshPro : NetworkBehaviour
{
    public NetworkVariable<TextMeshProUGUI> RefTextMesh = new NetworkVariable<TextMeshProUGUI>();
}

public class LobbyTeamPick : NetworkBehaviour
{
    public static LobbyTeamPick Singleton;

    public string playerName;

    //[SerializeField] TextMeshProUGUI buttonChildText;
    // Start is called before the first frame update
    void Awake()
    {
        LobbyTeamPick.Singleton = this;
    }

    // Update is called once per frame
    void Update()
    {
        playerName = Multiplayer.Instance.PlayerName;
    }

    public void JoinTeam()
    {
        GameObject button = EventSystem.current.currentSelectedGameObject;

        if (button != null)
        {
            string newPlayerTag = button.transform.parent.gameObject.tag;
            Multiplayer.Instance.PlayerTeam = newPlayerTag;

            GameObject buttonChild = button.transform.GetChild(0).gameObject;

            ReferenceTextServerRpc(buttonChild);

            
        }
    }

    [Rpc(SendTo.Server)]
    public void ReferenceTextServerRpc(NetworkObjectReference textBox)
    {
        NetworkObject newTextBox = textBox;
        UpdateBoxName(newTextBox, playerName);
    }

    public void UpdateBoxName(NetworkObject textBox, string playernameSend = null)
    {
        if (playernameSend != null) //TEMP
        {
            string S = playernameSend;
            SendUpdateBoxServerRpc(textBox, S);
        }
    }

    void UpdateName(NetworkObject textBox, string playernameSend)
    {
        SetText(textBox, playernameSend);
    }

    [ServerRpc(RequireOwnership = false)]
    void SendUpdateBoxServerRpc(NetworkObject textBox, string playernameSend)
    {
        ReceiveUpdateBoxClientRpc(textBox, playernameSend);
    }

    [ClientRpc]
    void ReceiveUpdateBoxClientRpc(NetworkObject textBox, string playernameSend)
    {
        LobbyTeamPick.Singleton.UpdateName(textBox, playernameSend);
    }

    public void SetText(NetworkObject textBox, string newName)
    {
        print(textBox);
        //textBox = newName;
    }
}
 */

//Methood 5
/*
 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.EventSystems;

public class LobbyTeamPick : NetworkBehaviour
{
    public static LobbyTeamPick Singleton;

    public string playerName;

    //[SerializeField] TextMeshProUGUI buttonChildText;
    // Start is called before the first frame update
    void Awake()
    {
        LobbyTeamPick.Singleton = this;
    }

    // Update is called once per frame
    void Update()
    {
        playerName = Multiplayer.Instance.PlayerName;
    }

    public void JoinTeam()
    {
        GameObject button = EventSystem.current.currentSelectedGameObject;

        if (button != null)
        {
            string newPlayerTag = button.transform.parent.gameObject.tag;
            Multiplayer.Instance.PlayerTeam = newPlayerTag;

            var buttonChild = button.transform.GetChild(0).gameObject.GetComponent<NetworkObject>();

            ReferenceTextServerRpc(buttonChild);
        }
    }

    [Rpc(SendTo.Server)]
    public void ReferenceTextServerRpc(NetworkObjectReference textBox)
    {
        if (textBox.TryGet(out NetworkObject targetObject))
        {
            NetworkObject newTextBox = textBox;
            LobbyTeamPick.Singleton.SetText(textBox, playerName);
        }
    }

    public void SetText(NetworkObject textBox, string newName)
    {
        print(textBox);
        //textBox = newName;
    }
}
 */