using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class RainManager : MonoBehaviour
{
    public static RainManager Instance;
    
    public TextMeshProUGUI rainCount;
    public int dropCount;
    public List<RollingBall> rainDrops = new();
    
    [SerializeField] private GameObject rainDropPrefab;
    [SerializeField] private Vector2 spawnIntervalRange = Vector2.one * 10;
    private const int MaxDropCount = 200;
    private bool _limitReached;
    private bool _rainActive;
    
    private void Awake()
    {
        Instance = this;
    }

    private void FixedUpdate()
    {
        if (!_rainActive)
            return;
        // spawn a raindrop every 0.1 seconds using fixed time
        if (Time.fixedTime % 0.1f < Time.fixedDeltaTime)
        {
            SpawnRainDrop();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 size = new Vector3(spawnIntervalRange.x * 2, 0.3f, spawnIntervalRange.y * 2);
        Gizmos.DrawWireCube(transform.position, size);
    }

    public void SetRaining(bool raining)
    {
        _rainActive = raining;
        if (raining)
            _limitReached = false;
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
        if (_limitReached)
        {
            SetRaining(false);
            RainButton.Instance.LimitReached();
            return;
        }
        if (dropCount >= MaxDropCount)
        {
            _limitReached = true;
            return;
        }
        var rainDrop = Instantiate(rainDropPrefab, GetRandomPosition(), Quaternion.identity);
        rainDrops.Add(rainDrop.GetComponent<RollingBall>());
        dropCount++;
        rainCount.text = dropCount.ToString();
    }
}