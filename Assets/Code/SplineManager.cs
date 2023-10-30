using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineManager : MonoBehaviour
{
    public static SplineManager Instance;
    
    [SerializeField] private GameObject splinePrefab;
    
    private List<RollingBall> _rainDrops = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _rainDrops = RainManager.Instance.rainDrops;
    }
}
