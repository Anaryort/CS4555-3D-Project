using System;
using UnityEngine;

public class Loot : MonoBehaviour
{
    public ItemSO itemSO;
    public SpriteRenderer sr;
    public Animator anim;
    public bool canBePickedUp = true;
    public int quantity;
    public static event Action<ItemSO, int> OnItemLooted;
    private void OnValidate()
    {
        if (itemSO == null)
            return;
        UpdateApperence();
    }

    public void Initialize(ItemSO itemSO, int quantity)
    {
        this.itemSO = itemSO;
        this.quantity = quantity;
        canBePickedUp = false;
        UpdateApperence();
    }

    private void UpdateApperence()
    {
        sr.sprite = itemSO.icon;
        this.name = itemSO.itemName;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        canBePickedUp = true;

        if (canBePickedUp)
        {
            anim.Play("LootAnim"); // typo fixed from "LooAnim"
            OnItemLooted?.Invoke(itemSO, quantity);
            Destroy(gameObject, 0.5f);
        }
    }

}
