using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class LightningPowerup : Powerup
{
    public float LightningAmount;
    
    public override void Execute()
    {
        Cloud.Instance.AddLightning(LightningAmount);
    }
}
