using UnityEngine;

[CreateAssetMenu(fileName = "ShopItem", menuName = "Scriptable Objects/ShopItem")]
public class ShopItem : ScriptableObject
{

    public enum ItemType
    {
        GunThenAmmo,
        JustGun,
        Consumbable,
        Other
    }


    public string name = "";
    public ItemType itemType = ItemType.GunThenAmmo;
    public int initialCost = 0;

    public int repeatCost = 0;

    public Texture2D icon;

    public GameObject prefab;

    public Vector3 scaleOverride = Vector3.zero;

    public int ammoPurchaseAmount = 10;

}
