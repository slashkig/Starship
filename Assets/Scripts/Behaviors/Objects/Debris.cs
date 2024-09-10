using SpaceGame;
using System.Collections;
using UnityEngine;
using static Extensions.Toolbox;
using static Extensions.TransformMethods;
using static SpaceGame.GameMethods;

public class Debris : MonoBehaviour, ObjectPooler.IPooledObject
{
    public ObjectPooler Pooler { get; set; }
    
    public Rigidbody2D selfRb;
    public Collider2D self;
    [HideInInspector] public float value;
    [HideInInspector] public Resource? type = null;
    [SerializeField] SpriteRenderer appearance;


    void OnCollisionEnter2D(Collision2D collision) => Depool();

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (TryGetComponent(out Projectile projectile))
        {
            projectile.OnHit();
            Depool();
        }
    }

    void OnParticleCollision(GameObject other) => Depool();

    public void Pool(Vector3 position, Quaternion rotation) => StartCoroutine(Activate(position, Quaternion.Euler(0, 0, Range(180))));

    public void Depool()
    {
        gameObject.SetActive(false);
        while (value > 0)
        {
            Vector3 randPos = transform.RandPos(self);
            Pickup piece = (Pickup)ObjManager.resourcePooler.Pool(transform.position + randPos, transform.rotation);
            piece.selfRb.AddForce(randPos);
            piece.type = type;
            value -= piece.value = value <= 5 ? value : UnityEngine.Random.Range(3, 5);
        }

        selfRb.isKinematic = true;
        Pooler.poolAvailable.Add(this);
        appearance.color = Color.white;
    }

    IEnumerator Activate(Vector3 position, Quaternion rotation)
    {
        yield return new WaitUntil(() => type != null);
        transform.SetPositionAndRotation(position, rotation);
        selfRb.isKinematic = false;
        selfRb.AddTorque(Range(20));
        gameObject.SetActive(true);
        appearance.color = type switch
        {
            Resource.Mineral => new Color(0.47f, 0.43f, 0.39f),
            Resource.Ore => new Color(0.47f, 0.315f, 0.295f),
            Resource.Metal => new Color(0.3f, 0.3f, 0.3f),
            Resource.Power => new Color(1, 1, 0.34f),
            Resource.Alloy => new Color(0.23f, 0.32f, 0.34f),
            Resource.Fuel => new Color(0.66f, 0.47f, 0.11f),
            _ => Color.white,
        };
    }
}