using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class RainManager : MonoBehaviour
{
    public static RainManager Instance;
    
    [SerializeField] private GameObject rainDropPrefab;
    [SerializeField] private Vector2 spawnIntervalRange = Vector2.one * 10;
    
    public TextMeshProUGUI rainCount;
    public int dropCount;
    public int maxDropCount = 1000;
    public bool limitReached;
    
    private void Awake()
    {
        Instance = this;
    }

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
        if (limitReached)
            return;
        if (dropCount >= maxDropCount)
        {
            limitReached = true;
            return;
        }
        Instantiate(rainDropPrefab, GetRandomPosition(), Quaternion.identity);
        // Destroy(rainDrop, 5f);
        dropCount++;
        rainCount.text = dropCount.ToString();
    }
}