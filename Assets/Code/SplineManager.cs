using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineManager : MonoBehaviour
{
    public static SplineManager Instance;
    
    [SerializeField] private GameObject splinePrefab;
    
    private List<Spline> _splines = new();
    private List<RollingBall> _rainDrops = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _rainDrops = RainManager.Instance.rainDrops;
    }

    public void CreateSpline(List<Vector3> controlPoints)
    {
        var Spline = Instantiate(splinePrefab, Vector3.zero, Quaternion.identity);
        Spline.GetComponent<Spline>().SetControlPoints(controlPoints);
    }
}
