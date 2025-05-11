using UnityEngine;
using DeckboundDungeon.Cards.Buff;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.SceneManagement;


public class PlayerController : MonoBehaviour
{
    private float attackCooldown = 0f;

    private List<EnemyController> targetsInRange = new List<EnemyController>();
    private CircleCollider2D rangeTrigger;

    [Header("Base stats")]
    [SerializeField] private float baseHealth = 100f;
    private float baseDamage = 20f;
    private float baseAttackSpeed = 1f;
    private float baseRange = 1f;
    private float baseSoulsRate = 1f;
    [SerializeField] private float maxSouls = 15f;

    [Header("Current stats")]
    [SerializeField] private float currentHealth;
    [SerializeField] private float currentDamage;
    [SerializeField] private float currentAttackSpeed;
    [SerializeField] private float currentRange;
    [SerializeField] private float currentSouls;
    [SerializeField] private float currentSoulsRate;
    [SerializeField] private int amountGold;

    //[Header("Events")]
    public event Action<float> OnSoulsChanged;
    public event Action<float> OnHealthChanged;
    public event Action<int> OnGoldChanged;

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

        OnHealthChanged?.Invoke(currentHealth);
        OnSoulsChanged?.Invoke(currentSouls);
    }

    public void ApplyTemporaryBuff(BuffType buff, float modifier, float duration){

        StartCoroutine(ApplyBuffCoroutine(buff, modifier, duration));
    }

    public IEnumerator ApplyBuffCoroutine( BuffType buff, float modifier, float duration){
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
            case BuffType.SoulRate:
                currentSoulsRate *= modifier;
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
            case BuffType.SoulRate:
                currentSoulsRate /= modifier;
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

        if (currentSouls < maxSouls)
        {
            currentSouls += currentSoulsRate * Time.deltaTime/2;
            currentSouls = Mathf.Clamp(currentSouls, 0f, maxSouls);
            OnSoulsChanged?.Invoke(currentSouls);
        }
    }

    private void TryAttack()
    {
        targetsInRange.RemoveAll(e => e == null);

        if (targetsInRange.Count == 0) 
            return;

        EnemyController target = targetsInRange[0];

        target.receiveDamage(currentDamage);
        //Aquí van animaciones, sonido y otros menesteres
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
        if(currentHealth <=0 ){
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
    }
    public void AddGold(int gold)
    {
        amountGold+= gold;
        OnGoldChanged?.Invoke(gold);
    }
    public void SpendGold(int gold)
    {
        if(amountGold > gold)
        {
            amountGold-=gold;
            OnGoldChanged?.Invoke(gold);
        }
    }
    private void Die()
    {
        SceneManager.LoadScene("GameOver");
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
        }
    }

    public float CurrentHealth   => currentHealth;
    public float CurrentDamage   => currentDamage;
    public float CurrentAttackSpeed => currentAttackSpeed;
    public float CurrentRange    => currentRange;
    public float CurrentSoulsRate => currentSoulsRate;
    public float CurrentSouls => currentSouls;
    public float MaxSouls => maxSouls;
}
