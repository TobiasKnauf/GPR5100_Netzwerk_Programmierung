using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerNetworkManager : MonoBehaviourPunCallbacks
{
    private List<RoomInfo> roomList;
    [SerializeField] private UIManager UIManager;

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        base.OnConnectedToMaster();
    }
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
    }
    public override void OnRoomListUpdate(List<RoomInfo> _list)
    {
        roomList = _list;
        UIManager.CreateNewRoomButtons(roomList);

        base.OnRoomListUpdate(roomList);
    }
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
    }
    public override void OnJoinedRoom()
    {
        UIManager.OnJoined();
        UIManager.UpdatePlayerPanels();

        base.OnJoinedRoom();
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UIManager.UpdatePlayerPanels();

        base.OnPlayerEnteredRoom(newPlayer);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UIManager.UpdatePlayerPanels();

        base.OnPlayerLeftRoom(otherPlayer);
    }
}
