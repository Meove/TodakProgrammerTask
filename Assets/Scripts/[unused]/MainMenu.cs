using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Cinemachine.CinemachineTriggerAction.ActionSettings;
using Unity.Netcode;

public class MainMenu : MonoBehaviour
{
    public GameObject MenuPage;
    public GameObject LobbyPage;
    public GameObject StartButton;
    public float hearbeatTimer;
    public float lobbyUpdateTimer;
    public NetworkData NetworkData;

    private Lobby hostLobby;
    private Lobby joinedLobby;
    private string playerName;
    // Start is called before the first frame update
    private void Awake()
    {
        //AuthenticatedPlayer();
        Debug.developerConsoleVisible = true;
    }

    void Start()
    {
        MenuPage.SetActive(true);
        LobbyPage.SetActive(false);
        StartButton.SetActive(false);
    }

    private void Instance_OnStartGame(object sender, EventArgs e)
    {
        print(sender);
        throw new NotImplementedException();
    }

    // Update is called once per frame
    void Update()
    {
        LobbyHeartbeat();
        LobbyPollForUpdate();
    }

     // Menu
    public void PlayButton()
    {
        
    }

    public void QuickButton()
    {
        
    }

    // Lobby
    public void JoinTeam()
    {
        GameObject button = EventSystem.current.currentSelectedGameObject;

        foreach (Player player in joinedLobby.Players)
        {
            if (player.Data["PlayerName"].Value == playerName && button.tag != "Occupied")
            {
                string newPlayerTag = button.transform.parent.gameObject.tag; 
                UpdatePlayerNameAndTeam(player.Data["PlayerName"].Value, newPlayerTag);

                GameObject buttonChild = button.transform.GetChild(0).gameObject;
                buttonChild.GetComponent<TextMeshProUGUI>().text = player.Data["PlayerName"].Value;

                button.tag = "Occupied";
            }
        }
        PrintPlayerInLobby();
    }

    public async void LeaveButton()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);

            PrintPlayerInLobby();

            MenuPage.SetActive(true);
            LobbyPage.SetActive(false);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public void StartGame() 
    {
        UpdateStartGame("YES");
    }

    // Function
    public async void AuthenticatedPlayer()
    {
        //await UnityServices.InitializeAsync();

        //AuthenticationService.Instance.SignedIn += () =>
        //{
        //    print(AuthenticationService.Instance.PlayerId);
        //};

        var options = new InitializationOptions();
        var profile = Guid.NewGuid().ToString().Substring(0, 8);
        options.SetProfile(profile);

        await UnityServices.InitializeAsync(options);

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        playerName = "Player_" + UnityEngine.Random.Range(10000, 50000);
        NetworkData.playerName = playerName;
        print(playerName);

    }

    public async void CreateLobby()
    {
        try
        {
            string lobbyName = "Lobby test";
            int maxPlayers = 10;

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "Deathmatch", DataObject.IndexOptions.S1) },
                    { "Map", new DataObject(DataObject.VisibilityOptions.Public, "BloodGulch", DataObject.IndexOptions.S2) },
                    { "START", new DataObject(DataObject.VisibilityOptions.Public, "NO", DataObject.IndexOptions.S3)},
                    { "RELAY_CODE", new DataObject(DataObject.VisibilityOptions.Public, "STRING", DataObject.IndexOptions.S4)}
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            hostLobby = lobby;
            joinedLobby = hostLobby;

            StartButton.SetActive(true);
            print("Lobby name: " + lobby.Name + " | Gamemode: " + lobby.Data["GameMode"].Value + " | Map: " + lobby.Data["Map"].Value);
            PrintPlayerInLobby();

            //Relay
            //NetworkData.CreateRelay();
            //string relaycode = NetworkData.UpdateJoinCode();
            //lobby = await LobbyService.Instance.UpdatePlayerAsync(hostLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            //{
            //    Data = new Dictionary<string, PlayerDataObject>
            //    {
            //        {"RELAY_CODE", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, relaycode) }
            //    }
            //});

            MenuPage.SetActive(false);
            LobbyPage.SetActive(true);

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void QuickJoin()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
            {
                new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                new QueryFilter(QueryFilter.FieldOptions.S1, "Deathmatch", QueryFilter.OpOptions.EQ),
            },
                Order = new List<QueryOrder>
            {
                new QueryOrder(false, QueryOrder.FieldOptions.Created)
            }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            print("Lobby found: " + queryResponse.Results.Count);

            foreach (Lobby lobby in queryResponse.Results)
            {
                print("Lobby name: " + lobby.Name + " | " + lobby.Data["GameMode"].Value + " | " + lobby.Data["Map"].Value);
            }

            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions
            {
                Player = GetPlayer()
            };

            Lobby quickJoinLobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            joinedLobby = quickJoinLobby;

            //string relaycode = joinedLobby.Data["RELAY_CODE"].Value;
            //NetworkData.JoinRelay(relaycode);

            if (queryResponse.Results.Count > 0)
            {
                MenuPage.SetActive(false);
                LobbyPage.SetActive(true);
            }

            PrintPlayerInLobby();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public void PrintPlayerInLobby()
    {
        Lobby lobby = joinedLobby;
        int num = 1;
        foreach(Player player in lobby.Players)
        {
            print("Player" + num + ": " + player.Id + " " + player.Data["PlayerName"].Value + "  Team: " + player.Data["Team"].Value);
            num++;
        }

        print(joinedLobby.Data["START"].Value);
    }

    public Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
                        { "Team", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, tag) }
                    }
        };
    }

    public async void UpdateGamemode(string gameMode)
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode) }
                }
            });
            joinedLobby = hostLobby;

            PrintPlayerInLobby();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void UpdatePlayerNameAndTeam(string newplayerName, string team)
    {
        try
        {
            playerName = newplayerName;
            Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(hostLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
                    {"Team", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, team) }
                }
            });

            joinedLobby = lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void UpdateStartGame(string startGame)
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {"START", new DataObject(DataObject.VisibilityOptions.Public, startGame) }
                }
            });
            joinedLobby = hostLobby;
            //NetworkData.joinedLobby = hostLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void LobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            hearbeatTimer -= Time.deltaTime;
            if (hearbeatTimer < 0f)
            {
                float heartbeatTimerMax = 20f;
                hearbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    public async void LobbyPollForUpdate()
    {
        if (hostLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                float lobbyUpdateTimerMax = 1.1f;
                lobbyUpdateTimer = lobbyUpdateTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(hostLobby.Id);
                joinedLobby = lobby;

                if (joinedLobby.Data["START"].Value == "YES")
                {
                    //SceneManager.LoadScene("Game");
                    foreach (Player player in joinedLobby.Players)
                    {
                        if (joinedLobby.HostId == player.Id)
                        {
                            //NetworkData.CreateRelay();
                            print("host");
                        }
                        else if (joinedLobby.HostId != player.Id)
                        {
                            print("client");
                        }
                    }
                }
            }
        }
    }
    public async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}