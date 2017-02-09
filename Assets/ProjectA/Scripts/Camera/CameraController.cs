using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public Transform lookAt;
    public Transform targetPosition;
    public Transform player;

    public Transform LeftRotationAxis;
    public Transform RightRotationAxis;

    private bool smooth = true;
    private float smoothSpeed = 0.06f;
    private float smoothLookSpeed = 0.045f;
    private Vector3 offset = new Vector3(0.4f, 2.85f, -5f);
    private Vector3 _cameraLocalEulerAngles = new Vector3(0f, 0f, 0f);

    private Vector3 leftRotationAxisOffset  = new Vector3(-5f, 0f, 0f);
    private Vector3 rightRotationAxisOffset = new Vector3(5f, 0f, 0f);

    void Start ()
    {
        offset = GvrViewer.Instance == null ? new Vector3(0, 2.85f, -5f) : new Vector3(0f, 2.85f, -5f);

        offset = targetPosition.position - player.position;
        transform.position    = offset + lookAt.position;
        transform.eulerAngles = _cameraLocalEulerAngles;
    }
    void LateUpdate()
    {
        LeftRotationAxis.position  = player.transform.position + (player.right * leftRotationAxisOffset.x);
        RightRotationAxis.position = player.transform.position + (player.right * rightRotationAxisOffset.x);

        Vector3 desirePosition = /*targetPosition.position;//*/player.transform.position + offset;
        transform.position = smooth ? Vector3.Lerp(transform.position, desirePosition, smoothSpeed) : desirePosition;
        
        if (GvrViewer.Instance == null)
        {
            Vector3 desireLookAt = player.position + (player.forward * 1f) + new Vector3(0, 1.5f, 0);
            lookAt.position = smooth ? Vector3.Lerp(lookAt.position, desireLookAt, smoothLookSpeed) : desireLookAt;
            transform.LookAt(lookAt.position);
        }
    }
}
