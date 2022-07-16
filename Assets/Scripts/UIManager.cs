using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class UIManager : MonoBehaviourPunCallbacks
{
    [Header("Panel/Screens")]
    [SerializeField] private GameObject m_StartPanel;
    [SerializeField] private GameObject m_RoomListPanel;
    [SerializeField] private GameObject m_RoomPanel;
    [SerializeField] private GameObject m_OptionsPanel;

    [Header("StartPanel Objects")]
    [SerializeField] private Image m_ControlsImage;
    [SerializeField] private Image m_GameplayImage;

    [Header("RoomCreation Objects")]
    [SerializeField] private TMP_InputField m_RoomNameInput;
    [SerializeField] private TMP_Dropdown m_RoomPlayerCount;
    [SerializeField] private TMP_Dropdown m_RoomMapIndex;

    [Header("RoomList Objects")]
    [SerializeField] private GameObject m_NewRoomParent;
    [SerializeField] private RoomListEntry m_RoomListEntryPrefab;

    public override void OnRoomListUpdate(List<Photon.Realtime.RoomInfo> roomList)
    {
        Debug.Log("new room added to the list");
    }

    public void ShowPanel(GameObject _panel)
    {
        m_StartPanel.SetActive(false);
        m_RoomListPanel.SetActive(false);
        m_RoomPanel.SetActive(false);
        m_OptionsPanel.SetActive(false);

        _panel.SetActive(true);
    }
    public void CreateRoom()
    {
        RoomListEntry roomPanel = Instantiate(m_RoomListEntryPrefab, m_NewRoomParent.transform);
        roomPanel.Init(m_RoomNameInput.text, (byte)m_RoomMapIndex.value, (byte)(m_RoomPlayerCount.value + 2), this);

        string name = roomPanel.RoomInfo.LobbyName;
        byte maxPlayers = roomPanel.RoomInfo.MaxPlayerCount;

        PhotonNetwork.CreateRoom(name, new RoomOptions { MaxPlayers = maxPlayers },TypedLobby.Default);
    }
    public override void OnCreatedRoom()
    {
        Debug.Log("Created Room: " + m_RoomNameInput.text);
    }
}
