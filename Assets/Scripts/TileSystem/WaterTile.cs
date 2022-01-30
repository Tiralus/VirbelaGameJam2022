using UnityEngine;

public class WaterTile : BaseTile
{
    public ParticleSystem CloudCollecting;

    public override void Select()
    {
        base.Select();
        
        CloudCollecting.Play();
    }

    public override void Deselect()
    {
        base.Deselect();
        
        CloudCollecting.Stop();
    }
}
