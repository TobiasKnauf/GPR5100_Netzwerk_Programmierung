using System;
using TMPro;
using UnityEngine;

[Serializable]
public struct RoomInfo
{
    public string LobbyName;
    public int MaxPlayerCount;
    public int CurrentPlayerCount;
    public int MapIndex;
    public RoomInfo(string _name, int _maxPlayers, int _mapIndex)
    {
        LobbyName = _name;
        MaxPlayerCount = _maxPlayers;
        CurrentPlayerCount = 0;
        MapIndex = _mapIndex;
    }
}

public class RoomListEntry : MonoBehaviour
{
    public RoomInfo RoomInfo;

    [SerializeField] private TMP_Text m_RoomName;
    [SerializeField] private TMP_Text m_RoomPlayers;

    private UIManager uiManager;

    private void Start()
    {
        uiManager = GetComponent<UIManager>();

        RoomInfo = new RoomInfo(Name, MaxPlayers, MapIndex);


        m_RoomName.text = Name;
        m_RoomPlayers.text = $"{RoomInfo.CurrentPlayerCount}/{MaxPlayers}";
    }

    public void Init(string _name, int _playercount, int _maxplayers)
    {
        Name = _name;
        RoomInfo.CurrentPlayerCount = _playercount;
        MaxPlayers = _maxplayers;
    }

    public void Update()
    {
        m_RoomPlayers.text = $"{RoomInfo.CurrentPlayerCount}/{MaxPlayers}";
    }
    public void JoinRoom()
    {
        if (RoomInfo.CurrentPlayerCount < MaxPlayers)
        {
            uiManager.UserRoom = Name;
            RoomInfo.CurrentPlayerCount++;
        }
        else
            Debug.LogWarning($"Can't join this room. Room {Name} is already full");
    }
}
