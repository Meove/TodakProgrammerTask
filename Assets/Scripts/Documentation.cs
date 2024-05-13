using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Documentation : MonoBehaviour
{
    // Summarize of Documentation for TodakProgrammingTask
    void Start()
    {
        Debug.LogError("Check out 'Assets > Scripts > Documentation.cs' for documentation"); // Using error bcoz easy to caught attention
    }

    /*
     [MainMenu.scene]
    - After launch game, player authentication registered and saved to the NetworkData/Multiplayer.cs
    - Player given two options, Play and Quick Join
    - Play create lobby, relay, and become a host. Able to start game
    - Quick Join search for available lobby. Wait for host to start game
    - Player given 2 given to teams to choose
    - Click the box to enter the team
    - Host only can start game

    - Bonus, Host/Client can leave the game
    - Host: Remove the lobby, making the whole client kicked out
    - Client: Remove from lobby registered slot. Host lobby is still
    
    [Game.scene]
    - All player in lobby are spawned to the setup scene
    - Player position spawn are based on the team they choose previously
    - Each player given they own cinematic camera
    - Player freely to move around using WASD
    - If host quit the game, the whole client will disconnected from game


    [Code breakdown]
    NetworkData.cs
    - Code start with all button referenced and ready to received click event
    - Show menu page and hide lobby page. If player is host, Start Button would show up
    - CreateGame(), JoinGame(), LeaveGame() call instance from Multiplayer.cs
    - StartGame() called every player in lobby ONLY to change another scene
    - HideMenu() and ShowMenu() are hide/show menu page

    Multiplayer.cs
    - Both script NetworkData.cs and Multiplayer.cs are connected together
    - Start code with authenticated player id when launch game
    - In this task, SignInAnonymouslyAsync() used for testing game with same Unity account
    - After host click Play, lobby, and relay are created
    - For client, player get relay code from lobby option data. Referencing currentlobby from host
    - Heartbeat() are started updating each 1.1 seconds

    LobbyTeamPick.cs
    - Update name when player click join team box to server/client
    - Send the name reference to server then send to the client to update who is the sender name

    LoadScene.cs
    - Wait function call from NetworkData.StartGame()

    GameSetup.cs
    - Set the camera into local object spawned by player
    - SetObjectPosition() change the player position based on they team chosen

    PlayerMove.cs
    - Script is inside Player prefab
    - ChangePos() manipulated player position. It could be called other than team pick position
    - Click WASD for movement
     */
}
