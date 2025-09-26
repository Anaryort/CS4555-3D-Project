using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shrink_power : MonoBehaviour
{
    public float multiplier = 3f;
    public float duration = 4f;
    public GameObject pickupEff;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(Pickup(other));
        }
    }

    IEnumerator Pickup(Collider player)
    {
        Debug.Log("Powerup picked!");

        // Effect
        Instantiate(pickupEff, transform.position, transform.rotation);

        // Apply Effect
        player.transform.localScale /= multiplier;

        // Disable Renderer and Collider
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        // Wait x
        yield return new WaitForSeconds(duration);

        //Revert Effect
        player.transform.localScale *= multiplier;

        // Destroy powerup
        Destroy(gameObject);
    }

}
