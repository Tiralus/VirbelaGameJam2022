using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("Menus")]
    public GameObject mainMenu;
    public GameObject gameHUD;
    public GameObject pauseMenu;
    public GameObject endGameMenu;
    [Space]
    public GameObject creditsPanel;

    [Header("Water Meter")]
    public Slider waterMeter;
    public Image waterFill;
    public float lowFillPerc = 0.25f;

    [Header("Win Meter")]
    public Slider grassMeter;
    public Slider corruptionMeter;

    [Header("Lightning Meter")]
    public Slider lightningMeter;

    [Header("Elements")]
    public GameObject winTitle;
    public GameObject loseTitle;

    private Color waterFillColor;
    private Color lowFillColor = Color.red;

    private Cloud cloud;

    private void Awake()
    {
        waterFillColor = waterFill.color;

        cloud = FindObjectOfType<Cloud>();
        if (cloud)
        {
            cloud.rain.UpdateWaterResource += UpdateWaterMeter;
            waterMeter.maxValue = cloud.rain.waterCapacity;
            UpdateWaterMeter(waterMeter.maxValue);

            cloud.UpdateLightningResource += UpdateLightningMeter;
        }

        AudioManager.instance.PlayMusic("Drizzle");
        mainMenu.SetActive(true);

        gameHUD.SetActive(false);
        pauseMenu.SetActive(false);
        endGameMenu.SetActive(false);
        creditsPanel.SetActive(false);
        
        grassMeter.maxValue = 1;
        corruptionMeter.maxValue = 1;

        ThreeDeeTiles.UpdateTiles += UpdateWinMeter;
        GameManager.IsEndGame += ShowEndGameMenu;
    }

    private void OnDestroy()
    {
        ThreeDeeTiles.UpdateTiles -= UpdateWinMeter;
        GameManager.IsEndGame -= ShowEndGameMenu;
    }

    private void UpdateWaterMeter(float value)
    {
        waterMeter.value = Mathf.Clamp(value, 0, waterMeter.maxValue);

        if (waterMeter.value <= waterMeter.maxValue * lowFillPerc)
            waterFill.color = lowFillColor;
        else
            waterFill.color = waterFillColor;
    }

    private void UpdateWinMeter(float grassPerc, float corruptionPerc)
    {
        grassMeter.value = grassPerc;
        corruptionMeter.value = corruptionPerc;
    }

    private void UpdateLightningMeter(float value, float maxValue)
    {
        lightningMeter.maxValue = maxValue;
        lightningMeter.value = Mathf.Clamp(value, 0, lightningMeter.maxValue);
    }

    public void StartGameButtonPressed()
    {
        mainMenu.SetActive(false);
        creditsPanel.SetActive(false);
        gameHUD.SetActive(true);
        AudioManager.instance.StopMusic("Drizzle");
        AudioManager.instance.PlayMusic("Thunderstorm");

        GameManager.Instance.StartGame();
    }

    public void ShowCreditsPanel()
    {
        creditsPanel.SetActive(true);
    }

    public void CloseCreditsPanel()
    {
        creditsPanel.SetActive(false);
    }

    public void PauseButtonPressed()
    {
        pauseMenu.SetActive(true);
        gameHUD.SetActive(false);

        GameManager.Instance.PauseGame(true);
    }

    public void ResumeButtonPressed()
    {
        pauseMenu.SetActive(false);
        gameHUD.SetActive(true);

        GameManager.Instance.PauseGame(false);
    }

    public void RestartButtonPressed()
    {
        mainMenu.SetActive(true);
        gameHUD.SetActive(false);
        pauseMenu.SetActive(false);
        endGameMenu.SetActive(false);

        GameManager.Instance.ReloadLevel();
    }

    public void NextLevelButtonPressed()
    {
        mainMenu.SetActive(true);
        gameHUD.SetActive(false);
        pauseMenu.SetActive(false);
        endGameMenu.SetActive(false);

        GameManager.Instance.LoadNextLevel();
    }

    public void ShowEndGameMenu(bool win)
    {
        gameHUD.SetActive(false);
        endGameMenu.SetActive(true);

        winTitle.SetActive(win);
        loseTitle.SetActive(!win);
    }
}
