using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using static ShopItem;
using static UnityEditor.Progress;
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

    private ShopItemUI[] shopUIList;
    private GameObject[] purchasedWeapons;

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
        shopUIList = new ShopItemUI[shopItems.Length];
        purchasedWeapons = new GameObject[shopItems.Length];

        int baseOffset = itemUIBase.baseOffset;
        int indexOffset = itemUIBase.indexOffset;
        Vector2 contentSize = shopContentView.sizeDelta;
        contentSize.y = indexOffset * shopItems.Length;
        shopContentView.sizeDelta = contentSize;

        for (int i = 0; i < shopItems.Length; i++)
        {
            //ShopItem item = shopItems[i];

            ShopItemUI itemUI =GameObject.Instantiate(shopItemUIPrefab,shopContentView).GetComponent<ShopItemUI>();
            shopUIList[i] = itemUI;
            RectTransform itemTransform = itemUI.GetComponent<RectTransform>();
            itemTransform.anchoredPosition = new Vector2(0, -baseOffset - indexOffset * i);
            InitializeShopItem(i);
            itemUI.purchaseButton.onClick.AddListener(() => PurchaseItem(itemUI.index));
        }
    }
    private void InitializeShopItem(int index)
    {

        ShopItem item = shopItems[index];
        string purchaseText = "Buy " + item.itemType.ToString();
        if (item.specialPurchaseText.Length > 0)
        {
            purchaseText = item.specialPurchaseText;
        }
        shopUIList[index].Initialize(item.name, item.initialCost, item.icon, index, purchaseText);
    }
    private void ReplaceShopItem(ShopItem newItem,int index)
    {
        shopItems[index] = newItem;
        ShopItem item = newItem;
        string purchaseText = "Buy " + item.itemType.ToString();
        if (string.IsNullOrEmpty(item.specialPurchaseText))
        {
            purchaseText = item.specialPurchaseText;
        }
        shopUIList[index].Initialize(item.name, item.initialCost, item.icon, index, purchaseText);
    }

    private void PurchaseItem(int index)
    {
        ShopItem item = shopItems[index];
        ItemType type = item.itemType;
        //Add Gun


        if ((type == ItemType.Gun || type==ItemType.Consumbable)&&purchasedWeapons[index] == null) {
            GameObject weapon = Instantiate(item.prefab, weaponsSystem.transform);
            weapon.transform.localPosition = item.position;
            weapon.transform.localScale = item.scale;
            weaponsSystem.AddWeapon(weapon);
            weapon.SetActive(false);
            purchasedWeapons[index]=weapon;
            cash -= item.initialCost;
        }
        else if(type==ItemType.Ammo||type==ItemType.Consumbable) {
            if (purchasedWeapons[index].TryGetComponent<Weapon>(out var weapon)){
                //weapon
            }
           
            
        }else
        {

        }
        //If replacement
        if (item.disableWhenPurchased)
        {
            ShopItemUI itemUI = shopUIList[index];
            itemUI.purchaseButton.interactable = false;
            itemUI.purchaseLabel.text = "Out of Stock";

        }
        if (item.replaceWhenPurchased && item.replacement != null)
        {
            ReplaceShopItem(item.replacement,index);



        }
        else if (item.replacement == null)
        {
            Debug.Log("This is bad. Do not do this. There should be a replacement if replacement is enabled");
        }







        if (type == ItemType.Other)
        {
            Debug.Log("Purchase item: " + index);

        }
        
        


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
