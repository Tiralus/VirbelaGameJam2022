using System.Collections.Generic;
using UnityEngine;

public class PowerupSpawner : MonoBehaviour
{
    public static PowerupSpawner Instance;
    public List<GameObject> Prefabs;
    public float SpawningCooldown;
    public int MaxPowerups;
    public float Height;

    private List<Powerup> _powerupInstances = new List<Powerup>();
    private float _spawningCooldown;

    private void Awake()
    {
        Instance = this;
    }

    public void CollectPowerup(Powerup powerup)
    {
        int index = _powerupInstances.IndexOf(powerup);

        if (index != -1)
        {
            _powerupInstances.RemoveAt(index);
            powerup.Execute();
            Destroy(powerup.gameObject);
        }
    }

    public void CheckSpawn()
    {
        if (_spawningCooldown > 0f)
        {
            _spawningCooldown -= Time.deltaTime;
            return;
        }

        _spawningCooldown = SpawningCooldown;

        if (_powerupInstances.Count < MaxPowerups)
        {
            BaseTile randomTile = ThreeDeeTiles.Instance.GetRandomTile();

            if (randomTile != null)
            {
                Vector3 position = randomTile.transform.position;
                position.y = Height;
                GameObject randomPrefab = Prefabs[Random.Range(0, Prefabs.Count)];
                GameObject powerupGameObject = Instantiate(randomPrefab, position, Quaternion.identity);
                Powerup powerup = powerupGameObject.GetComponent<Powerup>();
                
                if (powerup != null)
                {
                    _powerupInstances.Add(powerup);
                }
            }
        }
    }
}
