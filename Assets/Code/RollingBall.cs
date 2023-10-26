using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RollingBall : MonoBehaviour
{
    private TriangleSurface _triangleSurface;
    private const float Mass = 1;
    private int _triangleID;
    private int _nextTriangleID;
    private float _radius;
    private bool _rolling;
    private bool _rollingDown;
    private float _height;
    private Vector3 _oldVelocity = Vector3.zero;

    private void Awake()
    {
        _radius = gameObject.transform.localScale.x / 2;
    }

    private void Start()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down * 300, out hit))
        {
            _triangleSurface = hit.collider.gameObject.GetComponent<TriangleSurface>();
            // move down to the surface
            transform.position = hit.point;
        }
        else
        {
            Debug.LogWarning("No triangle surface found, destroying self");
            BallButton.Instance.ballCount--;
            BallButton.Instance.textElement.text = "Plukk opp ball";
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (_triangleSurface == null)
            return;
        
        _rollingDown = false;
        var unitNormal = Vector3.zero;
        var triangles = _triangleSurface.Triangles;
        
        var gravity = Physics.gravity * Mass;
        
        _triangleID = _triangleSurface.FindTriangle(transform.position);
        if (_triangleID != -1)
        {
            unitNormal = triangles[_triangleID].Normal;
            
            // Debug.Log("Triangle: " + _triangleID);
            
            Vector3 point = triangles[_triangleID].Vertices[0];
            var p = transform.position - point;
            var y = Vector3.Dot(p, unitNormal) * unitNormal;
            _rolling = y.magnitude < _radius;
            // Debug.Log("Rolling: " + _rolling);
        }
        else
        {
            if (transform.position.y < 300)
            {
                Debug.LogWarning("Falt av planet, ødelegger ball");
                BallButton.Instance.ballCount--;
                if (BallButton.Instance.textElement.text == "Maks antall baller nådd")
                    BallButton.Instance.textElement.text = "Plukk opp ball";
                Destroy(gameObject);
            }
            // Debug.LogWarning("No triangle found, destroying self");
            // BallButton.Instance.ballCount--;
            // BallButton.Instance.text.text = "Plukk opp ball";
            // Destroy(gameObject);
        }
        
        var surfaceNormal = -Vector3.Dot(gravity, unitNormal) * unitNormal;
        var force = gravity + surfaceNormal;
        Vector3 acceleration;
        if (_rolling)
            acceleration = force / Mass;
        else
            acceleration = gravity / Mass;
        // Draw debug line for acceleration
        Debug.DrawRay(transform.position, acceleration, Color.red);
        
        var velocity = _oldVelocity + acceleration * Time.fixedDeltaTime;
        // Draw debug line for velocity
        Debug.DrawRay(transform.position, velocity, Color.green);

        if (_rolling)
        {
            var _rollingDown = Vector3.Dot(velocity, unitNormal) < 0;
            if (_rollingDown)
            {
                velocity = new Vector3(velocity.x, 0, velocity.z);
            }
        }
        
        var position = transform.position + velocity * Time.fixedDeltaTime;
        
        // _nextTriangleID = _triangleSurface.FindTriangle(position);
        // if (_nextTriangleID != _triangleID && _nextTriangleID != -1 && _triangleID != -1)
        // {
        //     var nextTriangleNormal = triangles[_nextTriangleID].Normal;
        //     var triangleNormal = triangles[_triangleID].Normal;
        //     var reflectionNormal = (nextTriangleNormal + triangleNormal).normalized;
        //
        //     var crash = Vector3.Cross(nextTriangleNormal, triangleNormal).y > 0;
        //     
        //     if (crash)
        //         velocity -= 2 * Vector3.Dot(velocity, reflectionNormal) * reflectionNormal;
        //     else
        //         _rolling = false;
        // }

        if (_rolling)
        {
            if (_triangleID != -1)
            {
                _rollingDown = Vector3.Dot(velocity, unitNormal) < 0;
                // Calculate the center of the triangle
                Vector3 center = (triangles[_triangleID].Vertices[0] + triangles[_triangleID].Vertices[1] + triangles[_triangleID].Vertices[2]) / 3;
                Debug.DrawLine(center, center + unitNormal, Color.yellow);

                // Don't correct position when not pushing into surface.
                if (_rollingDown)
                {
                    // Find plane height from barycentric coordinates.
                    var barycentricCoordinates = Utilities.Barycentric(
                        triangles[_triangleID].Vertices[0],
                        triangles[_triangleID].Vertices[1],
                        triangles[_triangleID].Vertices[2],
                        position
                    );
        
                    _height = barycentricCoordinates.x * triangles[_triangleID].Vertices[0].y +
                              barycentricCoordinates.y * triangles[_triangleID].Vertices[1].y +
                              barycentricCoordinates.z * triangles[_triangleID].Vertices[2].y;
                }
            }
        }
        // magic number because barycentric does not account for correct point of the ball when projecting ball onto the triangle
        transform.position = _rollingDown ? new Vector3(position.x, _height + _radius - 0.2f , position.z) : position;
        _oldVelocity = velocity;
    }
}
