using System.Collections.Generic;
using UnityEngine;

public class GameplayTile : BaseTile
{
    [Header("Forest")]
    public float ForestSpreadChance;
    public float ForestSpreadCooldown;
    
    [Header("Grass")]
    public float MaxWaterSaturation;
    public float WaterDesaturationRate;
    public float GrassSpreadWaterSaturationThreshold;
    public float GrassSpreadChance;
    public float GrassSpreadCooldown;
    public ParticleSystem Foliage;
    public float FoliageMaxRate;
    public float ChangeToForestSaturationThreshold;
    public float ChangeToForestChance;
    public float ChangeToForestCooldown;
    
    [Header("Corruption")]
    public float CorruptionSpreadChanceRate;
    public float CorruptionSpreadCooldown;
    public ParticleSystem Evil;
    public float EvilMaxRate;
    public int CorruptionDamage;

    [Header("Fire")]
    public float FireSpreadCooldown;
    public ParticleSystem Fire;
    public float FireMaxRate;
    public float FireSpreadChanceReductionRate;
    public float FireSpreadChanceAddition;
    public float FireSpreadChanceStart;
    public float FireTickCooldown;
    public int FireTickDamage;

    [Header("General")]
    public List<int> MaxHealthByState;
    public float HealingCooldown;
    public ParticleSystem Smoke;
    public float SmokeMaxRate;
    public Collider Collider;
    public int CurrentState = 0;
    public GameObject[] StateObjects;

    private List<GameplayTile> _neighbors = new List<GameplayTile>();
    private float _waterSaturation;
    private float _spreadCooldown;
    private float _corruptionSpreadChance;
    private float _fireSpreadChance;
    private float _fireSpreadCooldown;
    private float _changeToForestCooldown;
    private ParticleSystem.EmissionModule _foliageEmission;
    private ParticleSystem.EmissionModule _evilEmission;
    private ParticleSystem.EmissionModule _fireEmission;
    private ParticleSystem.EmissionModule _smokeEmission;
    private int _health;
    private float _fireTickCooldown;
    private float _healingTickCooldown;

    public const int corruptionState = 0;
    public const int neutralState = 1;
    public const int grassState = 2;
    public const int forestState = 3;

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
        _fireEmission = Fire.emission;
        _smokeEmission = Smoke.emission;
    }

    protected override void Update()
    {
        base.Update();

        if (!GameManager.Instance.InPlay) return;

        switch (CurrentState)
        {
            case forestState:
                SpreadForest();
                SpreadFire();
                CheckHealing(Selected && Rain.Instance.IsRaining(), Rain.Instance.WaterHealAmount);
                CheckHealth();
                break;
            case grassState:
                SpreadGrass();
                SpreadFire();
                CheckHealing(Selected && Rain.Instance.IsRaining(), Rain.Instance.WaterHealAmount);
                CheckHealth();
                break;
            case corruptionState:
                SpreadCorruption();
                SpreadFire();
                CheckHealth();
                break;
        }
    }

    private void SpreadForest()
    {
        if (_spreadCooldown > 0f)
        {
            _spreadCooldown -= Time.deltaTime;
        }
        else
        {
            _spreadCooldown = ForestSpreadCooldown;

            if (Random.value <= ForestSpreadChance)
            {
                GameplayTile neighbor = SpreadToNeighbor(overrideState: grassState);
            
                if (neighbor != null)
                {
                    neighbor._spreadCooldown = GrassSpreadCooldown;
                }
            }
        }
    }

    private void SpreadGrass()
    {
        if (Selected &&
            Rain.Instance.IsRaining())
        {
            if (_waterSaturation < MaxWaterSaturation)
            {
                _waterSaturation += Rain.Instance.WaterSaturationRate * Time.deltaTime;

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
        }
        else if (_waterSaturation >= GrassSpreadWaterSaturationThreshold)
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

        if (_changeToForestCooldown > 0f)
        {
            _changeToForestCooldown -= Time.deltaTime;
        }
        else if (_waterSaturation >= ChangeToForestSaturationThreshold)
        {
            _changeToForestCooldown = ChangeToForestCooldown;
            
            if (Random.value <= ChangeToForestChance)
            {
                ChangeStateByIdx(forestState);
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
            
            GameplayTile neighbor = SpreadToNeighbor(CorruptionDamage);
            
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

    private void SpreadFire()
    {
        if (Selected &&
            Cloud.Instance.IsLightning)
        {
            if (Random.value <= Cloud.Instance.LightningChanceToStartFire)
            {
                if (_fireSpreadChance > 0f)
                {
                    _fireSpreadChance = FireSpreadChanceAddition;
                }
                else
                {
                    _fireSpreadChance = FireSpreadChanceStart;
                }
            }

            TakeDamage(Cloud.Instance.LightningDamage);
        }

        ParticleSystem.MinMaxCurve fireRate = _fireEmission.rateOverTime;
        fireRate.constant = FireMaxRate * _fireSpreadChance;
        _fireEmission.rateOverTime = fireRate;

        if (_fireSpreadChance > 0f)
        {
            if (_fireTickCooldown <= 0f)
            {
                TakeDamage(FireTickDamage);
                _fireTickCooldown = FireTickCooldown;
            }
            else
            {
                _fireTickCooldown -= Time.deltaTime;
            }
        }

        ParticleSystem.MinMaxCurve smokeRate = _smokeEmission.rateOverTime;
        float healthRatio = ((float) _health / (float) MaxHealthByState[CurrentState]);
        smokeRate.constant = SmokeMaxRate * (1 - healthRatio);
        _smokeEmission.rateOverTime = smokeRate;
        
        if (_fireSpreadChance <= 0f)
        {
            return;
        }

        if (_fireSpreadCooldown > 0f)
        {
            _fireSpreadCooldown -= Time.deltaTime;
        }
        else
        {
            _fireSpreadCooldown = FireSpreadCooldown;
        
            if (Random.value <= _fireSpreadChance)
            {
                SpreadFireToNeighbor();
            }
        }

        _fireSpreadChance -= FireSpreadChanceReductionRate * Time.deltaTime;

        if (Selected &&
            Rain.Instance.IsRaining())
        {
            _fireSpreadChance -= Rain.Instance.WaterFireSuppressRate * Time.deltaTime;
        }
    }

    private void CheckHealth()
    {
        if (_health <= 0f)
        {
            ChangeStateByIdx(neutralState);
        }
    }

    private void CheckHealing(bool canHeal, int healAmount)
    {
        if (_healingTickCooldown > 0f)
        {
            _healingTickCooldown -= Time.deltaTime;
            return;
        }

        if (canHeal)
        {
            _health += healAmount;
            
            if (_health > MaxHealthByState[CurrentState])
            {
                _health = MaxHealthByState[CurrentState];
            }
            
            _healingTickCooldown = HealingCooldown;
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
        _fireSpreadChance = 0f;
        _fireSpreadCooldown = 0f;
        _fireTickCooldown = 0f;
        _changeToForestCooldown = 0f;
        _health = MaxHealthByState[CurrentState];
        
        _fireEmission = Fire.emission;
        _smokeEmission = Smoke.emission;
        
        ParticleSystem.MinMaxCurve fireRate = _fireEmission.rateOverTime;
        fireRate.constant = FireMaxRate * _fireSpreadChance;
        _fireEmission.rateOverTime = fireRate;
        
        ParticleSystem.MinMaxCurve smokeRate = _smokeEmission.rateOverTime;
        float healthRatio = ((float) _health / (float) MaxHealthByState[CurrentState]);
        smokeRate.constant = SmokeMaxRate * (1 - healthRatio);
        _smokeEmission.rateOverTime = smokeRate;
        
        Fire.Clear();
        Smoke.Clear();
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

    private GameplayTile SpreadToNeighbor(int damage = 0, int overrideState = -1)
    {
        List<GameplayTile> randomNeighbors = new List<GameplayTile>(_neighbors);

        while (randomNeighbors.Count > 0)
        {
            int randomIndex = Random.Range(0, randomNeighbors.Count);
            GameplayTile randomNeighbor = randomNeighbors[randomIndex];
            randomNeighbors.RemoveAt(randomIndex);

            if (randomNeighbor == null || 
                randomNeighbor.CurrentState == CurrentState ||
                randomNeighbor.CurrentState == overrideState)
            {
                continue;
            }

            if (randomNeighbor.CurrentState == neutralState)
            {
                randomNeighbor.ChangeStateByIdx(overrideState != -1 ? overrideState : CurrentState);
            }
            else if (damage > 0)
            {
                randomNeighbor.TakeDamage(damage);
            }
            
            return randomNeighbor;
        }

        return null;
    }

    private void SpreadFireToNeighbor()
    {
        List<GameplayTile> randomNeighbors = new List<GameplayTile>(_neighbors);

        while (randomNeighbors.Count > 0)
        {
            int randomIndex = Random.Range(0, randomNeighbors.Count);
            GameplayTile randomNeighbor = randomNeighbors[randomIndex];
            randomNeighbors.RemoveAt(randomIndex);

            if (randomNeighbor == null)
            {
                continue;
            }

            if (randomNeighbor._fireSpreadChance > 0f)
            {
                randomNeighbor._fireSpreadChance += FireSpreadChanceAddition;
            }
            else
            {
                randomNeighbor._fireSpreadChance = FireSpreadChanceStart;
            }

            randomNeighbor._fireSpreadCooldown = FireSpreadCooldown;
            break;
        }
    }

    private void TakeDamage(int damage)
    {
        _health -= damage;
        _corruptionSpreadChance = 0f;
        _waterSaturation = 0f;
    }
}
