using UnityEngine;
using System.Collections;

public class UseItem : MonoBehaviour
{
    private PlayerController player;

    void Awake()
    {
        player = GetComponent<PlayerController>();
        if (player == null)
        {
            Debug.LogError($"UseItem on {gameObject.name} requires a PlayerController component on the same GameObject!");
        }
    }

    public void ApplyItemEffects(ItemSO itemSO)
    {
        if (player == null)
        {
            Debug.LogError("Cannot apply item effects: PlayerController is null!");
            return;
        }

        if (itemSO.speed != 0)
            player.UpdateSpeed(itemSO.speed);

        if (itemSO.jumpForce != 0)
            player.UpdateJumpForce(itemSO.jumpForce);

        if (itemSO.duration > 0)
            StartCoroutine(EffectTimer(itemSO, itemSO.duration));
    }

    private IEnumerator EffectTimer(ItemSO itemSO, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (player == null) yield break;

        if (itemSO.speed != 0)
            player.UpdateSpeed(-itemSO.speed);

        if (itemSO.jumpForce != 0)
            player.UpdateJumpForce(-itemSO.jumpForce);
    }
}