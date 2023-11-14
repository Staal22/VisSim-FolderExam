using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class RollingBall : MonoBehaviour
{
    public bool isRainDrop;   
    public UnityEvent onRollingBallDestruction;

    [SerializeField] private GameObject waterBodyPrefab;
    private const float Mass = 1;
    private static readonly Vector3 Gravity = Physics.gravity * Mass;
    private const float VelocityThreshold = 0.1f;
    
    private readonly List<Vector3> _controlPoints = new();
    private Action<List<Vector3>> _onBecomeWaterBody;
    private int _timeStep;
    private TriangleSurface _triangleSurface;
    private Vector3 _oldVelocity = Vector3.zero;
    private int _triangleID = -1;
    private int _nextTriangleID;
    private float _radius;
    private float _height;
    private float _initTime;
    private bool _rolling;
    private bool _rollingDown;
    private bool _floating;

    private void Awake()
    {
        _radius = gameObject.transform.localScale.x / 2;
    }

    private void Start()
    {
        _initTime = Time.fixedTime;
        _triangleSurface = TriangleSurface.Instance;
        _onBecomeWaterBody += SplineManager.Instance.CreateSpline;
    }

    private void FixedUpdate()
    {
        _timeStep++;
        
        var unitNormal = Vector3.zero;
        var triangles = _triangleSurface.Triangles;
        
        _triangleID = _triangleSurface.FindTriangle(transform.position, _triangleID);
        if (_triangleID != -1)
        {
            unitNormal = triangles[_triangleID].Normal;
            Vector3 point = triangles[_triangleID].Vertices[0];
            var p = transform.position - point;
            var y = Vector3.Dot(p, unitNormal) * unitNormal;
            _rolling = y.magnitude < _radius;
        }
        else
        {
            if (transform.position.y < 300)
            {
                Destroy(gameObject);
            }
        }
        
        var surfaceNormal = -Vector3.Dot(Gravity, unitNormal) * unitNormal;
        var force = Gravity + surfaceNormal;
        Vector3 acceleration;
        if (_rolling)
            acceleration = force / Mass;
        else
            acceleration = Gravity / Mass;
        
        var velocity = _oldVelocity + acceleration * Time.fixedDeltaTime;

        if (_rolling)
        {
            // friction
            var friction = -velocity * 0.01f;
            velocity += friction;
            
            _rollingDown = Vector3.Dot(velocity, unitNormal) < 0;
            if (_rollingDown && velocity.y < -5)
            {
                velocity = new Vector3(velocity.x, 0, velocity.z);
            }
        }
        else
        {
            _rollingDown = false;
        }
        
        var position = transform.position + velocity * Time.fixedDeltaTime;

        if (_rolling)
        {
            if (_triangleID != -1)
            {
                if (_timeStep >= 50)
                {
                    _timeStep = 0;
                    _controlPoints.Add(new Vector3(transform.position.x, /*0.0f*/transform.position.y , transform.position.z));
                }

                // Only correct position when rolling into the surface
                if (_rollingDown && !_floating)
                {
                    _height = triangles[_triangleID].HeightAtPoint(position);
                }
            }
        }
        // magic number because barycentric does not account for correct point of the ball when projecting ball onto the triangle
        if (_rollingDown && !_floating)
            transform.position = new Vector3(position.x, _height + _radius - 0.3f , position.z);
        else if (_floating)
            transform.position = new Vector3(position.x, _height + _radius - 0.1f, position.z);
        else
        {
            transform.position = new Vector3(position.x, position.y, position.z);
        }
        _oldVelocity = velocity;
        
        if (Math.Abs(_oldVelocity.x) < VelocityThreshold && Math.Abs(_oldVelocity.z) < VelocityThreshold && isRainDrop && _rolling && Time.fixedTime - _initTime > 10f)
        {
            BecomeWaterBody();
        }
    }

    public void DoFloat(float worldHeight)
    {
        _floating = true;
        _height = worldHeight;
    }

    public void StopFloating()
    {
        _floating = false;
    }
    
    public void BecomeWaterBody(bool merge = false)
    {
        if (!merge)
        {
            Instantiate(waterBodyPrefab, transform.position, Quaternion.identity);
        }
        _onBecomeWaterBody?.Invoke(_controlPoints);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (isRainDrop)
        {
            Debug.LogWarning("Ødelegger regndråpe");
            RainManager.Instance.dropCount--;
            RainManager.Instance.rainCount.text = RainManager.Instance.dropCount.ToString();
        }
        else
        {
            Debug.LogWarning("Ødelegger ball");
            BallButton.Instance.ballCount--;
            if (BallButton.Instance.textElement.text == "Maks antall baller nådd")
                BallButton.Instance.textElement.text = "Plukk opp ball";
        }
        onRollingBallDestruction?.Invoke();
    }
}
