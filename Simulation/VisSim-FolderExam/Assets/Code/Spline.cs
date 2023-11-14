using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spline : MonoBehaviour
{
    private List<Vector3> _controlPoints = new();
    private LineRenderer _lineRenderer;
    private TriangleSurface _triangleSurface;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.numCornerVertices = 3;
    }

    private void OnEnable()
    {
        _triangleSurface = TriangleSurface.Instance;
        if (_triangleSurface == null)
        {
            Debug.LogError("TriangleSurface instance is null");
        }
    }

    public void SetControlPoints(List<KeyValuePair<int, Vector2>> controlPoints)
    {
        if (controlPoints.Count < 3)
        {
            Debug.LogWarning("At least 3 control points are required for B-Spline, destroying spline.");
            Destroy(gameObject);
        }
        // var heightAdjustedControlPoints = new List<Vector3>();
        // foreach (var point in controlPoints)
        // {
        //     // get y value from barycentric coordinates using triangle id
        //     heightAdjustedControlPoints.Add(new Vector3(point.Value.x,
        //         _triangleSurface.Triangles[point.Key].HeightAtPoint(point.Value), point.Value.y));
        // }
        
        foreach (var point in controlPoints)
        {
            Debug.DrawLine(new Vector3(point.Value.x, 0, point.Value.y),
                new Vector3(point.Value.x, 0, point.Value.y) + Vector3.up * 1, Color.red,
                100);
        }
        
        // Calculate the knot vector
        List<float> knotVector = new List<float>();
        for (int i = 0; i < controlPoints.Count + 3; i++)
        {
            knotVector.Add(i);
        }

        const float step = 0.01f;
        List<Vector3> positions = new List<Vector3>();

        // Calculate the positions
        for (float t = 2; t <= controlPoints.Count; t += step)  // the t starts from 2 and ends at controlPoints.Count
        {
            var position = Vector3.zero;
            for (int i = 0; i < controlPoints.Count; i++)
            {
                float basis = BSplineBasis(i, 2, t, knotVector);
                position += basis * new Vector3(controlPoints[i].Value.x, 0, controlPoints[i].Value.y);
                // get the adjusted y-value for each point in the line renderer according to terrain
                position += basis * new Vector3(0,
                    _triangleSurface.Triangles[controlPoints[i].Key].HeightAtPoint(controlPoints[i].Value) + 0.1f,
                    0);
            }

            
            positions.Add(position);
        }
        
        _lineRenderer.positionCount = positions.Count;
        _lineRenderer.SetPositions(positions.ToArray());
    }
    
    private static float BSplineBasis(int i, int degree, float t, IReadOnlyList<float> knots)
    {
        if (degree == 0)
        {
            if (knots[i] <= t && t < knots[i+1]) return 1.0f;
            
            return 0f;
        }
        
        float a = (t - knots[i]) / (knots[i + degree] - knots[i]) * BSplineBasis(i, degree - 1, t, knots);
        float b = (knots[i + degree + 1] - t) / (knots[i + degree + 1] - knots[i + 1]) * BSplineBasis(i + 1, degree - 1, t, knots);
        return a + b;
        
    }
}