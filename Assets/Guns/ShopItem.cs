using UnityEngine;

[CreateAssetMenu(fileName = "ShopItem", menuName = "Scriptable Objects/ShopItem")]
public class ShopItem : ScriptableObject
{

    public enum ItemType
    {
        Gun,
        Ammo,
        Consumbable,
        Other
    }
    /*
    public enum ItemType2
    {
        Replacement,
        OneTime,
        Repeating,

        Consumbable,
        Other
    }
    */

    public string name = "";
    public ItemType itemType = ItemType.Gun;

    public int price = 0;
    

    public Texture2D icon;

    public GameObject prefab;

    public Vector3 position = Vector3.zero;
    public Vector3 scale = Vector3.one;

    public int ammoPurchaseAmount = 10;

    public string specialPurchaseText;



    public bool disableWhenPurchased = false;
    public bool replaceWhenPurchased = true;

    public ShopItem replacement;


}
