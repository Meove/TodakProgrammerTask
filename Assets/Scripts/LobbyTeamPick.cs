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
        //playerName = Multiplayer.Instance.PlayerName;
    }


    // Start here, the code flow down until the end of line
    public void JoinTeam() // Function called from Join Team button
    {
        GameObject button = EventSystem.current.currentSelectedGameObject;

        if (button != null)
        {
            string newPlayerTag = button.transform.parent.gameObject.tag;
            Multiplayer.Instance.PlayerTeam = newPlayerTag;

            buttonChild = button.transform.GetChild(0).gameObject;

            playerName = Multiplayer.Instance.PlayerName;
            UpdateBoxName(button.name, playerName);
        }
        //print(PlayerTeam);
    }

    public void UpdateBoxName(string boxName, string playernameSend = null)
    {
        if (boxName != null) //TEMP
        {
            string sendName = playernameSend;
            SendUpdateBoxServerRpc(sendName);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SendUpdateBoxServerRpc(string newName)
    {
        ReceiveUpdateBoxClientRpc(newName);
    }

    [ClientRpc]
    void ReceiveUpdateBoxClientRpc(string newName)
    {
        LobbyTeamPick.Singleton.UpdateName(newName);
    }

    void UpdateName(string newName)
    {
        SetText(newName);
    }

    void SetText(string newName)
    {
        buttonChild.GetComponent<TextMeshProUGUI>().text = newName;
    }
}