using UnityEngine;

public class FixedTrapBehaviour : MonoBehaviour
{
    public int damage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Enemy")
        {
            var enemy = collision.GetComponent<EnemyController>();
            if (enemy == null) return;
            enemy.ReceiveDamage(damage);
        }
    }

    public void ActivateAnimation()
    {
        GetComponent<Animator>().SetTrigger("active");
    }
}
