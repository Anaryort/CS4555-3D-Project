using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class ShopSlot : MonoBehaviour
{
    public ItemSO itemSO;
    public TMP_Text itemNameText;
    public TMP_Text priceText;
    public Image itemImage;
    [SerializeField] private ShopManager shopManager;
    private int price;
    public void Initalize(ItemSO newItemSO, int price)
    {
        itemSO = newItemSO;
        itemImage.sprite = itemSO.icon;
        itemNameText.text = itemSO.itemName;
        this.price = price;
        priceText.text = price.ToString();
    }
    public void OnBuyButtonClicked()
    {
        shopManager.TryBuyItem(itemSO, price);
    }
}
