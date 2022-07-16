using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System;

public class UIManager : MonoBehaviour
{
    [SerializeField] private PlayerNetworkManager player;

    [Header("Panel/Screens")]
    [SerializeField] private Canvas m_StartPanel;
    [SerializeField] private Canvas m_RoomListPanel;
    [SerializeField] private Canvas m_RoomPanel;
    [SerializeField] private Canvas m_OptionsPanel;

    [Header("StartPanel Objects")]
    [SerializeField] private Image m_ControlsImage;
    [SerializeField] private Image m_GameplayImage;
    [SerializeField] private TMP_InputField m_UserNameInput;

    [Header("RoomCreation Objects")]
    [SerializeField] private TMP_InputField m_RoomNameInput;
    [SerializeField] private TMP_Dropdown m_RoomPlayerCount;
    [SerializeField] private TMP_Dropdown m_RoomMapIndex;

    [Header("RoomList Objects")]
    [SerializeField] public GameObject m_RoomListEntryPrefab;
    [SerializeField] public Transform m_RoomListContent;

    [Header("RoomPanel Objects")]
    [SerializeField] private TMP_Text m_RoomTitleText;
    [SerializeField] private Transform m_ConnectedPlayersContent;
    [SerializeField] private GameObject m_ListedPlayerPrefab;

    private void CloseAllPanels()
    {
        m_StartPanel.enabled = false;
        m_RoomListPanel.enabled = false;
        m_RoomPanel.enabled = false;
        m_OptionsPanel.enabled = false;
    }
    public void ShowPanel(Canvas _panel)
    {
        CloseAllPanels();
        _panel.enabled = true;
    }
    public void CreateNewRoomButtons(List<RoomInfo> _roomList)
    {
        //Clear Room list
        foreach (Transform t in m_RoomListContent)
            Destroy(t.gameObject);

        for (int i = 0; i < _roomList.Count; i++)
        {
            GameObject newRoomListEntry = Instantiate(m_RoomListEntryPrefab, m_RoomListContent);
            newRoomListEntry.transform.Find("Txt_RoomName").GetComponent<TMP_Text>().text = _roomList[i].Name; //Set server name to button
            newRoomListEntry.transform.Find("Txt_Players").GetComponent<TMP_Text>().text = $"{_roomList[i].PlayerCount}/{_roomList[i].MaxPlayers}"; //Set current player and max players to button
            newRoomListEntry.GetComponent<Button>().onClick.AddListener(delegate { OnClick_JoinRoom(newRoomListEntry); }); //Assigns new Function to this button
        }
    }
    public void UpdatePlayerPanels()
    {
        //Clear Player Panels
        foreach (Transform t in m_ConnectedPlayersContent)
            Destroy(t.gameObject);

        Color[] colors =
        {
            Color.red,
            Color.green,
            Color.blue,
            Color.yellow,
        };

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            GameObject newPlayer = Instantiate(m_ListedPlayerPrefab, m_ConnectedPlayersContent);
            newPlayer.GetComponentInChildren<TMP_Text>().text = PhotonNetwork.PlayerList[i].NickName;
            newPlayer.GetComponent<Image>().color = colors[i];

            if (PhotonNetwork.PlayerList[i].IsMasterClient)
                AddHostStartButton(newPlayer);
        }
    }
    private void AddHostStartButton(GameObject _player)
    {
        Button hostStart = _player.transform.Find("Btn_HostStart").GetComponent<Button>();
        hostStart.gameObject.SetActive(true);
        hostStart.onClick.AddListener(delegate { StartRound(); });
    }

    private void StartRound()
    {
        //PhotonNetwork.LoadLevel(#GAMESCENE);
    }

    public void OnJoined()
    {
        ShowPanel(m_RoomPanel);
        m_RoomTitleText.text = PhotonNetwork.CurrentRoom.Name;
    }
    public void OnClick_CreateRoom()
    {
        if (!string.IsNullOrEmpty(m_UserNameInput.text))
            PhotonNetwork.NickName = m_UserNameInput.text;
        else
            PhotonNetwork.NickName = "Guest";

        RoomOptions options = new RoomOptions()
        {
            MaxPlayers = (byte)(m_RoomPlayerCount.value + 2),
            IsVisible = true, //needs to be true so the roomlist will be updated
            IsOpen = true,
        };
        PhotonNetwork.CreateRoom(m_RoomNameInput.text, options);
    }
    public void OnClick_JoinRoom(GameObject _button)
    {
        if (!string.IsNullOrEmpty(m_UserNameInput.text))
            PhotonNetwork.NickName = m_UserNameInput.text;
        else
            PhotonNetwork.NickName = "Guest";

        string roomName = _button.transform.Find("Txt_RoomName").GetComponent<TMP_Text>().text;
        PhotonNetwork.JoinRoom(roomName);
    }
}
