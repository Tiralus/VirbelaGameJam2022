using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public abstract class Powerup : MonoBehaviour
{
    public abstract void Execute();
    
    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
        {
            return;
        }

        Cloud cloud = other.GetComponent<Cloud>();


        if (cloud == null)
        {
            return;
        }
        
        PowerupSpawner.Instance.CollectPowerup(this);
    }
}
