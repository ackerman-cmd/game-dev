using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private Transform facingVisual;
    [SerializeField] private float facingMinSpeed = 0.15f;

    private Rigidbody _rigidbody;
    private InputAction _moveAction;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        var playerInput = GetComponent<PlayerInput>();
        _moveAction = playerInput.actions.FindActionMap("Player").FindAction("Move");
    }

    private void FixedUpdate()
    {
        if (_moveAction == null)
            return;

        Vector2 input = _moveAction.ReadValue<Vector2>();
        Vector3 move = new Vector3(input.x, 0f, input.y);
        if (move.sqrMagnitude > 1f)
            move.Normalize();

        Vector3 targetVelocity = move * moveSpeed;
        Vector3 v = _rigidbody.linearVelocity;
        _rigidbody.linearVelocity = new Vector3(targetVelocity.x, v.y, targetVelocity.z);
    }

    private void LateUpdate()
    {
        if (facingVisual == null)
            return;

        Vector3 planar = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);
        if (planar.sqrMagnitude < facingMinSpeed * facingMinSpeed)
            return;

        facingVisual.rotation = Quaternion.LookRotation(planar.normalized, Vector3.up);
    }
}
