using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CardBehaviour : MonoBehaviour
{
    public int enemiesToKill = 6;
    public TextMeshProUGUI healthText;

    int enemiesKilled = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (healthText != null) healthText.text = (enemiesToKill - enemiesKilled).ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy") 
        {
            enemiesKilled++;

            if(healthText != null) healthText.text = (enemiesToKill - enemiesKilled).ToString();

            if (enemiesKilled == enemiesToKill)
            {
                GetComponent<TrapController>().ClearAndDestroy();
            }

        }
    }
}
