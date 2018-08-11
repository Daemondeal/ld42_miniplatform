using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpController : MonoBehaviour {

    public PickUp Type;

	MeshRenderer text;
    SpriteRenderer sr;
    InventoryController playerInventory;
    PlatformCharController playerController;


    [Tooltip("Wait time before player can pick again, in seconds")]
    public float MenuTimer = 1f;

    float _currentTimer = 0f;

    // Use this for initialization
    void Start () {
		text = GetComponentInChildren<MeshRenderer>();
        sr = GetComponentInChildren<SpriteRenderer>();
        playerInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<InventoryController>();
        playerController = playerInventory.gameObject.GetComponent<PlatformCharController>();
	}
	
	// Update is called once per frame
	void Update () {
        if (_currentTimer > 0f)
        {
            _currentTimer -= Time.deltaTime;
            if (_currentTimer <= 0f)
            {
                _currentTimer = 0f;
            }
        }

        if (text.enabled && _currentTimer <= 0f && !playerInventory.MenuState())
        {
            // Pickup Logic

            if (Input.GetButtonDown("PickUp") && playerController.CurrentState == State.Running)
            {
                playerInventory.PickUpItem(this);
                Debug.Log("PickUp");
            }

        }
    }

    public Sprite GetSprite() {
        return sr.sprite;
    }

    public void ChangeItem(PickUp type, Sprite sprite) {
        sr.sprite = sprite;
        Type = type;
        _currentTimer = MenuTimer;
    }

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.tag == "Player") {
			text.enabled = true;
		}
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
        /*if (collision.gameObject.tag == "Player")
        {
            // Pickup Logic

            if (Input.GetButtonDown("PickUp"))
                collision.GetComponent<InventoryController>().PickUpItem(this);

        }*/
    }

    private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.gameObject.tag == "Player")
		{
			text.enabled = false;
		}
	}
}
