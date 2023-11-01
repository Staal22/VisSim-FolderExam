using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spline : MonoBehaviour
{
    private List<Vector3> _controlPoints = new();
    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    public void SetControlPoints(List<Vector3> controlPoints)
    {
        foreach (var point in controlPoints)
        {
            Debug.DrawLine(point, point + Vector3.up * 1, Color.red, 100);
        }
        
        if (controlPoints.Count < 3) return; // At least 3 points are required for B-Spline
  
        // Calculate the knot vector
        List<float> knotVector = new List<float>();
        for (int i = 0; i < controlPoints.Count + 3; i++)
        {
            knotVector.Add(i);
        }

        const float step = 0.01f;
        List<Vector3> positions = new List<Vector3>();

        // Calculate the positions
        for (float t = 2; t <= controlPoints.Count; t += step)  // the t starts from 2 and ends at controlPoints.Count.
        {
            Vector3 position = Vector3.zero;
            for (int i = 0; i < controlPoints.Count; i++)
            {
                float basis = BSplineBasis(i, 2, t, knotVector);
                position += basis * controlPoints[i];
            }
            positions.Add(position);
        }

        _lineRenderer.positionCount = positions.Count;
        _lineRenderer.SetPositions(positions.ToArray());
    }
    
    private float BSplineBasis(int i, int degree, float t, List<float> knots)
    {
        if (degree == 0)
        {
            if (knots[i] <= t && t < knots[i+1]) return 1.0f;
            else return 0f;
        }
        else
        {
            float a = ((t - knots[i]) / (knots[i + degree] - knots[i])) * BSplineBasis(i, degree - 1, t, knots);
            float b = ((knots[i + degree + 1] - t) / (knots[i + degree + 1] - knots[i + 1])) * BSplineBasis(i + 1, degree - 1, t, knots);
            return a + b;
        }
    }
}
