using UnityEngine;
using UnityEngine.UI;

public class PlayerHP : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 100;
    private int currentHP;

    [Header("UI")]
    public Slider healthSlider;

    [Header("Audio")]
    public AudioSource deathAudio;

    private bool isDead = false;

    public int CurrentHP()
    {
        return currentHP;
    }

    void Start()
    {
        currentHP = maxHP;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHP;
            healthSlider.value = currentHP;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        if (healthSlider != null)
            healthSlider.value = currentHP;

        Debug.Log("[PlayerHP] Took " + damage + " damage. Remaining HP: " + currentHP);

        if (currentHP <= 0)
            Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (deathAudio != null)
            deathAudio.Play();

        Debug.Log("[PlayerHP] Player died!");

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        BulletFire fire = GetComponent<BulletFire>();
        if (fire != null) fire.enabled = false;

        GameOverManager gom = Object.FindFirstObjectByType<GameOverManager>();
        if (gom != null)
            gom.PlayerDied();
        else
            Debug.LogWarning("[PlayerHP] No GameOverManager found in scene!");
    }
}