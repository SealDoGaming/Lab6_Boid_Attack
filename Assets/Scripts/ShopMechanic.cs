using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class ShopMechanic : MonoBehaviour
{
    public InputAction OpenMenu;
    public InputAction OpenShop;

    public WeaponSystem weaponsSystem;

    public MouseRotator[] aimControllers;

    public Animator phoneAnimator;

    public GameObject phoneMenu;
    public GameObject shopMenu;

    public RectTransform shopContentView;
    public GameObject shopItemUIPrefab;

    public ShopItem[] shopItems;

    public event Action<List<(Collider, Vector3)>> Hit;

    bool menuOpen = false;
    bool shopOpen = false;


    bool moving = false;

    int cash = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OpenMenu.performed += OnOpenMenu;
        OpenShop.performed += OnOpenShop;
        OpenMenu.Enable();
        OpenShop.Enable();
        InititalizeShop();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddCash(int money)
    {
        cash += money;
    }

    private void InititalizeShop()
    {
        ShopItemUI itemUIBase = shopItemUIPrefab.GetComponent<ShopItemUI>();

        int baseOffset = itemUIBase.baseOffset;
        int indexOffset = itemUIBase.indexOffset;
        Vector2 contentSize = shopContentView.sizeDelta;
        contentSize.y = indexOffset * shopItems.Length;
        shopContentView.sizeDelta = contentSize;

        for (int i = 0; i < shopItems.Length; i++)
        {
            ShopItem item = shopItems[i];

            ShopItemUI itemUI =GameObject.Instantiate(shopItemUIPrefab,shopContentView).GetComponent<ShopItemUI>();
            RectTransform itemTransform = itemUI.GetComponent<RectTransform>();
            itemTransform.anchoredPosition = new Vector2(0, -baseOffset - indexOffset * i);
            string price = "$" + item.initialCost;
            itemUI.Initialize(item.name, price, item.icon, i);
            itemUI.purchaseButton.onClick.AddListener(() => PurchaseItem(itemUI.index));
        }
    }

    private void PurchaseItem(int index)
    {
        Debug.Log("Purchase item: " + index);
    }

    private void OnOpenMenu(CallbackContext c)
    {
        if (shopOpen||moving)
        {
            return;
        }
        if (menuOpen)
        {
            phoneAnimator.SetBool("EnablePhone", false);
            for (int i = 0; i < aimControllers.Length; i++)
            {
                aimControllers[i].SetLock(false);
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;


            menuOpen = false;
            weaponsSystem.UnlockWeapons();
            moving = true;
            StartCoroutine(OnPhoneAway());
        }
        else
        {
            phoneAnimator.SetBool("EnablePhone", true);
            for (int i = 0; i < aimControllers.Length; i++)
            {
                aimControllers[i].SetLock(true);
            }
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;


            menuOpen = true;
            moving = true;
            StartCoroutine(OnPhoneOpened());
            phoneMenu.SetActive(true);
        }

    }

    private void OnOpenShop(CallbackContext c)
    {
        if (menuOpen || moving)
        {   
            return;
        }
        if (shopOpen)
        {
            phoneAnimator.SetBool("EnablePhone", false);
            for (int i = 0; i < aimControllers.Length; i++)
            {
                aimControllers[i].SetLock(false);
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;


            shopOpen = false;
            weaponsSystem.UnlockWeapons();
            
            StartCoroutine(OnPhoneAway());
        }
        else
        {
            phoneAnimator.SetBool("EnablePhone", true);
            for (int i = 0; i < aimControllers.Length; i++)
            {
                aimControllers[i].SetLock(true);
            }
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;


            shopOpen = true;
            StartCoroutine(OnPhoneOpened());
            shopMenu.SetActive(true);
            //weaponsSystem.LockWeapons();
        }

    }
    private IEnumerator OnPhoneAway()
    {

        yield return new WaitUntil(()=>phoneAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Phone Away"));
        phoneMenu.SetActive(false);
        shopMenu.SetActive(false);
        moving = false;
    }

    private IEnumerator OnPhoneOpened()
    {


        yield return new WaitUntil(() => phoneAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Phone Open"));
        weaponsSystem.LockWeapons();
        moving = false;
    }




}
