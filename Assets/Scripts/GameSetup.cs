using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class GameSetup : NetworkBehaviour
{
    public Transform RedSpawnPoint;
    public Transform BlueSpawnPoint;
    public CinemachineVirtualCamera VCam;
    [SerializeField] PlayerMove playermove;

    // Start is called before the first frame update
    void Start()
    {
        VCam.LookAt = NetworkManager.LocalClient.PlayerObject.transform; // Get player from network local prefab spawn
        VCam.Follow = NetworkManager.LocalClient.PlayerObject.transform; // Using long reference bcoz can't print typeof, dunno what this type is :/
                                                                         // Less risk 
        playermove = NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerMove>();
        SetObjectPosition();  
    }

    public void SetObjectPosition() // Change player position based on team choose
    {

        if (Multiplayer.Instance.PlayerTeam == "Blue") // If blue team...
        {
            print("Blue");
            playermove.ChangePos(BlueSpawnPoint);
        }
        else if (Multiplayer.Instance.PlayerTeam == "Red") // If red team...
        {
            print("Red");
            playermove.ChangePos(RedSpawnPoint);
        }
    }
}