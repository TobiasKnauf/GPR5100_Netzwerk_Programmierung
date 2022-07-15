using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region Singleton
    private static AudioManager instance;
    public static AudioManager Instance { get { return instance; } }

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

    [SerializeField] List<AudioClip> sfxClips = new List<AudioClip> ();

    [SerializeField] List<AudioClip> musicClips = new List<AudioClip> ();
}
