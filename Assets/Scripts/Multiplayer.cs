using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public enum EncryptionType
{
    DTLS, // Datagram Transport Layer Security
    WSS  // Web Socket Secure
}
// Note: Also Udp and Ws are possible choices

public class Multiplayer : MonoBehaviour
{
    [SerializeField] string lobbyName = "TodakTestLobby";
    [SerializeField] int maxPlayers = 10;
    [SerializeField] EncryptionType encryption = EncryptionType.DTLS;

    public static Multiplayer Instance { get; private set; }
    public NetworkData networkData;

    public string PlayerId { get; private set; }
    public string PlayerName { get; private set; }

    public string PlayerTeam { get; set; }

    private float hearbeatTimer;
    private float lobbyUpdateTimer;

    public Lobby currentLobby;
    string connectionType => encryption == EncryptionType.DTLS ? k_dtlsEncryption : k_wssEncryption;

    const float k_lobbyHeartbeatInterval = 20f;
    const float k_lobbyPollInterval = 65f;
    const string k_keyJoinCode = "RelayJoinCode"; // String keycode for client to join relay
    const string k_dtlsEncryption = "dtls"; // Datagram Transport Layer Security
    const string k_wssEncryption = "wss"; // Web Socket Secure

    async void Start()
    {
        Instance = this;
        DontDestroyOnLoad(this);

        await Authenticate();
    }

    void Update()
    {
        _ = HandleHeartbeatAsync();
        _ = HandlePollForUpdatesAsync();
    }

    async Task Authenticate()
    {
        await Authenticate("Player" + Random.Range(0, 1000));
    }

    async Task Authenticate(string playerName) // Create new player id
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            InitializationOptions options = new InitializationOptions();
            options.SetProfile(playerName);

            await UnityServices.InitializeAsync(options);
        }

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in as " + AuthenticationService.Instance.PlayerId);
        };

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            PlayerId = AuthenticationService.Instance.PlayerId;
            PlayerName = playerName;
        }
    }

    public async Task CreateLobby() // Create lobby and relay
    {
        try
        {
            Allocation allocation = await AllocateRelay();
            string relayJoinCode = await GetRelayJoinCode(allocation);

            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false
            };

            currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            Debug.Log("Created lobby: " + currentLobby.Name + " with code " + currentLobby.LobbyCode);

            await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject> {
                        {k_keyJoinCode, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode)}
                    }
            });

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(
                allocation, connectionType));

            NetworkManager.Singleton.StartHost();

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to create lobby: " + e.Message);
        }
    }

    public async Task LeaveGame() // Leave game
    {
        try
        {
            if (currentLobby.HostId == PlayerId) // If host...
            {
                await LobbyService.Instance.DeleteLobbyAsync(currentLobby.Id);
                NetworkManager.Singleton.Shutdown();
                networkData.ShowMenu();
                print("host leave game. lobby removed");
            }
            else // If client...
            {
                await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, PlayerId);
                currentLobby = null;
                NetworkManager.Singleton.Shutdown();
                networkData.ShowMenu();
                print("client leave game");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async Task QuickJoinLobby() // Join lobby for quick join
    {
        try
        {
            currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            string relayJoinCode = currentLobby.Data[k_keyJoinCode].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(
                joinAllocation, connectionType));

            NetworkManager.Singleton.StartClient();

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to quick join lobby: " + e.Message);
        }
    }

    async Task<Allocation> AllocateRelay() // Create relay. Code called from CreateLobby()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
            return allocation;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Failed to allocate relay: " + e.Message);
            return default;
        }
    }

    async Task<string> GetRelayJoinCode(Allocation allocation) // Return relay code just created. Code called from CreateLobby()
    {
        try
        {
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return relayJoinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Failed to get relay join code: " + e.Message);
            return default;
        }
    }

    async Task<JoinAllocation> JoinRelay(string relayJoinCode) // Join relay from code saved in k_keyJoinCode
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            return joinAllocation;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Failed to join relay: " + e.Message);
            return default;
        }
    }

    async Task HandleHeartbeatAsync()
    {
        if (currentLobby != null)
        {
            hearbeatTimer -= Time.deltaTime;
            if (hearbeatTimer < 0f)
            {
                float heartbeatTimerMax = 20f;
                hearbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
            }
        }
    }

    async Task HandlePollForUpdatesAsync() // Update lobby data
    {
        try
        {
            if (currentLobby != null)
            {
                lobbyUpdateTimer -= Time.deltaTime;
                if (lobbyUpdateTimer < 0f)
                {
                    float lobbyUpdateTimerMax = 1.1f;
                    lobbyUpdateTimer = lobbyUpdateTimerMax;

                    currentLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
                }
            }
        }
        catch (LobbyServiceException e)
        {
            networkData.ShowMenu();
            Debug.LogError("Failed to poll for updates on lobby: " + e.Message);
        }
    }
}
