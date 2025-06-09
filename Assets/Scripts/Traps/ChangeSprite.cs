using UnityEngine;

public class ChangeSprite : MonoBehaviour
{
    public Sprite[] sprites;
    private Animator animator;
    int index = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (index == 0) animator.enabled = false;
            GetComponent<SpriteRenderer>().sprite = sprites[index];
            index++;

            if (index == sprites.Length)
            {
                GetComponent<TrapController>().ClearAndDestroy();
                Invoke("Clear", 1f);
            }
        }
    }

    //private void Clear()
    //{
    //    GetComponent<TrapController>().ClearAndDestroy();
    //}
}
