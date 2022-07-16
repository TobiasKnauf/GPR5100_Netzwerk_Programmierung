using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(Photon.Realtime.TypedLobby.Default);
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("Joined the lobby!");
    }
}
