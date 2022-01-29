using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
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
    }

    private void UpdateWaterMeter(float value)
    {
        waterMeter.value = Mathf.Clamp(value, 0, waterMeter.maxValue);

        if (waterMeter.value <= waterMeter.maxValue * lowFillPerc)
            fill.color = lowFillColor;
        else
            fill.color = fillColor;
    }
}
