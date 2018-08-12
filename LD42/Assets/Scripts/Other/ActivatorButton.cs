using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatorButton : MonoBehaviour {

    public ActivableItem[] Items;
    public bool IgnoreCollisionWithPlayer = true;

    private void Start()
    {
        if(IgnoreCollisionWithPlayer){
            Collider2D player = GameObject.FindGameObjectWithTag("Player").GetComponent<Collider2D>();

            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), player);
        } }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Arrow")
        {
            foreach(ActivableItem item in Items)
            {
                item.Activate();
            }

            Destroy(gameObject);
        }
    }
}
