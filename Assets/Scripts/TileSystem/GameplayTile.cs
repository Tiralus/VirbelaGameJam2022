using System.Collections.Generic;
using UnityEngine;

public class GameplayTile : BaseTile
{
    [Header("Grass")]
    public float MaxWaterSaturation;
    public float WaterSaturationRate;
    public float WaterDesaturationRate;
    public float GrassSpreadWaterSaturationThreshold;
    public float GrassSpreadChance;
    public float GrassSpreadCooldown;
    public ParticleSystem Foliage;
    public float FoliageMaxRate;
    
    [Header("Corruption")]
    public float CorruptionSpreadChanceRate;
    public float CorruptionSpreadCooldown;
    public ParticleSystem Evil;
    public float EvilMaxRate;

    [Header("Fire")]
    public float FireSpreadChanceRate;
    public float FireSpreadCooldown;
    public ParticleSystem Fire;
    public float FireMaxRate;

    [Header("General")]
    public Collider Collider;
    public int CurrentState = 0;
    public GameObject[] StateObjects;

    private List<GameplayTile> _neighbors = new List<GameplayTile>();
    private float _waterSaturation;
    private float _spreadCooldown;
    private float _corruptionSpreadChance;
    private float _fireSpreadChance;
    private ParticleSystem.EmissionModule _foliageEmission;
    private ParticleSystem.EmissionModule _evilEmission;
    private ParticleSystem.EmissionModule _fireEmission;

    public const int corruptionState = 0;
    public const int neutralState = 1;
    public const int grassState = 2;

    public System.Action OnStateChanged;

    private const float _neighborDistance = 10f;
    private readonly float[] _neighborAngles =
    {
        0f,
        60f,
        120f,
        180f,
        240f,
        300f
    };

    private void OnEnable()
    {
        ChangeStateByIdx(0);
    }

    protected override void Start()
    {
        base.Start();

        _foliageEmission = Foliage.emission;
        _evilEmission = Evil.emission;
    }

    protected override void Update()
    {
        base.Update();

        switch (CurrentState)
        {
            case grassState:
                SpreadGrass();
                break;
            case corruptionState:
                SpreadCorruption();
                break;
        }
    }

    private void SpreadGrass()
    {
        if (Selected &&
            Rain.Instance.IsRaining())
        {
            if (_waterSaturation < MaxWaterSaturation)
            {
                _waterSaturation += WaterSaturationRate * Time.deltaTime;

                if (_waterSaturation > MaxWaterSaturation)
                {
                    _waterSaturation = MaxWaterSaturation;
                }
            }
        }
        else
        {
            if (_waterSaturation > 0f)
            {
                _waterSaturation -= WaterDesaturationRate * Time.deltaTime;

                if (_waterSaturation < 0f)
                {
                    _waterSaturation = 0f;
                }
            }
        }

        ParticleSystem.MinMaxCurve foliageRate = _foliageEmission.rateOverTime;
        foliageRate.constant = FoliageMaxRate * (_waterSaturation / MaxWaterSaturation);
        _foliageEmission.rateOverTime = foliageRate;

        if (_spreadCooldown > 0f)
        {
            _spreadCooldown -= Time.deltaTime;
            return;
        }

        if (_waterSaturation >= GrassSpreadWaterSaturationThreshold)
        {
            _spreadCooldown = GrassSpreadCooldown;

            if (Random.value <= GrassSpreadChance)
            {
                GameplayTile neighbor = SpreadToNeighbor();
            
                if (neighbor != null)
                {
                    neighbor._spreadCooldown = GrassSpreadCooldown;
                }
            }
        }
    }

    private void SpreadCorruption()
    {
        ParticleSystem.MinMaxCurve evilRate = _evilEmission.rateOverTime;
        evilRate.constant = EvilMaxRate * _corruptionSpreadChance;
        _evilEmission.rateOverTime = evilRate;
        
        if (_spreadCooldown > 0f)
        {
            _spreadCooldown -= Time.deltaTime;
            return;
        }
        
        _spreadCooldown = CorruptionSpreadCooldown;

        if (Random.value <= _corruptionSpreadChance)
        {
            _corruptionSpreadChance = 0f;
            
            GameplayTile neighbor = SpreadToNeighbor();
            
            if (neighbor != null)
            {
                neighbor._spreadCooldown = CorruptionSpreadCooldown;
            }

            return;
        }

        _corruptionSpreadChance += CorruptionSpreadChanceRate;

        if (_corruptionSpreadChance > 1f)
        {
            _corruptionSpreadChance = 1;
        }
    }

    public void SpreadFire()
    {
        ParticleSystem.MinMaxCurve fireRate = _fireEmission.rateOverTime;
        fireRate.constant = EvilMaxRate * _fireSpreadChance;
        _fireEmission.rateOverTime = fireRate;

        if (_spreadCooldown > 0f)
        {
            _spreadCooldown -= Time.deltaTime;
            return;
        }

        _spreadCooldown = FireSpreadCooldown;

        if (Random.value <= _fireSpreadChance)
        {
            _fireSpreadChance = 0f;

            GameplayTile neighbor = SpreadToNeighbor();

            if (neighbor != null)
            {
                neighbor._spreadCooldown = FireSpreadCooldown;
            }

            return;
        }

        _fireSpreadChance += FireSpreadChanceRate;

        if (_fireSpreadChance > 1f)
        {
            _fireSpreadChance = 1;
        }
    }

    public void ChangeStateByIdx(int inIdx)
    {
        CurrentState = inIdx;

        int count = StateObjects.Length;
        for (int idx = 0; idx < count; ++idx)
        {
            StateObjects[idx].SetActive(inIdx == idx ? true : false);
        }

        _spreadCooldown = 0f;
        _waterSaturation = 0f;
        _corruptionSpreadChance = 0f;

        if (OnStateChanged != null) OnStateChanged();
    }

    public void FindNeighbors()
    {
        Collider.enabled = false;
        
        Vector3 position = transform.position;
        foreach (float angle in _neighborAngles)
        {
            Vector3 angleVector = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), 0f, Mathf.Sin(Mathf.Deg2Rad * angle));
            bool foundNeighbor = Physics.Raycast(position, angleVector, out RaycastHit hit, _neighborDistance);

            if (foundNeighbor &&
                hit.collider != null)
            {
                GameplayTile neighborTile = hit.collider.GetComponent<GameplayTile>();

                if (neighborTile != null)
                {
                    _neighbors.Add(neighborTile);
                }
            }
        }
        
        Collider.enabled = true;
    }

    public GameplayTile SpreadToNeighbor()
    {
        List<GameplayTile> randomNeighbors = new List<GameplayTile>(_neighbors);

        while (randomNeighbors.Count > 0)
        {
            int randomIndex = Random.Range(0, randomNeighbors.Count);
            GameplayTile randomNeighbor = randomNeighbors[randomIndex];
            randomNeighbors.RemoveAt(randomIndex);

            if (randomNeighbor == null ||
                randomNeighbor.CurrentState != neutralState)
            {
                continue;
            }
            
            randomNeighbor.ChangeStateByIdx(CurrentState);
            return randomNeighbor;
        }

        return null;
    }
}
