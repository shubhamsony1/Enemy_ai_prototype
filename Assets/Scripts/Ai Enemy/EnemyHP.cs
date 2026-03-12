using UnityEngine;
using UnityEngine.UI;

public class EnemyHP : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 100;
    private int currentHP;

    [Header("UI")]
    public Slider healthSlider;

    [Header("Audio")]
    public AudioSource deathAudio;

    private bool isDead = false;

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

        Debug.Log($"[EnemyHP] Took {damage} damage. Remaining HP: {currentHP}");

        if (currentHP <= 0)
            Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (deathAudio != null)
            deathAudio.Play();

        Debug.Log("[EnemyHP] Enemy died.");

        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        float delay = (deathAudio != null && deathAudio.clip != null) ? deathAudio.clip.length : 0f;
        Destroy(gameObject, delay);
    }
}