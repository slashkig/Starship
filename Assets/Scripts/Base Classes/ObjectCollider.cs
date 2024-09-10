using Extensions.Enums;
using SpaceGame;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;
using static Extensions.Toolbox;
using static SpaceGame.GameMethods;

/// <summary> The root class for all destroyable objects in the game. </summary>
public abstract class ObjectCollider : SpaceGameObject
{
    public ObjectStats stats;
    public Dictionary<Health, float> health = new Dictionary<Health, float>();
    [HideInInspector] public int id = 0;
    protected Slider[] healthSliders;
    protected GameObject healthBar;

    protected override void OnCreate()
    {
        healthBar = Instantiate(Manager.healthBar, transform.position, transform.rotation, transform);
        healthBar.transform.localScale = new Vector3(0.4f, 0.2f, 1);
        healthSliders = healthBar.GetComponentsInChildren<Slider>();
        RotationConstraint constraint = healthBar.GetComponent<RotationConstraint>();
        constraint.AddSource(AimSource(Camera.main.transform));
        constraint.constraintActive = constraint.locked = true;
        healthBar.SetActive(false);
        foreach (EnumPair<Health> enumPair in stats.startingHealth)
        {
            EnumMethods.AddToDict(enumPair, ref health);
            Slider healthBar = healthSliders[(int)enumPair.type];
            healthBar.value = healthBar.maxValue = enumPair.value;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.otherCollider.gameObject == gameObject) TakeDamage(collision.relativeVelocity.magnitude, Damage.Kinetic);
    }

    void OnParticleCollision(GameObject other)
    {
        if (other.TryGetComponent(out ParticleBeam particles)) TakeDamage(particles.damage, particles.type);
    }

    void OnMouseUpAsButton()
    {
        if (GameManager.GameState != Mode.Paused && GameManager.GameState != Mode.Build)
        {
            if (Manager.player.id != id)
            {
                Manager.player.Target = Manager.displayedObject is Ship ship ? ship.center : Manager.displayedObject.transform;
                if (Manager.displayedObject != null) Manager.DisplayRepair();
            }
            else
            {
                Manager.DisplayRepair(Manager.displayedObject as BuildableObject);
            }
        }
    }

    public override void Display(bool show = true)
    {
        if (GameManager.GameState != Mode.Paused)
        {
            if (!show) Manager.DisplayStats();
            else Manager.DisplayStats(this, true, this is Asteroid);
            Displayed = show;
        }
    }
    
    public bool TakeDamage(float damage, Damage damageType)
    {
        float currentDamage = damage;
        if (!healthBar.activeSelf && currentDamage > 0) healthBar.SetActive(true);
        foreach (EnumPair<Health> pair in damageMods[damageType])
        {
            (Health type, float value) = pair.Split();
            if (health[type] == 0)
            {
                continue;
            }
            else if (health[type] < currentDamage * value)
            {
                currentDamage -= health[type] / value;
                healthSliders[(int)type].value = health[type] = 0;
            }
            else
            {
                healthSliders[(int)type].value = health[type] = RoundToTenths(health[type] - currentDamage * value);
                currentDamage = 0;
                break;
            }
        }
        if (health[Health.Hull] <= 0)
        {
            Die();
            return true;
        }
        else if (Displayed)
        {
            Display();
        }
        return false;
    }
}