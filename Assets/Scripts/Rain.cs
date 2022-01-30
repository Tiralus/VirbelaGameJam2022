using System;
using UnityEngine;

public class Rain : MonoBehaviour
{
    public static Rain Instance;
    
    [Header("Rain")]
    public ParticleSystem rainParticle;
    public float rainDirectionMagnitude;
    public float rainPower;
    public float WaterSaturationRate;
    public float WaterFireSuppressRate;
    public int WaterHealAmount;

    [Header("Water Resource")]
    public float waterCapacity = 50f;
    public float waterUseRate = 1f;
    public float waterRefillRate = 5f;

    private bool _rainOn = false;
    private float _waterCurrentAmount;

    private ParticleSystem.ForceOverLifetimeModule _rainForce;
    private ParticleSystem.EmissionModule _rainEmmission;

    public Action<float> UpdateWaterResource;

    public bool CanRain() => _waterCurrentAmount > 0;
    public bool IsRaining() => _rainOn;

    public bool isOverWaterSource = false;

    private void Awake()
    {
        Instance = this;
        
        _rainForce = rainParticle.forceOverLifetime;
        _rainEmmission = rainParticle.emission;

        _waterCurrentAmount = waterCapacity;        
    }

    private void Update()
    {
        if (GameManager.Instance.IsPaused() || !GameManager.Instance.InPlay()) return;

        if (!CanRain() && IsRaining())
            EnableRain(false);

        UseWater();
        RefillWater();
    }

    public void EnableRain(bool enable)
    {
        _rainOn = CanRain() && enable;
        _rainEmmission.enabled = _rainOn;
    }

    public void UpdateRainParticles(Vector3 direction)
    {
        if (!IsRaining()) return;

        ParticleSystem.MinMaxCurve rainForceX = _rainForce.x;
        rainForceX.constant = -direction.x * rainDirectionMagnitude;
        _rainForce.x = rainForceX;

        ParticleSystem.MinMaxCurve rainForceZ = _rainForce.z;
        rainForceZ.constant = -direction.z * rainDirectionMagnitude;
        _rainForce.z = rainForceZ;

        ParticleSystem.MinMaxCurve rainRate = _rainEmmission.rateOverTime;
        rainRate.constant = rainPower;
        _rainEmmission.rateOverTime = rainRate;
    }

    public void UseWater(bool checkIsRaining = true)
    {
        if (checkIsRaining && !IsRaining()) return;

        _waterCurrentAmount -= waterUseRate * Time.deltaTime;
        _waterCurrentAmount = Mathf.Clamp(_waterCurrentAmount, 0, waterCapacity);

        UpdateWaterResource?.Invoke(_waterCurrentAmount);
    }

    public void RefillWater()
    {
        if (!isOverWaterSource) return;

        _waterCurrentAmount += waterRefillRate * Time.deltaTime;
        _waterCurrentAmount = Mathf.Clamp(_waterCurrentAmount, 0, waterCapacity);

        UpdateWaterResource?.Invoke(_waterCurrentAmount);
    }
}
