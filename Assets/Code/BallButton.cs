using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class BallButton : MonoBehaviour
{
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Camera mainCamera;
    
    private TextMeshProUGUI _text;
    private bool _holdingBall;

    private void Awake()
    {
        _text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void PickBall()
    {
        // change cursor to ball
        Cursor.SetCursor(ballPrefab.GetComponentInChildren<SpriteRenderer>().sprite.texture, Vector2.zero, CursorMode.Auto);
        _text.text = "Trykk på overflaten for å plassere ballen";
        _holdingBall = true;
    }
    
    private void Update()
    {
        if (!_holdingBall)
            return;
        
        if (Input.GetMouseButtonDown(0))
        {
            // place ball
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                Instantiate(ballPrefab, hit.point, Quaternion.identity);
            }
            else
            {
                Debug.LogError("No surface found");
            }
            // change cursor back to arrow
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            _text.text = "Plukk opp ball";
            _holdingBall = false;
        }
    }

}
