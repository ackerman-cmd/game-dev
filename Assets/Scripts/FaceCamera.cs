using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera _cam;

    private void Start()
    {
        _cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (_cam == null)
            _cam = Camera.main;
            
        if (_cam != null)
        {
            transform.forward = _cam.transform.forward;
        }
    }
}
