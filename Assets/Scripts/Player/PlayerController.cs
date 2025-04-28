using UnityEngine;
using DeckboundDungeon.Cards.Buff;
using System.Collections.Generic;
using System.Collections;
using System;

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

    [Header("Current stats")]
    [SerializeField] private float currentHealth;
    [SerializeField] private float currentDamage;
    [SerializeField] private float currentAttackSpeed;
    [SerializeField] private float currentRange;
    [SerializeField] private float currentSoulsRate;
    [SerializeField] private int amountGold;

    public event Action<float> OnHealthChanged;

    void Awake()
    {
        currentHealth = baseHealth;
        currentDamage = baseDamage;
        currentAttackSpeed = baseAttackSpeed;
        currentRange = baseRange;
        currentSoulsRate = baseSoulsRate;

        rangeTrigger = GetComponent<CircleCollider2D>();
        rangeTrigger.isTrigger = true;
        rangeTrigger.radius = baseRange;

        OnHealthChanged?.Invoke(currentHealth);
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

    public void receiveDamage(float damage){
        currentHealth -= damage;

        currentHealth = Mathf.Clamp(currentHealth, 0f, baseHealth);

        OnHealthChanged?.Invoke(currentHealth);
        if(currentHealth <=0 ){
            Die();
        }
    }
    private void Die(){
        Destroy(gameObject);
    }
}
