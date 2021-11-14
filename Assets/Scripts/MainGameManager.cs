using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainGameManager : MonoBehaviour
{
    public bool isPaused = false;
    public bool gameOver = false;
    public bool gameWon = false;

    public GameObject pauseMenu;
    public GameObject gameOverMenu;
    public GameObject winMenu;

    public AudioClip[] enemySFX;


    // Start is called before the first frame update
    void Start()
    {
        pauseMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        winMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && !gameOver && !gameWon)
        {
            if(!isPaused)
                PauseGame();
            else
                ResumeGame();
        }

        if(gameOver)
        {
            StartCoroutine(handleGameOver());
        }

        if(gameWon)
        {
            StartCoroutine(handleWin());
        }
    }

    void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }


    void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    IEnumerator handleGameOver()
    {
        yield return new WaitForSeconds(0.5f);
        gameOverMenu.SetActive(true);
    }

    IEnumerator handleWin()
    {
        yield return new WaitForSeconds(3f);
        winMenu.SetActive(true);
    }

    public void toMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScreen");
    }
}
