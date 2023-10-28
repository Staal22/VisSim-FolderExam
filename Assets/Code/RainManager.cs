using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class RainManager : MonoBehaviour
{
    [SerializeField] private GameObject rainDropPrefab;
    [SerializeField] private Vector2 spawnIntervalRange = Vector2.one * 10;
    [SerializeField] private TextMeshProUGUI rainCount;
    
    private int _dropCount;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnRainDrop), 0, 0.1f);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 size = new Vector3(spawnIntervalRange.x * 2, 0.3f, spawnIntervalRange.y * 2);
        Gizmos.DrawWireCube(transform.position, size);
    }

    Vector3 GetRandomPosition()
    {
        float x = Random.Range(-spawnIntervalRange.x, spawnIntervalRange.x);
        float z = Random.Range(-spawnIntervalRange.y, spawnIntervalRange.y);
        
        Vector3 position = transform.position;

        position.x += x;
        position.z += z;

        return position;
    }
    
    void SpawnRainDrop()
    {
        if (_dropCount >= 1000)
            return;
        Instantiate(rainDropPrefab, GetRandomPosition(), Quaternion.identity);
        // Destroy(rainDrop, 5f);
        _dropCount++;
        rainCount.text = _dropCount.ToString();
    }
}