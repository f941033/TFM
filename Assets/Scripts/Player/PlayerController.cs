using UnityEngine;
using DeckboundDungeon.Cards.Buff;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.SceneManagement;
using TMPro;


public class PlayerController : MonoBehaviour
{
    public Animator soulsAnimation;
    private float attackCooldown = 0f;

    private List<EnemyController> targetsInRange = new List<EnemyController>();
    private CircleCollider2D rangeTrigger;
    public TextMeshProUGUI textGold;

    [Header("Base stats")]
    [SerializeField] public float baseHealth { get; set; } = 100f;
    private float baseDamage = 15f;
    private float baseAttackSpeed = 1f;
    private float baseRange = 1f;
    private float baseSoulsRate = 1f;
    [SerializeField] public float maxSouls = 15f;
    public PlayerSoulsBar playerSouls;

    [Header("Current stats")]
    [SerializeField] private float currentHealth;
    [SerializeField] private float currentDamage;
    [SerializeField] private float currentAttackSpeed;
    [SerializeField] private float currentRange;
    [SerializeField] private float currentSouls;
    [SerializeField] private float currentSoulsRate;
    [SerializeField] public int amountGold = 50;
    [SerializeField] public byte soulsBuyPerShop = 0;


    //[Header("Events")]
    public event Action<float> OnSoulsChanged;
    public event Action<float> OnHealthChanged;
    public event Action<int> OnGoldChanged;

    private Animator anim;
    private AudioSource audioSource;

    [Header("Audio")]
    public AudioClip attackSound;


    void Awake()
    {
        currentHealth = baseHealth;
        currentDamage = baseDamage;
        currentAttackSpeed = baseAttackSpeed;
        currentRange = baseRange;
        currentSouls = 6f;
        currentSoulsRate = baseSoulsRate;

        rangeTrigger = GetComponent<CircleCollider2D>();
        rangeTrigger.isTrigger = true;
        rangeTrigger.radius = baseRange;
        currentSouls = maxSouls;

        OnHealthChanged?.Invoke(currentHealth);
        OnSoulsChanged?.Invoke(currentSouls);

        textGold.text = amountGold.ToString();

        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    public void ApplyTemporaryBuff(BuffType buff, float modifier, float duration)
    {

        StartCoroutine(ApplyBuffCoroutine(buff, modifier, duration));
    }

    public IEnumerator ApplyBuffCoroutine(BuffType buff, float modifier, float duration)
    {
        switch (buff)
        {
            case BuffType.Health:
                currentHealth *= modifier;
                break;
            case BuffType.Damage:
                currentDamage *= modifier;
                break;
            case BuffType.AttackSpeed:
                currentAttackSpeed *= modifier;
                break;
            case BuffType.Range:
                currentRange *= modifier;
                rangeTrigger.radius = currentRange;
                break;
        }
        yield return new WaitForSeconds(duration);

        switch (buff)
        {
            case BuffType.Health:
                currentHealth /= modifier;
                break;
            case BuffType.Damage:
                currentDamage /= modifier;
                break;
            case BuffType.AttackSpeed:
                currentAttackSpeed /= modifier;
                break;
            case BuffType.Range:
                currentRange /= modifier;
                rangeTrigger.radius = currentRange;
                break;
        }
    }
    void Update()
    {
        attackCooldown -= Time.deltaTime;
        if (attackCooldown <= 0f)
        {
            TryAttack();
            attackCooldown = 1f / currentAttackSpeed;
        }

        //if (currentSouls < maxSouls)
        //{
        //    currentSouls += currentSoulsRate * Time.deltaTime/2;
        //    currentSouls = Mathf.Clamp(currentSouls, 0f, maxSouls);
        //    OnSoulsChanged?.Invoke(currentSouls);
        //}
    }

    private void TryAttack()
    {
        targetsInRange.RemoveAll(e => e == null);

        if (targetsInRange.Count == 0)
            return;

        EnemyController target = targetsInRange[0];

        target.ReceiveDamage(currentDamage);

        //Aquí van animaciones, sonido y otros menesteres
        anim.SetTrigger("attack");
        audioSource.PlayOneShot(attackSound);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Enemy")) return;
        var enemy = col.GetComponent<EnemyController>();
        if (enemy != null && !targetsInRange.Contains(enemy))
            targetsInRange.Add(enemy);
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (!col.CompareTag("Enemy")) return;
        var enemy = col.GetComponent<EnemyController>();
        if (enemy != null)
            targetsInRange.Remove(enemy);
    }

    public void receiveDamage(float damage)
    {
        currentHealth -= damage;
        GetComponent<Damageable>().TakeDamage(damage);

        currentHealth = Mathf.Clamp(currentHealth, 0f, baseHealth);

        OnHealthChanged?.Invoke(currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public bool TrySpendSouls(float amount)
    {
        if (currentSouls >= amount)
            return true;
        return false;
    }

    public void SpendSouls(float amount)
    {
        currentSouls -= amount;
        OnSoulsChanged?.Invoke(currentSouls);
        soulsAnimation.SetTrigger("soul");
    }
    public void AddGold(int gold)
    {
        amountGold += gold;
        textGold.text = amountGold.ToString();

        OnGoldChanged?.Invoke(gold);
    }
    public void SpendGold(int gold)
    {
        if (amountGold >= gold)
        {
            amountGold -= gold;
            textGold.text = amountGold.ToString();
            OnGoldChanged?.Invoke(gold);
        }
    }
    private void Die()
    {
        PlayerPrefs.SetInt("OroGuardado", amountGold);
        SceneManager.LoadScene("GameOver");
    }

    public void RefillSouls()
    {
        currentSouls = maxSouls;
        OnSoulsChanged?.Invoke(currentSouls);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, baseRange);
    }

    public float BaseHealth
    {
        get => baseHealth;
        set
        {
            baseHealth = value;
            currentHealth = value;
            OnHealthChanged?.Invoke(currentHealth);
        }
    }

    public void AddKey()
    {
        FindFirstObjectByType<GameManager>().ShowMessage("You are not fit to see it… your weakness forbids it.", 4);
    }
    public void AddSouls()
    {
        if (soulsBuyPerShop < 3)
        {
            currentSouls++;
            maxSouls++;
            playerSouls.UpdateMaxSouls();
            soulsBuyPerShop++;
        }
    }

    public float CurrentHealth => currentHealth;
    public float CurrentDamage => currentDamage;
    public float CurrentAttackSpeed => currentAttackSpeed;
    public float CurrentRange => currentRange;
    public float CurrentSoulsRate => currentSoulsRate;
    public float CurrentSouls => currentSouls;
    public float MaxSouls => maxSouls;
    public int AmountGold => amountGold;

    public void Heal(float health)
    {
        currentHealth = Mathf.Min(currentHealth + health, baseHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void IncCurrentSouls()
    {
        currentSouls++;
    }

    public void IncMaxSouls()
    {
        maxSouls++;
        playerSouls.UpdateMaxSouls();
    }
}
