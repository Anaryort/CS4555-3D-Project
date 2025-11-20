using UnityEngine;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    [Header("Slots / UI")]
    public InventorySlot[] itemSlots;
    public TMP_Text goldText;

    [Header("Gameplay")]
    public UseItem useItem;
    public int gold;
    public GameObject LootPrefab;
    public Transform player;

    // -------- Persistent data (static = survives scene loads) --------
    private static ItemSO[] savedItems;
    private static int[] savedQuantities;
    private static int savedGold;
    private static bool hasSavedData = false;

    private void Start()
    {
        // If we already have saved data, restore it into new scene's UI
        if (hasSavedData && savedItems != null && savedItems.Length == itemSlots.Length)
        {
            gold = savedGold;
            if (goldText != null)
                goldText.text = gold.ToString();

            for (int i = 0; i < itemSlots.Length; i++)
            {
                itemSlots[i].itemSO = savedItems[i];
                itemSlots[i].quantity = savedQuantities[i];
                itemSlots[i].UpdateUI();
            }
        }
        else
        {
            // First time (or slot count changed) – just sync current state into static storage
            foreach (var slot in itemSlots)
            {
                slot.UpdateUI();
            }

            if (goldText != null)
                goldText.text = gold.ToString();

            SavePersistentData();
        }
    }

    private void OnEnable()
    {
        Loot.OnItemLooted += AddItem;
    }

    private void OnDisable()
    {
        Loot.OnItemLooted -= AddItem;
    }

    // -------- Public so other scripts (e.g. InventorySlot) can call it --------
    public void SavePersistentData()
    {
        if (savedItems == null || savedItems.Length != itemSlots.Length)
        {
            savedItems = new ItemSO[itemSlots.Length];
            savedQuantities = new int[itemSlots.Length];
        }

        for (int i = 0; i < itemSlots.Length; i++)
        {
            savedItems[i] = itemSlots[i].itemSO;
            savedQuantities[i] = itemSlots[i].quantity;
        }

        savedGold = gold;
        hasSavedData = true;
    }

    public void AddItem(ItemSO itemSO, int quantity)
    {
        if (itemSO.isGold)
        {
            gold += quantity;
            if (goldText != null)
                goldText.text = gold.ToString();

            SavePersistentData();
            return;
        }

        // Try to stack into existing slots
        foreach (var slot in itemSlots)
        {
            if (slot.itemSO == itemSO && slot.quantity < itemSO.stackSize)
            {
                int availableSpace = itemSO.stackSize - slot.quantity;
                int amountToAdd = Mathf.Min(availableSpace, quantity);

                slot.quantity += amountToAdd;
                quantity -= amountToAdd;

                slot.UpdateUI();

                if (quantity <= 0)
                {
                    SavePersistentData();
                    return;
                }
            }
        }

        // Put into empty slot(s)
        foreach (var slot in itemSlots)
        {
            if (slot.itemSO == null)
            {
                int amountToAdd = Mathf.Min(itemSO.stackSize, quantity);
                slot.itemSO = itemSO;
                slot.quantity = amountToAdd;
                slot.UpdateUI();

                quantity -= amountToAdd;
                if (quantity <= 0)
                {
                    SavePersistentData();
                    return;
                }
            }
        }

        // If still leftover, drop it into the world
        if (quantity > 0)
        {
            DropLoot(itemSO, quantity);
        }

        SavePersistentData();
    }

    public void DropItem(InventorySlot slot)
    {
        DropLoot(slot.itemSO, 1);
        slot.quantity--;
        if (slot.quantity <= 0)
        {
            slot.itemSO = null;
        }
        slot.UpdateUI();
        SavePersistentData();
    }

    private void DropLoot(ItemSO itemSO, int quantity)
    {
        Loot loot = Instantiate(LootPrefab, player.position, Quaternion.identity).GetComponent<Loot>();
        loot.Initialize(itemSO, quantity);
    }

    public void UseItem(InventorySlot slot)
    {
        if (slot.itemSO != null && slot.quantity > 0)
        {
            // Find all UseItem components in the scene (for both Player 1 and Player 2)
            UseItem[] allUseItems = FindObjectsOfType<UseItem>();

            if (allUseItems == null || allUseItems.Length == 0)
            {
                Debug.LogError("UseItem component not found! Make sure UseItem components exist on Player GameObjects in the scene.");
                return;
            }

            // Apply item effects to all players
            foreach (UseItem useItemInstance in allUseItems)
            {
                if (useItemInstance != null)
                {
                    useItemInstance.ApplyItemEffects(slot.itemSO);
                }
            }

            slot.quantity--;
            if (slot.quantity <= 0)
            {
                slot.itemSO = null;
            }
            slot.UpdateUI();

            SavePersistentData();
        }
    }
}
