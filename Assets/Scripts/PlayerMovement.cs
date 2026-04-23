using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private Transform facingVisual;

    private Rigidbody _rigidbody;
    private InputAction _moveAction;
    private bool _isGrounded;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        var playerInput = GetComponent<PlayerInput>();
        _moveAction = playerInput.actions.FindActionMap("Player").FindAction("Move");

        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        _rigidbody.freezeRotation = true;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && _isGrounded)
        {
            Vector3 v = _rigidbody.linearVelocity;
            _rigidbody.linearVelocity = new Vector3(v.x, 0f, v.z);
            _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            _isGrounded = false;
        }
    }

    private void FixedUpdate()
    {
        if (_moveAction == null)
            return;

        RaycastHit[] hits = Physics.SphereCastAll(transform.position + Vector3.up * 0.5f, 0.4f, Vector3.down, 0.8f);
        _isGrounded = false;
        RaycastHit groundHit = default;
        float highestY = -999f;

        foreach (var h in hits)
        {
            if (!h.collider.isTrigger && h.collider.gameObject != gameObject && !h.collider.transform.IsChildOf(transform))
            {
                if (h.point.y > highestY)
                {
                    highestY = h.point.y;
                    _isGrounded = true;
                    groundHit = h;
                }
            }
        }

        Vector2 input = _moveAction.ReadValue<Vector2>();
        Vector3 move;
        if (GameplayModeController.Instance != null && GameplayModeController.Instance.IsFirstPerson)
            move = GameplayModeController.Instance.GetFirstPersonPlanarMove(input);
        else
        {
            move = new Vector3(input.x, 0f, input.y);
            if (move.sqrMagnitude > 1f)
                move.Normalize();
        }

        var speedUp = GetComponent<PlayerSpeedUp>();
        float currentSpeed = moveSpeed * (speedUp != null ? speedUp.Multiplier : 1f);

        Vector3 targetVelocity = move * currentSpeed;

        if (_isGrounded)
        {
            if (move.sqrMagnitude < 0.01f)
            {
                _rigidbody.linearVelocity = Vector3.zero;
            }
            else
            {
                Vector3 slopeMove = Vector3.ProjectOnPlane(targetVelocity, groundHit.normal);

                if (slopeMove.y > 0f)
                    slopeMove.y += 3f;

                _rigidbody.linearVelocity = slopeMove;
            }
        }
        else
        {
            Vector3 v = _rigidbody.linearVelocity;
            _rigidbody.linearVelocity = new Vector3(targetVelocity.x, v.y, targetVelocity.z);
        }
    }

    private void LateUpdate()
    {
        if (facingVisual == null)
            return;

        if (GameplayModeController.Instance != null && GameplayModeController.Instance.IsFirstPerson)
        {
            Vector3 f = GameplayModeController.Instance.GetFirstPersonPlanarForward();
            facingVisual.rotation = Quaternion.LookRotation(f, Vector3.up);
            return;
        }

        if (!MouseGroundUtility.TryGetMouseGroundPoint(out Vector3 ground))
            return;

        Vector3 forward = ground - transform.position;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f)
            return;

        facingVisual.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
    }
}
