using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRadius = 2.5f;

    private InputAction _attackAction;

    private void Start()
    {
        var playerInput = GetComponent<PlayerInput>();
        _attackAction = playerInput.actions.FindActionMap("Player").FindAction("Attack", throwIfNotFound: false);
        if (_attackAction == null)
            Debug.LogWarning("PlayerCombat: действие 'Attack' не найдено в PlayerControls. Проверьте ассет и карту Player.");
    }

    private void Update()
    {
        if (_attackAction == null)
            return;
        if (_attackAction.WasPressedThisFrame())
            DoMeleeAttack();
    }

    private void DoMeleeAttack()
    {
        var hits = Physics.OverlapSphere(transform.position, attackRadius, ~0, QueryTriggerInteraction.Collide);
        foreach (var c in hits)
        {
            var enemy = c.GetComponent<Enemy>();
            if (enemy != null)
                enemy.TakeDamage(attackDamage);
        }
    }
}
