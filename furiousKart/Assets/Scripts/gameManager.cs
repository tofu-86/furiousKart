
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Photon.Realtime;
using Photon.Pun;

public class gameManager : MonoBehaviourPunCallbacks
{

    
    byte maxPlayersPerRoom = 7; // maximum players connectable.
    bool isConnecting; // boolean value to see if client is connecting. 
    public InputField playerName; // playername passing in.
    public Text feedbackText; // Gives information of network status.
    string gameVersion = "1"; // sets game version.



    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        if (PlayerPrefs.HasKey("PlayerName")) // seein gif there is saved data for player name from previous runtime
            playerName.text = PlayerPrefs.GetString("PlayerName"); // setting player name automatically.

    }

    public void ConnectNetwork()
    {
        feedbackText.text = "";
        isConnecting = true;

        PhotonNetwork.NickName = playerName.text;
        if (PhotonNetwork.IsConnected)
        {
            feedbackText.text += "\nJoining Room...";
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            feedbackText.text = "\nConnecting...";
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }


    // setting playerpref when name is set
    public void GiveName(string name)
    {
        name = playerName.text;
        PlayerPrefs.SetString("PlayerName", name);
    }

    public void ConnectSingle()
    {
        SceneManager.LoadScene("Singleplay");
    }

    // Network Callbacks

    public override void OnConnectedToMaster()
    {
        if (isConnecting)
        {
            feedbackText.text += "\nOnConnectedToMaster...";
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        feedbackText.text += "\nFailed to join random room.";
        Debug.Log("Failed to join random room");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = this.maxPlayersPerRoom});

    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        feedbackText.text += "\nDisconnected because " + cause;
        isConnecting = false;

    }

    public override void OnJoinedRoom()
    {
        feedbackText.text += "\nJoined Room with" + PhotonNetwork.CurrentRoom.PlayerCount + "Players.";
        //PhotonNetwork.CurrentRoom.IsOpen = true;
        //PhotonNetwork.CurrentRoom.IsVisible = true;

        PhotonNetwork.LoadLevel("Track1_Grassy");

        }


}
