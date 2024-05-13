using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyUpdateTeam : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetText(string newName, GameObject buttonChild)
    {
        buttonChild.GetComponent<TextMeshProUGUI>().text = newName;
    }
}
