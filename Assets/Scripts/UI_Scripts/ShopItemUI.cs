using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TextMeshProUGUI nameLabel;
    public Image priceBox;
    public TextMeshProUGUI priceLabel;

    public RawImage iconImage;


    public Button purchaseButton;
    public TextMeshProUGUI purchaseLabel;

    public event Action<int> Purchase;


    public int baseOffset = 55;
    public int indexOffset = 120;


    public int index = 0;

    public void Initialize(string name,string priceText,Texture2D image, int index, string purchaseText = "Buy Gun")
    {
        nameLabel.text = name;
        priceLabel.text = priceText;
        iconImage.texture = image;
        this.index = index;
        purchaseLabel.text = purchaseText;

    }



}
