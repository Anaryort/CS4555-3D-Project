using UnityEngine;

public class DestroyOnTrigger : MonoBehaviour
{
    public GameObject ObjToDestroy;
    
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            Destroy(ObjToDestroy);
        }
    }

}
