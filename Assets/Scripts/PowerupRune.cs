using UnityEngine;

public class PowerupRune : MonoBehaviour
{
    public enum Type { Heal, AttackSpeed, Invincible }
    public Type runeType;

    private float _time;

    private void Update()
    {
        _time += Time.deltaTime;
        transform.Rotate(0f, 90f * Time.deltaTime, 0f, Space.World);
        transform.position += new Vector3(0f, Mathf.Sin(_time * 3f) * 0.005f, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ApplyBuff(other.gameObject);
            Destroy(gameObject);
        }
    }

    private void ApplyBuff(GameObject player)
    {
        switch (runeType)
        {
            case Type.Heal:
                var hp = player.GetComponent<PlayerHealth>();
                if (hp != null) hp.HealFull();
                break;
            case Type.AttackSpeed:
                var combat = player.GetComponent<PlayerCombat>();
                if (combat != null) combat.BuffAttackSpeed(10f);
                break;
            case Type.Invincible:
                var health = player.GetComponent<PlayerHealth>();
                if (health != null) health.MakeInvincible(8f);
                break;
        }
    }
}
