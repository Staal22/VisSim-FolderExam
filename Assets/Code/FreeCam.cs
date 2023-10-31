using UnityEngine;

/// <summary>
///     I did not write this. I found it here: https://gist.github.com/ashleydavis/f025c03a9221bc840a2b
///     Honestly, it's very inefficient and the code makes me cry, but it works fine.
///     I added some slight performance improvements and refactored the code to be improve readability.
/// </summary>
public class FreeCam : MonoBehaviour
{
    /// <summary>
    ///     Normal speed of camera movement.
    /// </summary>
    public float movementSpeed = 10f;

    /// <summary>
    ///     Speed of camera movement when shift is held down,
    /// </summary>
    public float fastMovementSpeed = 100f;

    /// <summary>
    ///     Sensitivity for free look.
    /// </summary>
    public float freeLookSensitivity = 3f;

    /// <summary>
    ///     Amount to zoom the camera when using the mouse wheel.
    /// </summary>
    public float zoomSensitivity = 10f;

    /// <summary>
    ///     Amount to zoom the camera when using the mouse wheel (fast mode).
    /// </summary>
    public float fastZoomSensitivity = 50f;

    /// <summary>
    ///     Set to true when free looking (on right mouse button).
    /// </summary>
    private bool looking;

    private void Update()
    {
        var fastMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        var speed = fastMode ? fastMovementSpeed : movementSpeed;
        var transform1 = transform;
        
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) transform1.position += -transform.right * (speed * Time.deltaTime);

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) transform1.position += transform.right * (speed * Time.deltaTime);

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) transform1.position += transform.forward * (speed * Time.deltaTime);

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) transform1.position += -transform.forward * (speed * Time.deltaTime);

        if (Input.GetKey(KeyCode.Q)) transform1.position += transform.up * (speed * Time.deltaTime);

        if (Input.GetKey(KeyCode.E)) transform1.position += -transform.up * (speed * Time.deltaTime);

        if (Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.PageUp)) transform.position += Vector3.up * (speed * Time.deltaTime);

        if (Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.PageDown)) transform.position += -Vector3.up * (speed * Time.deltaTime);

        if (looking)
        {
            var rotation1 = transform1.localEulerAngles;
            var newRotationX = rotation1.y + Input.GetAxis("Mouse X") * freeLookSensitivity;
            var newRotationY = rotation1.x - Input.GetAxis("Mouse Y") * freeLookSensitivity;
            transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
        }

        var axis = Input.GetAxis("Mouse ScrollWheel");
        if (axis != 0)
        {
            var sensitivity = fastMode ? fastZoomSensitivity : zoomSensitivity;
            transform1.position += transform.forward * (axis * sensitivity);
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
            StartLooking();
        else if (Input.GetKeyUp(KeyCode.Mouse1)) StopLooking();
    }

    private void OnDisable()
    {
        StopLooking();
    }

    /// <summary>
    ///     Enable free looking.
    /// </summary>
    public void StartLooking()
    {
        looking = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    ///     Disable free looking.
    /// </summary>
    public void StopLooking()
    {
        looking = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}