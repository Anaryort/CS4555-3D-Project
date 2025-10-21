using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ShopKeeper : MonoBehaviour
{
    public Animator anim;
    private bool playerInRange;
    [SerializeField] private List<ShopItems> shopItems;
    [SerializeField] private List<ShopItems> shopBuffs;
    [SerializeField] private List<ShopItems> shopWeapons;

    public static event Action<ShopManager, bool> OnShopStateChanged;
    public ShopManager shopManager;


    public CanvasGroup shopCanvasGroup;
    private bool isShopOpen;

    void Update()
    {
        if (playerInRange)
        {
            if (Input.GetButtonDown("Interact"))
            {
                if (!isShopOpen)
                {
                    Time.timeScale = 1;
                    isShopOpen = true;
                    OnShopStateChanged?.Invoke(shopManager, true);
                    shopCanvasGroup.alpha = 1;
                    shopCanvasGroup.blocksRaycasts = true;
                    shopCanvasGroup.interactable = true;
                    OpenItemShop();
                }
                else
                {
                    Time.timeScale = 0;
                    isShopOpen = false;
                    OnShopStateChanged?.Invoke(shopManager, false);
                    shopCanvasGroup.alpha = 0;
                    shopCanvasGroup.blocksRaycasts = false;
                    shopCanvasGroup.interactable = false;
                }
            }
        }
    }

    public void OpenItemShop()
    {
        shopManager.PopulateShopItems(shopItems);
    }
    public void OpenBuffShop()
    {
        shopManager.PopulateShopItems(shopBuffs);

    }
    public void OpenWeaponShop()
    {
        shopManager.PopulateShopItems(shopWeapons);

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            anim.SetBool("playerInRange", true);
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            anim.SetBool("playerInRange", false);
            playerInRange = false;
        }
    }
}
