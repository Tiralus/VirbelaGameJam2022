using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class ThreeDeeTiles : MonoBehaviour
{
    [System.Serializable]
    public struct TilePairBridge
    {
        public Tile From;
        public GameObject To;
        public int InitialState;
    };

    public static ThreeDeeTiles Instance;
    public Vector3 Min, Max;

    public List<TilePairBridge> Tiles;

    private Tilemap _TilemapRef;
    private List<GameplayTile> _gameplayTiles = new List<GameplayTile>();
    private List<BaseTile> _baseTiles = new List<BaseTile>();

    public static System.Action<float, float> UpdateTiles;

    public float GrassPercentage;
    public float CorruptionPercentage;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        
        _TilemapRef = GetComponent<Tilemap>();

        InstantiatePrefabsFromTilemap();

        // Because we aren't really displaying the tilemap we can get rid of it.
        TilemapRenderer renderer = GetComponent<TilemapRenderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }

        // HACK
        Cloud cloudRef = FindObjectOfType<Cloud>();
        if (cloudRef != null)
        {
            cloudRef.Min = Min;
            cloudRef.Max = Max;
        }
    }

    void InstantiatePrefabsFromTilemap()
    {
        Vector3Int min = _TilemapRef.cellBounds.min;
        Vector3Int max = _TilemapRef.cellBounds.max;
        for (int xIdx = min.x; xIdx < max.x; ++xIdx)
        {
            for (int yIdx = min.y; yIdx < max.y; yIdx++)
            {
                for (int zIdx = min.z; zIdx < max.z; zIdx++)
                {
                    Vector3Int cellPos = new Vector3Int(xIdx, yIdx, zIdx);
                    TileBase tile = _TilemapRef.GetTile(cellPos);
                    if (tile != null)
                    {
                        Vector3 localPos = _TilemapRef.CellToLocal(cellPos);
                        //Debug.LogFormat("Found tile at: {0} local: {1}", cellPos, localPos);
                        foreach (TilePairBridge bridge in Tiles)
                        {
                            if (bridge.From.name.Equals(tile.name))
                            {
                                GameObject newObject = Instantiate(bridge.To, localPos, Quaternion.identity, transform);
                                BaseTile baseTile = newObject.GetComponent<BaseTile>();

                                if (baseTile != null)
                                {
                                    _baseTiles.Add(baseTile);
                                    
                                    if (baseTile is GameplayTile newTile)
                                    {
                                        newTile.OnStateChanged += UpdatePercentages;
                                        _gameplayTiles.Add(newTile);
                                        newTile.ChangeStateByIdx(bridge.InitialState);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        foreach (GameplayTile gameplayTile in _gameplayTiles)
        {
            gameplayTile.FindNeighbors();
        }

        UpdatePercentages();
    }

    private void UpdatePercentages()
    {
        float totalGameTiles = _gameplayTiles.Count;
        int grassTiles = 0;
        int corruptionTiles = 0;

        foreach (GameplayTile tile in _gameplayTiles)
        {
            switch (tile.CurrentState)
            {
                case GameplayTile.corruptionState:
                    ++corruptionTiles;
                    break;
                case GameplayTile.grassState:
                case GameplayTile.treesState:
                    ++grassTiles;
                    break;
            }
        }

        UpdateTiles?.Invoke(grassTiles/totalGameTiles, corruptionTiles/totalGameTiles);
    }

    public BaseTile GetRandomTile()
    {
        if (_baseTiles == null ||
            _baseTiles.Count <= 0)
        {
            return null;
        }

        return _baseTiles[Random.Range(0, _baseTiles.Count)];
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 size = Max - Min;
        Vector3 center = Min + (size / 2f);
        Gizmos.DrawWireCube(center, size);
    }
}
