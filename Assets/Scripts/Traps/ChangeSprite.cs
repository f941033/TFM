using UnityEngine;

public class ChangeSprite : MonoBehaviour
{
    public GameObject sprite;
    public Sprite[] sprites;
    private Animator animator;
    int index = 0;    

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
        if (collision.CompareTag("Enemy"))
        {
            if(sprite.GetComponent<Animator>()!= null)
                if (index == 0) sprite.GetComponent<Animator>().enabled = false;
            if (index < sprites.Length)
            {
                sprite.GetComponent<SpriteRenderer>().sprite = sprites[index];
                index++;
            }

            if (index == sprites.Length)
            {
                GetComponent<TrapController>().ClearAndDestroy();
            }
        }
    }


}
