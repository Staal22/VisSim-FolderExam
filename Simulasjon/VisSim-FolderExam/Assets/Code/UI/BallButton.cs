using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class BallButton : MonoBehaviour
{
    public static BallButton Instance;
    
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Camera mainCamera;
    
    public TextMeshProUGUI textElement;
    public int ballCount;

    [SerializeField] private Texture2D cursorTexture;
    
    private bool _holdingBall;

    private void Awake()
    {
        Instance = this;
        textElement = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void PickBall()
    {
        if (ballCount >= 10)
        {
            textElement.text = "Maks antall baller nådd";
            return;
        }
        // change cursor to ball
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
        textElement.text = "Trykk på overflaten for å plassere ballen";
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
                Instantiate(ballPrefab, hit.point + new Vector3(0,30,0), Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("Kunne ikke detektere overflate, prøv å zoome ut");
            }
            // change cursor back to arrow
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            textElement.text = "Plukk opp ball";
            _holdingBall = false;
            ballCount++;
        }
    }

}