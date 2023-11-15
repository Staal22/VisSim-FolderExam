using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineManager : MonoBehaviour
{
    public static SplineManager Instance;
    
    [SerializeField] private GameObject splinePrefab;

    private void Awake()
    {
        Instance = this;
    }
    
    public void CreateSpline(List<KeyValuePair<int, Vector2>> controlPoints)
    {
        var spline = Instantiate(splinePrefab, Vector3.zero, Quaternion.identity);
        spline.GetComponent<Spline>().SetControlPoints(controlPoints);
    }
}
