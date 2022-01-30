using System.Collections.Generic;
using UnityEngine;

public class GameplayTile : BaseTile
{
    public Collider Collider;
    public int CurrentState = 0;
    public GameObject[] StateObjects;

    private List<GameplayTile> _neighbors = new List<GameplayTile>();

    private const float _neighborDistance = 10f;
    private readonly float[] _neighborAngles =
    {
        0f,
        60f,
        120f,
        180f,
        240f,
        300f
    };

    private void OnEnable()
    {
        ChangeStateByIdx(0);
    }

    public void ChangeStateByIdx(int inIdx)
    {
        CurrentState = inIdx;

        int count = StateObjects.Length;
        for (int idx = 0; idx < count; ++idx)
        {
            StateObjects[idx].SetActive(inIdx == idx ? true : false);
        }
    }

    public void FindNeighbors()
    {
        Collider.enabled = false;
        
        Vector3 position = transform.position;
        foreach (float angle in _neighborAngles)
        {
            Vector3 angleVector = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), 0f, Mathf.Sin(Mathf.Deg2Rad * angle));
            bool foundNeighbor = Physics.Raycast(position, angleVector, out RaycastHit hit, _neighborDistance);

            if (foundNeighbor &&
                hit.collider != null)
            {
                GameplayTile neighborTile = hit.collider.GetComponent<GameplayTile>();

                if (neighborTile != null)
                {
                    _neighbors.Add(neighborTile);
                }
            }
        }
        
        Collider.enabled = true;
    }
}
