using Extensions.Enums;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SpaceGame.GameMethods;
using Enum = System.Enum;

public class BuildManager : MonoBehaviour
{
    [HideInInspector] public Ship ship;
    [SerializeField] SpriteRenderer self;
    private Color validSpace;
    private Color invalidSpace;
    private Vector3 offset;
    private bool needToMove;
    private bool external;

    public bool ValidSpace { get; private set; }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            StopAllCoroutines();
            gameObject.SetActive(false);
        }
        else if (needToMove)
        {
            transform.localPosition = ship.WorldToLocalCell(GameManager.WorldMousePos) + offset;
            self.color = (ValidSpace = external ? ship.AdjacentSpaces(transform.position) > 0 && !ship.tilemap.HasCell(transform
                .position) : ship.DisplayedSpace?.damageHierarchy?.Count == 1) ? validSpace : invalidSpace;
        }
    }

    void OnMouseEnter() => needToMove = false;

    void OnMouseExit() => needToMove = true;

    private void OnEnable() => Manager.SetMenu();

    private void OnDisable() => Manager.SetMenu();

    public IEnumerator BuildUI(ShipComponent prefab)
    {
        SpriteRenderer target = prefab.GetComponent<SpriteRenderer>();
        validSpace = target.color * new Color(0, 1, 0, 0.6f);
        invalidSpace = target.color * new Color(1, 0, 0, 0.6f);
        self.sprite = target.sprite;
        transform.localScale = prefab.transform.localScale;
        offset = prefab.transform.position;
        external = prefab is ExternalComponent;
        needToMove = true;
        yield return new WaitWhile(() => Input.GetMouseButtonDown(0));
        gameObject.SetActive(true);
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0) && ValidSpace);
        if (ship.TryUpdateResources(prefab.Stats.cost, -1, SpaceGame.IterBehavior.EnsureSuccessBeforeRunning))
            { Instantiate(prefab.gameObject, transform.position, transform.rotation, ship.transform); }
        gameObject.SetActive(false);
    }
}