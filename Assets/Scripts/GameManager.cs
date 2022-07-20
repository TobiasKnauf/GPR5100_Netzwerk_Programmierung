using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }

    private void Awake()
    {
        Initialize();
    }

    private void OnDestroy()
    {
        Terminate();
    }

    private void Initialize()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Terminate()
    {
        if (this == Instance)
        {
            instance = null;
        }
    }
    #endregion

    #region Gamestates & Scenes

    [SerializeField] private Scene menu;
    [SerializeField] private Scene match;

    public void StartMatch()
    {
        SceneManager.LoadScene(match.name);
    }

    public void EndMatch()
    {
        SceneManager.LoadScene(menu.name);
    }

    public void LeaveMatch()
    {
        EndConnection();
        SceneManager.LoadScene(menu.name);
    }

    public void QuitGame()
    {
        EndConnection();
        Application.Quit();
    }

    #endregion

    public void EndConnection()
    {
        // TODO
    }
}
