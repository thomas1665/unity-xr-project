//Attach this script to your camera object to enable first-person camera movement.

using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public float sensX;
    public float sensY;

    float xRotation;
    float yRotation;

    private void Start()
    {
        // Lock the cursor to the center of the screen and hide it.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Get mouse input for camera rotation.
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        // Update rotation values based on the mouse input.
        yRotation += mouseX;
        xRotation += mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply the new rotation to the camera.
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
    }
}
