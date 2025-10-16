using UnityEngine;
using UnityEngine.UI;

public class AudioSourceScanner : MonoBehaviour
{
    [SerializeField] AudioSource startMusic;
    [SerializeField] AudioSource backroundMusic;
    [SerializeField] AudioSource pauseMusic;
    [SerializeField] AudioSource gameOverMusic;
    [SerializeField] AudioSource gameWin;
    [SerializeField] bool musicEnable = false;
    [SerializeField] bool musicDisable = false;
    [SerializeField] public bool musicGameOver = false;


    private void Start()
    {
        if (startMusic != null && backroundMusic != null && pauseMusic != null)
        {
            musicEnable = true;
        }
    }
    public void PauseMusic()
    {
        musicDisable = !musicDisable; 

        if (musicEnable && musicDisable)
        {
            startMusic.Stop();
            backroundMusic.Pause();
            pauseMusic.Play();
        }
        else if(musicEnable && !musicDisable)
        {
            pauseMusic.Stop();
            startMusic.Stop();
            backroundMusic.Play();
        }
    }

    public void GameOverMusic()
    {
        AudioSource[] audio = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource audioSource in audio)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
        if (musicGameOver && gameOverMusic != null)
        {  
                gameOverMusic.Play();
        }
        else if (!musicGameOver && gameWin != null)
        {
            gameWin.Play();
        }
    }
}