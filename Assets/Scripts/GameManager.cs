using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameUI gameUI;
    [Tooltip("Percentage of grass tiles present for player to win")] 
    public float playerPerc = 50f;
    [Tooltip("Percentage of corruption tiles present for enemy to win")] 
    public float enemyPerc = 50f;

    private bool gamePaused = false;
    public bool IsPaused() => gamePaused;

    private bool inPlay = false;
    public bool InPlay() => inPlay;

    private void Awake()
    {
        Instance = this;

        inPlay = false;
        PauseGame(false);
    }

    private void Start()
    {
        ThreeDeeTiles.UpdateTiles += CheckEndGame;
    }

    private void OnDestroy()
    {
        ThreeDeeTiles.UpdateTiles -= CheckEndGame;
    }

    public void StartGame()
    {
        inPlay = true;
    }

    private void CheckEndGame(float grassPerc, float corruptionPerc)
    {
        if (!InPlay()) return;

        if (grassPerc >= playerPerc || corruptionPerc == 0f)
            EndGame(true);
        else if (corruptionPerc >= enemyPerc || grassPerc == 0f)
            EndGame(false);
    }

    public void EndGame(bool win)
    {
        inPlay = false;

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
