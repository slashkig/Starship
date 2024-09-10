using SpaceGame;
using System;
using System.Collections;
using UnityEngine;
using static Extensions.Toolbox;
using static SpaceGame.GameMethods;

public class Pickup : MonoBehaviour, ObjectPooler.IPooledObject
{
    public ObjectPooler Pooler { get; set; }

    public Rigidbody2D selfRb;
    [HideInInspector] public float value;
    [HideInInspector] public Enum type = null;
    [SerializeField] SpriteRenderer appearance;

    public void Pool(Vector3 position, Quaternion rotation)
    {
        appearance.sprite = null;
        gameObject.SetActive(true);
        StartCoroutine(Activate(position, rotation));
    }

    public void Depool()
    {
        type = null;
        gameObject.SetActive(false);
        selfRb.isKinematic = true;
        Pooler.poolAvailable.Add(this);
        transform.localScale = new Vector3(0.5f, 0.5f, 1);
        appearance.color = Color.white;
    }

    IEnumerator Activate(Vector3 position, Quaternion rotation)
    {
        yield return new WaitUntil(() => type != null);
        appearance.sprite = ObjManager.circle;
        appearance.color = SetAppearance();
        transform.SetPositionAndRotation(position, rotation);
        selfRb.isKinematic = false;
        yield return new WaitForSeconds(200);
        Depool();
    }

    private Color SetAppearance()
    {
        switch (type)
        {
            case Resource.Mineral:
                return new Color(0.47f, 0.43f, 0.39f);
            case Resource.Ore:
                return new Color(0.47f, 0.315f, 0.295f);
            case Resource.Metal:
                return new Color(0.3f, 0.3f, 0.3f);
            case Resource.Power:
                return new Color(1, 1, 0.34f);
            case Resource.Alloy:
                return new Color(0.23f, 0.32f, 0.34f);
            case Resource.Fuel:
                SetAppearance(ObjManager.square, Vector2.one * 0.5f);
                return new Color(0.66f, 0.47f, 0.11f);
            case Ammo.Cell:
                SetAppearance(ObjManager.hexagon, Vector2.one * 0.4f);
                return new Color(1, 0.7f, 0);
            case Ammo.Missile:
                SetAppearance(ObjManager.bullet, new Vector2(0.6f, 1.2f));
                return new Color(0.5f, 0.5f, 0.55f);
            case Ammo.Shell:
                SetAppearance(ObjManager.square, Vector2.one * 0.7f);
                return new Color(0.3f, 0.3f, 0.3f);
            default:
                return Color.white;
        }
    }

    private void SetAppearance(Sprite shape, Vector2 scale)
    {
        appearance.sprite = shape;
        selfRb.AddTorque(Range(20));
        transform.localScale = scale;
    }
}
