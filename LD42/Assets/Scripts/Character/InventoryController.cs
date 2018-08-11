using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class InventoryController : MonoBehaviour {

    public float CursorStartingPoint = -46f;
    public float CursorMove = 46;

    [Tooltip("Wait time before player can confirm, in seconds")]
    public float MenuTimer = 1f;

    public Image[] InventoryPanels;

    RectTransform cursorPosition;
    Image cursorImage;
    PlatformCharController controller;

    public int _cursorIndex;
    int _panelsNum;
    PickUp[] _spaces;
    PickUpController _currentPedestal;
    float _currentTimer = 0f;

    void Start()
    {
        controller = GetComponent<PlatformCharController>();

        /*Image[] panels = GameObject.FindGameObjectsWithTag("InventoryPanel").Select(x => x.GetComponent<Image>()).ToArray();

        InventoryPanels = new Image[panels.Length];
        foreach (Image i in InventoryPanels)
        {
            if (i.gameObject.name == "Panel1")
                InventoryPanels[0] = i;
            else if (i.gameObject.name == "Panel2")
                InventoryPanels[1] = i;
            else
                InventoryPanels[2] = i;
        }*/

       

        GameObject cursor = GameObject.FindGameObjectWithTag("InventoryCursor");
        cursorPosition = cursor.GetComponent<RectTransform>();
        cursorImage = cursor.GetComponent<Image>();

        _spaces = new PickUp[InventoryPanels.Length];

        for (int i = 0; i < _spaces.Length; i++)
        {
            _spaces[i] = PickUp.Bomb;
        }

        _spaces[0] = PickUp.Sword;
        _spaces[1] = PickUp.Bow;
        _spaces[2] = PickUp.Bomb;
    }

    public bool MenuState() {
        return controller.MenuState();
    }

    private void Update()
    {
        if (controller.MenuState()) {
            if (_currentTimer > 0f)
            {
                _currentTimer -= Time.deltaTime;
                if (_currentTimer <= 0f)
                {
                    _currentTimer = 0f;
                }
            }

            if (Input.GetButtonDown("Left"))
            {
                _cursorIndex--;
                if (_cursorIndex < 0)
                {
                    _cursorIndex = _spaces.Length - 1;
                }

            }
            else if (Input.GetButtonDown("Right"))
            {
                _cursorIndex++;
                if (_cursorIndex >= _spaces.Length)
                {
                    _cursorIndex = 0;
                }
            }
            cursorPosition.anchoredPosition = new Vector3(CursorStartingPoint + CursorMove * _cursorIndex, cursorPosition.anchoredPosition.y, 0);

            if (Input.GetButtonDown("PickUp") && _currentTimer <= 0f) {
                Debug.Log("Confirm, index: " + _cursorIndex);

                PickUp pedestalItem = _currentPedestal.Type;
                Sprite pedestalSprite = _currentPedestal.GetSprite();

                // Switching Item
                _currentPedestal.ChangeItem(_spaces[_cursorIndex], InventoryPanels[_cursorIndex].sprite);
                _spaces[_cursorIndex] = pedestalItem;
                InventoryPanels[_cursorIndex].sprite = pedestalSprite;
                InventoryPanels[_cursorIndex].color = new Color(1, 1, 1, 1);

                cursorImage.enabled = false;
                _cursorIndex = 0;
                controller.CloseMenu();
            }
        }
    }

    public void PickUpItem(PickUpController pedestal) {
        /*var flag = false;

        for (var i = 0; i < _spaces.Length; i++)
        {
            if (_spaces[i] == PickUp.Empty)
            {
                flag = true;
                _spaces[i] = PickUp.Sword;
                inventoryPanels[i].sprite = graphics;
                inventoryPanels[i].color = new Color(1, 1, 1, 1);
                break;
            }
        } */

        _currentPedestal = pedestal;
        controller.OpenMenu();
        cursorImage.enabled = true;
        _currentTimer = MenuTimer;
    }

    public bool HasPickUp(PickUp type){
        return _spaces.Where(x => x == type).Count() > 0;
    }
}
