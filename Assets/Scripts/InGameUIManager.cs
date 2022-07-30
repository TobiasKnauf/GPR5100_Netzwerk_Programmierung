using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

public class InGameUIManager : MonoBehaviour
{
    [Header("Pause UI")]
    [SerializeField] private Canvas m_PauseCanvas;
    [Header("Round Results")]
    [SerializeField] private Canvas m_ResultCanvas;
    [SerializeField] private TMP_Text m_RoundWinnerText;
    [SerializeField] private Image m_RoundWinnerColorImage;
    [SerializeField] private Slider m_SldTimer;
    [SerializeField] private float m_TimeUntilNextRound;
    [Header("Game Results")]
    [SerializeField] private Canvas m_GameOverCanvas;
    [SerializeField] private GameObject[] m_StandingsPanels;
    [SerializeField] private TMP_Text[] m_GameStandingsText;
    [SerializeField] private Image[] m_GameStandingsColor;
    private PlayerController[] PlayerStandings;


    private float timer;
    private bool roundOver;

    private void Start()
    {
        m_PauseCanvas.enabled = false;
        for (int i = 0; i < m_StandingsPanels.Length; i++)
        {
            m_StandingsPanels[i].SetActive(false);
        }
    }

    private void Update()
    {
        if (roundOver)
        {
            timer -= Time.deltaTime;
            m_SldTimer.value = timer;
            if(timer <= 0)
            {
                StartNextRound();
            }
        }
    }
    public void EndOfMatch()
    {
        m_GameOverCanvas.enabled = true;
        for (int i = 0; i < GameManager.Instance.Players.Count; i++)
        {
            PlayerStandings[i] = GameManager.Instance.Players.ElementAt(i).Value;
        }
        PlayerStandings.OrderBy(player => player.Wins);
        for (int i = 0; i < GameManager.Instance.Players.Count; i++)
        {
            m_StandingsPanels[i].SetActive(true);
            m_GameStandingsText[i].text = PlayerStandings[i].NickName;
            m_GameStandingsColor[i].color = PlayerStandings[i].Color;
        }
    }
    private void StartNextRound()
    {
        roundOver = false;
        GameManager.Instance.StartRound();
        Debug.Log("Started next Round: " + GameManager.Instance.CurrentRound);
        m_ResultCanvas.enabled = false;
    }

    public void ShowPanel()
    {
        m_PauseCanvas.enabled = !m_PauseCanvas.enabled;
    }
    public void EndOfRound()
    {
        PlayerController winner = GameManager.Instance.GetWinner();

        GameManager.Instance.RepositionPlayers();

        m_ResultCanvas.enabled = true;
        m_SldTimer.maxValue = m_TimeUntilNextRound;
        m_SldTimer.value = m_SldTimer.maxValue;
        m_RoundWinnerText.text = winner.NickName + " won!";
        m_RoundWinnerColorImage.color = winner.Color;
        timer = m_TimeUntilNextRound;
        roundOver = true;
    }

    public void LeaveGame()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }
}
