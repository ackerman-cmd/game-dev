using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FaceMovementDirection : MonoBehaviour
{
    [SerializeField] private Transform facingVisual;
    [SerializeField] private float minSpeed = 0.12f;

    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void LateUpdate()
    {
        if (facingVisual == null || _rigidbody == null)
            return;

        Vector3 planar = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);
        if (planar.sqrMagnitude < minSpeed * minSpeed)
            return;

        facingVisual.rotation = Quaternion.LookRotation(planar.normalized, Vector3.up);
    }
}
