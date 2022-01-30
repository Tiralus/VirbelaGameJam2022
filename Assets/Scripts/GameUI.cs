using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("Menus")]
    public GameObject mainMenu;
    public GameObject gameHUD;
    public GameObject pauseMenu;
    public GameObject endGameMenu;

    [Header("Water Meter")]
    public Slider waterMeter;
    public Image fill;
    public float lowFillPerc = 0.25f;

    private Cloud cloud;

    private Color fillColor;
    private Color lowFillColor = Color.red;

    private void Awake()
    {
        fillColor = fill.color;  
    }

    private void Start()
    {
        cloud = FindObjectOfType<Cloud>();
        if (cloud)
        {
            cloud.rain.UpdateWaterResource += UpdateWaterMeter;
            waterMeter.maxValue = cloud.rain.waterCapacity;
            UpdateWaterMeter(waterMeter.maxValue);
        }
        
        AudioManager.instance.PlayMusic("Drizzle");
        mainMenu.SetActive(true);
        
        gameHUD.SetActive(false);
        pauseMenu.SetActive(false);
        endGameMenu.SetActive(false);
    }

    private void UpdateWaterMeter(float value)
    {
        waterMeter.value = Mathf.Clamp(value, 0, waterMeter.maxValue);

        if (waterMeter.value <= waterMeter.maxValue * lowFillPerc)
            fill.color = lowFillColor;
        else
            fill.color = fillColor;
    }

    public void StartGameButtonPressed()
    {
        mainMenu.SetActive(false);
        gameHUD.SetActive(true);
        AudioManager.instance.StopMusic("Drizzle");
        AudioManager.instance.PlayMusic("Thunderstorm");

        // TODO: call to start game
    }

    public void PauseButtonPressed()
    {
        pauseMenu.SetActive(true);
        gameHUD.SetActive(false);

        // TODO: Pause game
    }

    public void ResumeButtonPressed()
    {
        pauseMenu.SetActive(false);
        gameHUD.SetActive(true);

        // TODO: Unpause game
    }

    public void RestartButtonPressed()
    {
        mainMenu.SetActive(true);
        gameHUD.SetActive(false);
        pauseMenu.SetActive(false);
        endGameMenu.SetActive(false);

        // TODO: Reset game
    }

    public void ShowEndGameMenu()
    {
        gameHUD.SetActive(false);
        endGameMenu.SetActive(true);
    }
}
