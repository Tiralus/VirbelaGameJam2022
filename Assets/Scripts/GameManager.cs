using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Tooltip("Percentage of grass tiles present for player to win")]
    public float playerPerc = 0.5f;
    [Tooltip("Percentage of corruption tiles present for enemy to win")]
    public float enemyPerc = 0.5f;

    public bool IsPaused { get; private set; }

    public bool InPlay { get; private set; }

    public List<GameObject> Levels;
    public int levelIndex = 0;

    public static Action<bool> IsEndGame;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        StartLevel(levelIndex);

        ThreeDeeTiles.UpdateTiles += CheckEndGame;
    }

    private void OnDestroy()
    {
        ThreeDeeTiles.UpdateTiles -= CheckEndGame;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartLevel(levelIndex);
    }

    public void ReloadLevel()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    public void LoadNextLevel()
    {
        if (levelIndex < Levels.Count - 1)
            levelIndex++;
        else
            levelIndex = 0;

        ReloadLevel();
    }

    public void StartLevel(int index)
    {
        AudioManager.instance.StopMusic("Thunderstorm");
        AudioManager.instance.PlayMusic("Drizzle");

        InPlay = false;
        PauseGame(false);
        Instantiate(Levels[levelIndex]);
    }

    public void StartGame()
    {
        InPlay = true;
    }

    private void CheckEndGame(float grassPerc, float corruptionPerc)
    {
        if (!InPlay) return;

        if (grassPerc >= playerPerc || corruptionPerc == 0f)
            EndGame(true);
        else if (corruptionPerc >= enemyPerc || grassPerc == 0f)
            EndGame(false);
    }

    public void EndGame(bool win)
    {
        if (!InPlay) return;

        InPlay = false;

        IsEndGame?.Invoke(win);
    }

    public void PauseGame(bool pause)
    {
        IsPaused = pause;
        Time.timeScale = IsPaused ? 0f : 1f;
    }
}
