using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Tilemaps;

public class CardBehaviour : MonoBehaviour
{
    int enemiesToKill;
    public TextMeshProUGUI healthText;

    int enemiesKilled = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemiesToKill = GetComponent<TrapController>().cardData.uses;
        if (healthText != null) healthText.text = (enemiesToKill - enemiesKilled).ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var col = collision.GetComponent<EnemyController>();
        if (col != null && col.type == EnemyController.EnemyType.Trampero) 
        {
            col.NotifyTrapFound(transform);
            return;
        }
        if (collision.tag == "Enemy") 
        {
            enemiesKilled++;

            if(healthText != null) healthText.text = (enemiesToKill - enemiesKilled).ToString();

            CheckToDestroy();

        }
    }

    void CheckToDestroy()
    {
        if (enemiesKilled >= enemiesToKill)
        {
            GetComponent<TrapController>().ClearAndDestroy();
        }
    }

    public void ReceiveDamage(float damage)
    {
        enemiesKilled += (int) damage;
        healthText.text = (enemiesToKill - enemiesKilled).ToString();

        CheckToDestroy();
    }
}
