using System;
using TMPro;
using UnityEngine;
using Photon.Pun;

[Serializable]
public struct RoomInfo
{
    public string LobbyName;
    public byte MaxPlayerCount;
    public byte CurrentPlayerCount;
    public byte MapIndex;
    public RoomInfo(string _name, byte _maxPlayers, byte _mapIndex)
    {
        LobbyName = _name;
        MaxPlayerCount = _maxPlayers;
        CurrentPlayerCount = 0;
        MapIndex = _mapIndex;
    }
}

public class RoomListEntry : MonoBehaviourPunCallbacks
{
    public RoomInfo RoomInfo;

    [SerializeField] private TMP_Text m_RoomName;
    [SerializeField] private TMP_Text m_RoomPlayers;

    private UIManager uiManager;

    public void Init(string _name, byte _mapIndex, byte _maxplayers, UIManager _manager)
    {
        RoomInfo = new RoomInfo(_name, _maxplayers, _mapIndex);
        uiManager = _manager;

        m_RoomName.text = RoomInfo.LobbyName;
        m_RoomPlayers.text = $"{RoomInfo.CurrentPlayerCount}/{RoomInfo.MaxPlayerCount}";
    }

    public void Update()
    {
        m_RoomPlayers.text = $"{RoomInfo.CurrentPlayerCount}/{RoomInfo.MaxPlayerCount}";
    }
    public void JoinRoom()
    {
        if (RoomInfo.CurrentPlayerCount < RoomInfo.MaxPlayerCount)
        {
            PhotonNetwork.JoinRoom(RoomInfo.LobbyName);
        }
        else
            Debug.LogWarning($"Can't join this room. Room {RoomInfo.LobbyName} is already full");
    }
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + RoomInfo.LobbyName);
        RoomInfo.CurrentPlayerCount++;

    }
}
