using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static ShopItem;
using static UnityEngine.InputSystem.InputAction;

public class ShopMechanic : MonoBehaviour
{
    public InputAction OpenMenu;
    public InputAction OpenShop;

    public int startingCash = 0;

    public WeaponSystem weaponsSystem;

    public MouseRotator[] aimControllers;

    public Animator phoneAnimator;

    public GameObject phoneMenu;
    public GameObject shopMenu;

    public RectTransform shopContentView;
    public GameObject shopItemUIPrefab;



    public Button optionsButton;
    public Button unstuckButton;
    public Button mainMenuButton;
    public Button selfDestructButton;



    public TextMeshProUGUI shopTitle;
    public ShopItem[] shopItems;

    private ShopItemUI[] shopUIList;

    private GameObject[] purchasedWeapons;


    public Rigidbody phone;

    public GameObject selfDestructExplosion;
    public Transform explosionTargetTransform;

    public Transform unstuckSpot;
    public TextMeshProUGUI cashText;

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
        InitializeMenuButtons();
        cash = startingCash;
        InitializeShop();
    }

    // Update is called once per frame
    void Update()
    {
        cashText.text = "Cash: $" + cash;
    }
    #region Shop Methods

    public void AddCash(int money)
    {
        cash += money;
    }

    private void InitializeShop()
    {
        ShopItemUI itemUIBase = shopItemUIPrefab.GetComponent<ShopItemUI>();
        shopUIList = new ShopItemUI[shopItems.Length];
        purchasedWeapons = new GameObject[shopItems.Length];

        int baseOffset = itemUIBase.baseOffset;
        int indexOffset = itemUIBase.indexOffset;
        Vector2 contentSize = shopContentView.sizeDelta;
        contentSize.y = indexOffset * (shopItems.Length+1)+25;
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
        shopUIList[index].Initialize(item.name, item.price, item.icon, index, purchaseText);
    }
    private void ReplaceShopItem(ShopItem newItem,int index)
    {
        shopItems[index] = newItem;
        ShopItem item = newItem;
        string purchaseText = "Buy " + item.itemType.ToString();
        if (!string.IsNullOrEmpty(item.specialPurchaseText))
        {
            purchaseText = item.specialPurchaseText;
        }
        shopUIList[index].Initialize(item.name, item.price, item.icon, index, purchaseText);
    }

    private void PurchaseItem(int index)
    {
        ShopItem item = shopItems[index];
        ItemType type = item.itemType;
        //Add Gun
        if (cash < item.price)
        {
            return;
        }

        if ((type == ItemType.Gun || type==ItemType.Consumbable)&&purchasedWeapons[index] == null) {
            GameObject weapon = Instantiate(item.prefab, weaponsSystem.transform);
            weapon.transform.localPosition = item.position;
            weapon.transform.localScale = item.scale;
            weapon.GetComponent<Weapon>().SetCurrentAmmo(item.ammoPurchaseAmount);
            weaponsSystem.AddWeapon(weapon);
            weapon.SetActive(false);
            purchasedWeapons[index]=weapon;
            cash -= item.price;
        }
        else if(type==ItemType.Ammo||type==ItemType.Consumbable) {
            purchasedWeapons[index].GetComponent<Weapon>().AddAmmoToReserve(item.ammoPurchaseAmount);
            cash-= item.price;

            
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
        else if (item.replaceWhenPurchased && item.replacement == null)
        {
            Debug.Log("This is bad. Do not do this. There should be a replacement if replacement is enabled");
        }







        if (type == ItemType.Other)
        {
            Debug.Log("Purchase item: " + index);

        }
        
        


    }
    #endregion

    #region Menu Button Methods
    private void InitializeMenuButtons()
    {
        optionsButton.onClick.AddListener(() => OpenOptions());
        unstuckButton.onClick.AddListener(() => Unstuck());
        mainMenuButton.onClick.AddListener(() => QuitToMainMenu());
        selfDestructButton.onClick.AddListener(() => SelfDestruct());
    }
    private void OpenOptions()
    {

    }

    private void Unstuck()
    {
        transform.position = unstuckSpot.position;
        transform.rotation = unstuckSpot.rotation;
        OnOpenMenu(new CallbackContext());
    }

    private void QuitToMainMenu()
    {

        SceneManager.LoadScene("BoidScene3");
    }
    
    private void SelfDestruct()
    {
        phone.useGravity = true;
        phone.isKinematic = false;
        phone.transform.parent = null;

        GameObject explosion = Instantiate(selfDestructExplosion, explosionTargetTransform);
        explosion.transform.localScale = Vector3.one;
        explosion.transform.localPosition = Vector3.zero;
        explosion.transform.localRotation = Quaternion.identity;
        SetGameLayerRecursive(explosion, explosionTargetTransform.gameObject.layer);
        //SetPhoneBackground(brokenScreenMAT);
        StopAllCoroutines();
        moving = true;
        StartCoroutine(DeathPause());

    }
    private void SetGameLayerRecursive(GameObject _go, int _layer)
    {
        _go.layer = _layer;
        foreach (Transform child in _go.transform)
        {
            child.gameObject.layer = _layer;

            Transform _HasChildren = child.GetComponentInChildren<Transform>();
            if (_HasChildren != null)
                SetGameLayerRecursive(child.gameObject, _layer);

        }
    }

    private IEnumerator DeathPause()
    {
        yield return new WaitForSeconds(0.25f);
        //Debug.Break();
        Application.Quit();
    }
    #endregion


    #region Phone Control Methods
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
            //SetPhoneBackground(menuScreenMAT);


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
            moving = true;
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
            //SetPhoneBackground(shopScreenMAT);  


            shopOpen = true;
            moving = true;
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

    #endregion

}
