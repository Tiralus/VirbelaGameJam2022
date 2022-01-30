using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameUI gameUI;

    private bool gamePaused = false;
    public bool IsPaused() => gamePaused;

    private void Awake()
    {
        Instance = this;

        PauseGame(false);
    }

    public void StartGame()
    {
        //TODO: Activate game events / enemies
    }

    public void EndGame(bool win)
    {
        // TODO: Stop game events / enemies

        gameUI.ShowEndGameMenu(win);
    }

    public void PauseGame(bool pause)
    {
        gamePaused = pause;
        Time.timeScale = gamePaused ? 0f : 1f;
    }

    public void ResetGame()
    {
        Scene scene = SceneManager.GetActiveScene(); 
        SceneManager.LoadScene(scene.name);
    }
}
