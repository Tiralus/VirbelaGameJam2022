using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour
{
    [Header("Movement")]
    public float Speed;
    public Vector3 Min;
    public Vector3 Max;
    
    [Header("Rain")]
    public ParticleSystem Rain;
    public float RainDirectionMagnitude;
    public float RainPower;
    
    [Header("Input")]
    public List<KeyCode> Left;
    public List<KeyCode> Right;
    public List<KeyCode> Forward;
    public List<KeyCode> Backward;
    public List<KeyCode> Up;
    public List<KeyCode> Down;

    private ParticleSystem.ForceOverLifetimeModule _rainForce;
    private ParticleSystem.EmissionModule _rainEmmission;

    private void Start()
    {
        _rainForce = Rain.forceOverLifetime;
        _rainEmmission = Rain.emission;
    }

    private void Update()
    {
        Vector3 position = transform.position;
        Vector3 direction = GetInputDirection();
        position += direction.normalized * Speed * Time.deltaTime;
        position = ClampPosition(position);
        transform.position = position;
        
        ParticleSystem.MinMaxCurve rainForceX = _rainForce.x;
        rainForceX.constant = -direction.x * RainDirectionMagnitude;
        _rainForce.x = rainForceX;
        
        ParticleSystem.MinMaxCurve rainForceZ = _rainForce.z;
        rainForceZ.constant = -direction.z * RainDirectionMagnitude;
        _rainForce.z = rainForceZ;

        ParticleSystem.MinMaxCurve rainRate = _rainEmmission.rateOverTime;
        rainRate.constant = RainPower;
        _rainEmmission.rateOverTime = rainRate;
    }

    private Vector3 ClampPosition(Vector3 position)
    {
        if (position.x > Max.x)
        {
            position.x = Max.x;
        }
        
        if (position.y > Max.y)
        {
            position.y = Max.y;
        }
        
        if (position.z > Max.z)
        {
            position.z = Max.z;
        }
        
        if (position.x < Min.x)
        {
            position.x = Min.x;
        }
        
        if (position.y < Min.y)
        {
            position.y = Min.y;
        }
        
        if (position.z < Min.z)
        {
            position.z = Min.z;
        }

        return position;
    }

    private Vector3 GetInputDirection()
    {
        Vector3 direction = Vector2.zero;

        if (IsKeyDown(Left))
        {
            direction.x -= 1;
        }
        
        if (IsKeyDown(Right))
        {
            direction.x += 1;
        }
        
        if (IsKeyDown(Backward))
        {
            direction.z -= 1;
        }
        
        if (IsKeyDown(Forward))
        {
            direction.z += 1;
        }
        
        if (IsKeyDown(Down))
        {
            direction.y -= 1;
        }
        
        if (IsKeyDown(Up))
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 size = Max - Min;
        Vector3 center = Min + (size / 2f);
        Gizmos.DrawWireCube(center, size);
    }
}
