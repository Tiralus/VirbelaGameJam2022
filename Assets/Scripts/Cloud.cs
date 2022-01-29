using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour
{
    [Header("Movement")]
    public float speed;
    public Vector3 min;
    public Vector3 max;
    
    [Header("Rain")]
    public ParticleSystem rain;
    public float rainDirectionMagnitude;
    public float rainPower;

    [Header("Lightning")]
    public ParticleSystem lightning;
    
    [Header("Input")]
    public List<KeyCode> left;
    public List<KeyCode> right;
    public List<KeyCode> forward;
    public List<KeyCode> backward;
    public List<KeyCode> up;
    public List<KeyCode> down;

    public List<KeyCode> rainKey;
    public List<KeyCode> lightningKey;

    private ParticleSystem.ForceOverLifetimeModule _rainForce;
    private ParticleSystem.EmissionModule _rainEmmission;

    private bool _rainOn = false;

    private void Start()
    {
        _rainForce = rain.forceOverLifetime;
        _rainEmmission = rain.emission;

        EnableRain(_rainOn);
    }

    private void Update()
    {
        Vector3 position = transform.position;
        Vector3 direction = GetInputDirection();
        position += direction.normalized * speed * Time.deltaTime;
        position = ClampPosition(position);
        transform.position = position;

        if (IsKeyPressed(rainKey))
            EnableRain(!_rainOn);

        UpdateRain(direction);

        if (IsKeyPressed(lightningKey))
            ActivateLightning();
    }

    private Vector3 ClampPosition(Vector3 position)
    {
        if (position.x > max.x)
        {
            position.x = max.x;
        }
        
        if (position.y > max.y)
        {
            position.y = max.y;
        }
        
        if (position.z > max.z)
        {
            position.z = max.z;
        }
        
        if (position.x < min.x)
        {
            position.x = min.x;
        }
        
        if (position.y < min.y)
        {
            position.y = min.y;
        }
        
        if (position.z < min.z)
        {
            position.z = min.z;
        }

        return position;
    }

    private Vector3 GetInputDirection()
    {
        Vector3 direction = Vector2.zero;

        if (IsKeyDown(left))
        {
            direction.x -= 1;
        }
        
        if (IsKeyDown(right))
        {
            direction.x += 1;
        }
        
        if (IsKeyDown(backward))
        {
            direction.z -= 1;
        }
        
        if (IsKeyDown(forward))
        {
            direction.z += 1;
        }
        
        if (IsKeyDown(down))
        {
            direction.y -= 1;
        }
        
        if (IsKeyDown(up))
        {
            direction.y += 1;
        }
        
        return direction;
    }

    public bool IsKeyDown(List<KeyCode> key)
    {
        foreach (KeyCode keyCode in key)
        {
            if (Input.GetKey(keyCode))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsKeyPressed(List<KeyCode> key)
    {
        foreach (KeyCode keyCode in key)
        {
            if (Input.GetKeyDown(keyCode))
            {
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 size = max - min;
        Vector3 center = min + (size / 2f);
        Gizmos.DrawWireCube(center, size);
    }

    public void EnableRain(bool enable)
    {
        _rainOn = enable;
        _rainEmmission.enabled = _rainOn;
    }

    private void UpdateRain(Vector3 direction)
    {
        if (!_rainOn) return;

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

    private void ActivateLightning()
    {
        lightning.Play();
    }
}
