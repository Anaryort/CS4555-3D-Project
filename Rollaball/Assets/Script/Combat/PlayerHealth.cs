using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Stats")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Respawn Settings")]
    public float deathY = -20f;       // Fall below this Y to trigger respawn
    public Transform respawnPoint;    // Set in Inspector
    public float respawnDelay = 2f;   // Time before respawning

    private Vector3 _spawnPos;
    private Quaternion _spawnRot;

    [Header("UI")]
    public Slider healthBar;   // Optional: assign in Inspector if you want a UI bar

    private Rigidbody rb;
    private Collider col;
    private PlayerController controller;

    private void Start()
    {
        currentHealth = maxHealth;

        if (healthBar != null)
            healthBar.maxValue = maxHealth;

        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        controller = GetComponent<PlayerController>();

        _spawnPos = respawnPoint ? respawnPoint.position : transform.position;
        _spawnRot = respawnPoint ? respawnPoint.rotation : transform.rotation;
    }

    private void Update()
    {
        // Check death by falling
        if (rb != null && rb.position.y < deathY && currentHealth > 0)
        {
            TakeDamage(maxHealth); // kill player on fall
        }
    }

    // Called by enemies when they deal damage
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth < 0)
            currentHealth = 0;

        if (healthBar != null)
            healthBar.value = currentHealth;

        Debug.Log("Player took " + damageAmount + " damage. Current Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player Died!");

        // disable movement + physics + collider
        if (controller != null) controller.enabled = false;
        if (rb != null) rb.isKinematic = true;
        if (col != null) col.enabled = false;

        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        // reset health
        currentHealth = maxHealth;
        if (healthBar != null)
            healthBar.value = currentHealth;

        // reset position/rotation
        rb.position = _spawnPos;
        rb.rotation = _spawnRot;

        // reset velocity
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // re-enable movement + physics + collider
        if (controller != null) controller.enabled = true;
        if (rb != null) rb.isKinematic = false;
        if (col != null) col.enabled = true;

        Debug.Log("Player Respawned!");
    }

    public void Heal(int healAmount)
    {
        currentHealth += healAmount;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        if (healthBar != null)
            healthBar.value = currentHealth;

        Debug.Log("Player healed " + healAmount + ". Current Health: " + currentHealth);
    }
}
