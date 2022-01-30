using System;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour
{
    public static Cloud Instance;
    
    [Header("Movement")]
    public float Speed;
    public Vector3 Min;
    public Vector3 Max;
    public LayerMask GroundLayer;
    public Transform CameraTransform;
    
    [Header("Lightning")]
    public ParticleSystem lightning;
    public float LightningChanceToStartFire;
    public int LightningDamage;
    
    [Header("Input")]
    public List<KeyCode> Left;
    public List<KeyCode> Right;
    public List<KeyCode> Forward;
    public List<KeyCode> Backward;
    public List<KeyCode> Up;
    public List<KeyCode> Down;

    public List<KeyCode> rainKey;
    public List<KeyCode> lightningKey;

    public Rain rain;

    public Transform DropShadow;

    private BaseTile _selectedTile;
    
    public bool IsLightning { get; private set; }

    private void Start()
    {
        rain = GetComponent<Rain>();
        Instance = this;
    }

    private void Update()
    {
        if (GameManager.Instance.IsPaused()) return;

        Vector3 direction = GetInputDirection();

        transform.position += CameraTransform.TransformDirection(direction).normalized * Speed * Time.deltaTime;
        transform.position = ClampPosition(transform.position);
        
        rain.UpdateRainParticles(direction);

        if (IsKeyPressed(rainKey))
            rain.EnableRain(!rain.IsRaining());

        if (IsKeyPressed(lightningKey))
            ActivateLightning();

        BaseTile previousSelectedTile = _selectedTile;
        
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit) &&
            hit.collider != null)
        {
            BaseTile tile = hit.collider.GetComponent<BaseTile>();
            
            if (tile != null &&
                _selectedTile != tile)
            {
                tile.Select();
                _selectedTile = tile;
            }

            if (hit.collider is SphereCollider sphereCollider)
            {
                Vector3 dropShadowPosition = DropShadow.position;
                dropShadowPosition.y = sphereCollider.transform.position.y + sphereCollider.center.y;
                DropShadow.position = dropShadowPosition;
            }
        }

        if (_selectedTile != previousSelectedTile &&
            previousSelectedTile != null)
        {
            previousSelectedTile.Deselect();
        }

        rain.isOverWaterSource = _selectedTile != null && _selectedTile is WaterTile;
    }

    private void LateUpdate()
    {
        IsLightning = false;
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
        Vector3 size = Max - Min;
        Vector3 center = Min + (size / 2f);
        Gizmos.DrawWireCube(center, size);
    }

    private void ActivateLightning()
    {
        AudioManager.instance.PlaySound("Thunder");
        lightning.Play();
        IsLightning = true;
    }
}
