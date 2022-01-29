using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameplayTile : MonoBehaviour
{
    public int CurrentState = 0;
    public GameObject[] StateObjects;

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
}
