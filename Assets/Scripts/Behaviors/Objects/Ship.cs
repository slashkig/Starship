using Extensions;
using Extensions.Enums;
using SpaceGame;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Extensions.Toolbox;
using static SpaceGame.GameMethods;
using static UnityEngine.Input;

public class Ship : BuildableObject
{
    public Dictionary<Vector2Int, ShipSpace> spaces = new Dictionary<Vector2Int, ShipSpace>();
    public Tilemap tilemap;
    public Transform center;
    public bool powered = true;
    public int playerNumber;
    [HideInInspector] public float forwardSpeed = 0;
    [HideInInspector] public float turnSpeed = 0;
    [HideInInspector] public int numEngines = 0;
    [SerializeField] TextMeshProUGUI resourceText;
    [SerializeField] TextMeshProUGUI messageText;
    [SerializeField] Rigidbody2D selfRb;
    [SerializeField] BuildManager builder;
    private bool canCollect = true;
    private float energyUsed = 0;
    private float fuelUsed = 0;
    private Transform target = null;
    
    public Action OnTargetChanged = delegate {};
    public Action OnShipDeath = delegate {};
    public Action<bool> SetMoving;

    private const int speedFactor = 10000;

    public Dictionary<Enum, float> Resources { get; protected set; } = new Dictionary<Enum, float>()
    {
        [Resource.Alloy] = 0,
        [Resource.Power] = 100,
        [Resource.Fuel] = 60,
        [Resource.Metal] = 10,
        [Resource.Mineral] = 10,
        [Resource.Ore] = 0,
        [Ammo.Shell] = 30,
        [Ammo.Missile] = 200,
        [Ammo.Cell] = 30
    };

    public Transform Target { get => target; set { target = value; OnTargetChanged(); } }

    public ShipSpace DisplayedSpace { get; private set; } = null;

    public bool IsMoving { get; private set; } = false;

    private float turn => -GetAxis("Horizontal");

    private float thrust => Mathf.Max(-0.2f, GetAxis("Vertical")) + Mathf.Abs(turn) * 0.1f;

    protected override void OnCreate()
    {
        SetMoving = (bool set) => IsMoving = set;
        base.OnCreate();
        healthBar.transform.SetParent(center);
        healthBar.transform.localPosition = Vector3.forward;
        healthBar.transform.localScale = new Vector3(0.4f, 0.6f, 1);
        if (playerNumber == 2)
        {
            OnTargetChanged += () => { if (numEngines > 0) SetMoving(true); };
            TryUpdateResources(new List<EnumPair<Enum>>() { (Ammo.Cell, 220), (Ammo.Missile, 50), (Ammo.Shell, 220), (Resource.Power, 200) });
        }
        else
        {
            builder.ship = this;
            UpdateResources(10, Resource.Mineral);
        }
    }

    void Update()
    {
        if (target != null && center.Distance(target) > 120) { Target = null; }
        else if (playerNumber == 2 && Manager.player != null && target == null && center.Distance(Manager.player.center) <= 100)
            { Target = Manager.player.center; }
        if (GameManager.GameState != Mode.Paused)
        {
            if (Resources[Resource.Power] > 0 && powered)
            {
                energyUsed += 0.005f;
                if (GameManager.GameState == Mode.Playing && playerNumber == 1)
                {
                    if (GetKeyDown(KeyCode.N)) Manufacture(Resource.Metal);
                    if (GetMouseButtonDown(1) && !Manager.statText.IsActive()) Target = null;
                    
                    if (numEngines > 0 && ship.Resources[Resource.Fuel] >= 0.1f && (thrust != 0 || turn != 0))
                    {
                        selfRb.AddForce(forwardSpeed * speedFactor * thrust * Time.deltaTime * transform.up);
                        selfRb.AddTorque(turn * turnSpeed * (speedFactor / 100) * Time.deltaTime, ForceMode2D.Impulse);
                        
                        fuelUsed += numEngines * (Mathf.Abs(thrust) + Mathf.Abs(turn / 5)) / 20;
                        energyUsed += numEngines * (Mathf.Abs(thrust) + Mathf.Abs(turn)) / 100;

                        if (thrust > 0 && !IsMoving) SetMoving(true);
                        else if (thrust <= 0 && IsMoving) SetMoving(false);
                    }
                    else if (IsMoving) { SetMoving(false); }
                }
                else if (playerNumber == 1 && IsMoving) { SetMoving(false); }
            }
            else if (playerNumber == 1)
            {
                if (powered)
                {
                    Target = null;
                    SetMoving(false);
                    Manager.SetPlayerUI(powered = false);
                    resourceText.gameObject.SetActive(false);
                    messageText.text = "Alert: No Power!";
                    messageText.color = new Color(1, 0.5f, 0);
                    messageText.gameObject.SetActive(true);
                    Camera.main.cullingMask = 0b1100110111;
                }
                else if (Resources[Resource.Power] > 0)
                {
                    Manager.SetPlayerUI(powered = true);
                    resourceText.gameObject.SetActive(true);
                    messageText.gameObject.SetActive(false);
                    Camera.main.cullingMask = 0b1101110111;
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (energyUsed >= 1) { UpdateResources(-Mathf.Floor(energyUsed) / 10, Resource.Power); energyUsed %= 1; }
        if (fuelUsed >= 1) { UpdateResources(-Mathf.Floor(fuelUsed) / 10, Resource.Fuel); fuelUsed %= 1; }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (canCollect)
        {
            if (collision.TryGetComponent(out Pickup pickup))
            {
                UpdateResources(pickup.value, pickup.type);
                pickup.Depool();
            }
            else if (collision.TryGetComponent(out ResourcePackage package))
            {
                EnumMethods.AddToDict(Resources, package.resources);
                UpdateResources(0, null);
                Destroy(collision.gameObject);
            }
        }
    }

    void OnParticleCollision(GameObject other)
    {
        if (other.TryGetComponent(out ParticleBeam particles))
        {
            var collisions = new List<ParticleCollisionEvent>(1);
            particles.beam.GetCollisionEvents(gameObject, collisions);
            GetSpaceOnEdge(collisions[0].intersection).TakeDamage(particles.damage, particles.type);
        }
    }

    void OnMouseOver()
    {
        ShipSpace tileUnder = GetSpace(GameManager.WorldMousePos);
        if (Manager.displayedObject == null || DisplayedSpace != tileUnder) { (DisplayedSpace = tileUnder).Display(true); }
    }

    void OnMouseExit()
    {
        Manager.DisplayStats();
        DisplayedSpace = null;
    }

    public void Build(ShipComponent prefab)
    {
        if (prefab.Stats.cost.TrueForAll((EnumPair<Resource> pair) => Resources[pair.type] >= pair.value))
            { StartCoroutine(builder.BuildUI(prefab)); }
    }

    protected override void Die()
    {
        canCollect = false;
        OnShipDeath += base.Die;
        if (playerNumber == 1)
        {
            Camera.main.transform.SetParent(Manager.transform);
            Transform mainCanvas = resourceText.transform.parent;
            Manager.SetPlayerUI(false);
            for (int i = 0; i < mainCanvas.childCount; i++)
            {
                mainCanvas.GetChild(i).gameObject.SetActive(false);
            }
            messageText.text = "YOU DIED";
            messageText.color = Color.red;
            messageText.gameObject.SetActive(true);
        }
        
        Instantiate(ObjManager.resourceBoxPrefab, center.position, transform.rotation).resources = Resources;
        OnShipDeath();
    }

    public ShipSpace GetSpace(Vector2 worldPos) => spaces[(Vector2Int)tilemap.WorldToCell(worldPos)];

    public ShipSpace GetSpaceOnEdge(Vector2 worldPos) => spaces[(Vector2Int)tilemap.EdgeToCell(self.ClosestPoint(worldPos), worldPos)];

    public Vector3 WorldToLocalCell(Vector3 worldPos) => Vector3Int.RoundToInt(tilemap.CellToLocal(tilemap.WorldToCell(worldPos))) + new Vector3(0.5f, 0.5f);

    public int AdjacentSpaces(Vector3 worldPos)
    {
        int count = 0;
        Vector3Int position = tilemap.WorldToCell(worldPos);
        foreach (Vector3Int adjacent in new Vector3Int[] { Vector3Int.left, Vector3Int.right, Vector3Int.down, Vector3Int.up })
        {
            if (spaces.TryGetValue((Vector2Int)(position + adjacent), out ShipSpace space) && space.damageHierarchy?.Count > 0)
                { count++; }
        }
        return count;
    }

    public bool TryUpdateResources<E>(List<EnumPair<E>> resourceList, float factor = 1, IterBehavior failBehavior =
    IterBehavior.DontCheckForFail) where E : Enum {
        try
        {
            if (failBehavior == IterBehavior.DontCheckForFail)
            {
                resourceList.ForEach((EnumPair<E> pair) => UpdateResources(pair * factor, pair, false));
                return true;
            }
            return resourceList.IterThrough((EnumPair<E> pair) => UpdateResources(pair * factor, pair, false),
                (EnumPair<E> pair) => pair * factor >= 0 || Resources[pair] + pair * factor >= 0, failBehavior);
        }
        finally { UpdateResources(0, null); }
    }

    public void UpdateResources(float amount, Enum type, bool update = true)
    {
        if (type != null) Resources[type] = RoundToTenths(Resources[type] + amount);
        if (update && powered && playerNumber == 1)
        {
            resourceText.text = "";
            foreach (EnumPair<Enum> pair in Resources)
            {
                if (pair.value > 0) resourceText.text += $"{names[pair.type]}: {pair.value}\n";
            }
        }
    }

    public void Manufacture(Enum product)
    {
        int factor = GetKey(KeyCode.LeftShift) || GetKey(KeyCode.RightShift) ? 10 : 1;
        bool isPower = product as Resource? == Resource.Power;
        if ((Resources[Resource.Power] > 0.2f * factor || isPower) && TryUpdateResources(recipies[product].ToList(),
        -factor, IterBehavior.EnsureSuccessBeforeRunning)) {
            if (isPower) UpdateResources(factor * 3, Resource.Power);
            else TryUpdateResources(new List<EnumPair<Enum>> { (product, factor), (Resource.Power, -0.2f * factor) });
        }
    }
}