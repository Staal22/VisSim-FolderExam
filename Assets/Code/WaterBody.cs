using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WaterBody : MonoBehaviour
{
    private float _expandHeight = 0.5f;
    private const float XScale = 0.3f;
    private const float ZScale = 0.3f;
    private void Expand()
    {
        var scale = transform.localScale;
        scale.x += XScale;
        scale.z += ZScale;
        transform.localScale = scale;
        var pos = transform.position;
        pos.y += _expandHeight;
        _expandHeight /= 2;
        transform.position = pos;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        var rollingBall = other.gameObject.GetComponent<RollingBall>();
        if (rollingBall != null && rollingBall.isRainDrop)
        {
            Expand();
            Destroy(rollingBall.gameObject);
            return;

        }
        var waterBody = other.gameObject.GetComponent<WaterBody>();
        if (waterBody != null)
        {
            Expand();
            Destroy(waterBody.gameObject);
        }
        
    }
}
