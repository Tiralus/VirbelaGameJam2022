using System;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour
{
    public enum KeyType
    {
        Down,
        Pressed,
        Released
    }
    
    [Serializable]
    public class InputKey
    {
        public KeyCode Key;
        public KeyType Type;
    }
    
    public static Cloud Instance;
    
    [Header("Movement")]
    public float Speed;
    public float FastSpeed;
    public float AceelerationMultiplier;
    public Vector3 Min;
    public Vector3 Max;
    public LayerMask GroundLayer;
    public Transform CameraTransform;
    
    [Header("Lightning")]
    public ParticleSystem lightning;
    public float LightningChanceToStartFire;
    public int LightningDamage;
    public float LightningCapacity;
    public float LightningUseRate;
    
    [Header("Input")]
    public List<InputKey> Left;
    public List<InputKey> Right;
    public List<InputKey> Forward;
    public List<InputKey> Backward;
    public List<InputKey> Up;
    public List<InputKey> Down;

    public List<InputKey> speedKey;
    public List<InputKey> rainKey;
    public List<InputKey> rainOn;
    public List<InputKey> rainOff;
    public List<InputKey> lightningKey;

    public Rain rain;
    public PowerupSpawner PowerupSpawner;
    public SphereCollider Collider;

    public Transform DropShadow;

    private BaseTile _selectedTile;
    
    public bool IsLightning { get; private set; }

    private float _CurrentSpeed;
    private Vector3 _CurrentDirection = Vector3.zero;
    private float _lightningResource;

    public Action<float, float> UpdateLightningResource;

    private void Start()
    {
        rain = GetComponent<Rain>();
        Instance = this;
        _lightningResource = LightningCapacity;
    }

    private void Update()
    {
        if (GameManager.Instance.IsPaused || !GameManager.Instance.InPlay) return;

        Vector3 direction = GetInputDirection();

        if (direction == Vector3.zero)
        {
            _CurrentSpeed = Mathf.Lerp(_CurrentSpeed, 0, Time.deltaTime * AceelerationMultiplier);
        }
        else
        {
            bool isFastSpeed = CheckKey(speedKey);
            float targetSpeed = 0.0f;
            if (isFastSpeed && rain.CanRain())
            {
                targetSpeed = FastSpeed;
                rain.UseWater(false);
            }
            else
            {
                targetSpeed = Speed;
            }
            _CurrentSpeed = Mathf.Lerp(_CurrentSpeed, targetSpeed, Time.deltaTime * AceelerationMultiplier);
            _CurrentDirection = Vector3.Lerp(_CurrentDirection, CameraTransform.TransformDirection(direction).normalized, Time.deltaTime * AceelerationMultiplier);
        }

        transform.position += _CurrentDirection * _CurrentSpeed * Time.deltaTime;
        transform.position = ClampPosition(transform.position);
        
        rain.UpdateRainParticles(_CurrentDirection * (_CurrentSpeed / FastSpeed));
        PowerupSpawner.CheckSpawn();

        if (CheckKey(rainKey))
            rain.EnableRain(!rain.IsRaining());
        
        if (CheckKey(rainOn))
            rain.EnableRain(true);
        
        if (CheckKey(rainOff))
            rain.EnableRain(false);

        if (CheckKey(lightningKey))
            ActivateLightning();

        BaseTile previousSelectedTile = _selectedTile;

        Vector3 position = transform.position;
        position.y += Collider.center.y + Collider.radius;
        if (Physics.Raycast( position, Vector3.down, out RaycastHit hit) &&
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

        if (CheckKey(Left))
        {
            direction.x -= 1;
        }
        
        if (CheckKey(Right))
        {
            direction.x += 1;
        }
        
        if (CheckKey(Backward))
        {
            direction.z -= 1;
        }
        
        if (CheckKey(Forward))
        {
            direction.z += 1;
        }
        
        if (CheckKey(Down))
        {
            direction.y -= 1;
        }
        
        if (CheckKey(Up))
        {
            direction.y += 1;
        }
        
        return direction;
    }

    public bool CheckKey(List<InputKey> inputKeys)
    {
        foreach (InputKey inputKey in inputKeys)
        {
            switch (inputKey.Type)
            {
                case KeyType.Down:
                    if (Input.GetKey(inputKey.Key))
                    {
                        return true;
                    }
                    break;
                case KeyType.Pressed:
                    if (Input.GetKeyDown(inputKey.Key))
                    {
                        return true;
                    }
                    break;
                case KeyType.Released:
                    if (Input.GetKeyUp(inputKey.Key))
                    {
                        return true;
                    }
                    break;
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
        if (_lightningResource < LightningUseRate)
        {
            return;
        }

        _lightningResource -= LightningUseRate;
        
        AudioManager.instance.PlaySound("Thunder");
        lightning.Play();
        IsLightning = true;

        UpdateLightningResource?.Invoke(_lightningResource, LightningCapacity);
    }

    public void AddLightning(float lightningAmount)
    {
        _lightningResource += lightningAmount;

        if (_lightningResource > LightningCapacity)
        {
            _lightningResource = LightningCapacity;
        }

        UpdateLightningResource?.Invoke(_lightningResource, LightningCapacity);
    }
}
