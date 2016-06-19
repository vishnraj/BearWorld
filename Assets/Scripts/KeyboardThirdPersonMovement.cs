using UnityEngine;
using System.Collections;

public class KeyboardThirdPersonMovement : MonoBehaviour
{
    public GameObject main_camera;
    float movementSpeed = .1f;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            angleFromCameraToMove(transform.forward, main_camera.transform.forward);
        }

        if (Input.GetKey(KeyCode.S))
        {
            angleFromCameraToMove(transform.forward, -main_camera.transform.forward);
        }

        if (Input.GetKey(KeyCode.A))
        {
            angleFromCameraToMove(transform.forward, -main_camera.transform.right);
        }

        if (Input.GetKey(KeyCode.D))
        {
            angleFromCameraToMove(transform.forward, main_camera.transform.right);
        }
    }

    void angleFromCameraToMove(Vector3 bear, Vector3 camera)
    {
        Vector3 current_pos = transform.position;
        float rotate_angle = 0f;

        Vector3 cameraPlaneProjection = new Vector3(camera.x, 0, camera.z);
        rotate_angle = Vector3.Angle(bear, cameraPlaneProjection);

        if (rotate_angle > 10f)
        {
            transform.Rotate(new Vector3(0, 1, 0), rotate_angle);
        }


        current_pos += movementSpeed * transform.forward;
        transform.position = current_pos;
    }
}
