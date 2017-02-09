using UnityEngine;
using System.Collections;

public class CameraPivot : MonoBehaviour
{
    public Transform VRcam;
    void Start ()
    {
	
	}

    void Update()
    {

        if (GvrViewer.Instance != null)
        {
            if (GvrViewer.Instance.Triggered)
            {
                Debug.Log("Recenter()");
                GvrViewer.Instance.Recenter();
            }
        }
    }
	
    public void Recenter()
    {
        transform.localRotation = Quaternion.Inverse(VRcam.rotation);
    }
}
