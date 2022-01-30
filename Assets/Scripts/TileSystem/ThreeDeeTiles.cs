using System.Collections;
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

    public List<TilePairBridge> Tiles;

    private Tilemap _TilemapRef;
    private List<GameplayTile> _gameplayTiles = new List<GameplayTile>();

    // Start is called before the first frame update
    void Start()
    {
        _TilemapRef = GetComponent<Tilemap>();

        InstantiatePrefabsFromTilemap();

        // Because we aren't really displaying the tilemap we can get rid of it.
        TilemapRenderer renderer = GetComponent<TilemapRenderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
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
                                GameplayTile newTile = newObject.GetComponent<GameplayTile>();
                                if (newTile != null)
                                {
                                    _gameplayTiles.Add(newTile);
                                    newTile.ChangeStateByIdx(bridge.InitialState);
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
    }
}
