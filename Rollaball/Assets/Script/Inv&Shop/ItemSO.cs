using UnityEngine;
[CreateAssetMenu(fileName = "New Item")]



public class ItemSO : ScriptableObject
{
    
    public string itemName;
    public string itemDescription;
    public Sprite icon;
    public int stackSize = 3;
    public bool isGold;

    [Header("Stats")]
    public int currentHealth;
    public int maxHealth;
    public float speed;
    public float jumpForce;
    
    [Header("Temp")]
    public float duration;

}
