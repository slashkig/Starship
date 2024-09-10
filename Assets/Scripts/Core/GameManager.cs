using SpaceGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Extensions.Toolbox;
using static SpaceGame.GameMethods;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    private static List<Vector2Int> generatedAreas = new List<Vector2Int>();
    
    public TextMeshProUGUI statText;
    public GameObject healthBar;
    public Transform resourcesParent;
    public Transform asteroidParent;
    public Ship player;
    [HideInInspector] public SpaceGameObject displayedObject;
    [HideInInspector] public BuildableObject repairingObject;
    [SerializeField] GameObject mainUI;
    [SerializeField] GameObject factoryButton;
    [SerializeField] GameObject factoryButtons;
    [SerializeField] RepairMenu repairButtons;
    [SerializeField] GameObject buildUI;
    [SerializeField] GameObject pauseIcon;
    [SerializeField] List<GameObject> asteroids;
    [SerializeField] Camera map;
    [SerializeField] GameObject mapUI;
    [SerializeField] RectTransform mapButton;
    
    private const int numAsteroids = 750;
    private const int bounds = 400;
    
    public static Mode GameState { get; protected set; } = Mode.Paused;

    public static Vector3 WorldMousePos => (GameState == Mode.Map ? Manager.Map : Camera.main).ScreenToWorldPoint(Input.mousePosition);
    
    public Camera Map => map;

    void Awake()
    {
        Time.timeScale = 0;
        CreateFactoryButtons(recipies.Keys.ToList());
        TryGenArea(0, 0);
        Time.timeScale = 1;
        GameState = Mode.Map;
        ChangeView();
    }

    void Update()
    {
        if (GameState != Mode.Paused)
        {
            if (Input.GetKeyDown(KeyCode.M)) ChangeView();
            if (Pausing()) StartCoroutine(Pause());
            if (player != null) CheckArea();
        }
    }

    public void ChangeView()
    {
        switch (GameState)
        {
            case Mode.Paused:
                return;
            case Mode.Playing:
                mapButton.anchorMin = Vector2.zero;
                GameState = Mode.Map;
                break;
            case Mode.Map:
                mapButton.anchorMin = new Vector2(0.8f, 0.7f);
                GameState = Mode.Playing;
                break;
        }
        map.rect = new Rect(mapButton.anchorMin, Vector2.one);
        mapButton.sizeDelta = Vector2.zero;
        DisplayStats();
    }

    public void DisplayStats(ObjectCollider target = null, bool showHealth = false, bool showResources = false)
    {
        if (player.powered)
        {
            statText.gameObject.SetActive(target != null);
            if (target != null) displayedObject = target;
            if (showHealth)
            {
                statText.text = "";
                foreach (Health type in target.health.Keys)
                {
                    statText.text += $"\n{type}: {target.health[type]}";
                }
            }
            if (showResources)
            {
                foreach (Resource resource in target.resourceValue.Keys)
                {
                    statText.text += $"\n{names[resource]}: {target.resourceValue[resource]}";
                }
            }
        }
    }

    public void DisplayStats(Debris target)
    {
        if (player.powered)
        {
            statText.gameObject.SetActive(true);
            //displayedObject = target;
            statText.text = names[target.type];
        }
    }

    public void DisplayRepair(BuildableObject target = null)
    {
        repairButtons.gameObject.SetActive(target != null);
        repairingObject = target;
        if (target != null)
        {
            factoryButtons.SetActive(false);
            buildUI.SetActive(false);
            repairButtons.OnDisplay(target);
        }
    }

    public virtual void SetPlayerUI(bool active)
    {
        mapButton.gameObject.SetActive(active);
        mapUI.SetActive(!active);
        statText.text = "";
    }

    public IEnumerator Pause()
    {
        Time.timeScale = 0;
        Mode pauseState = GameState;
        GameState = Mode.Paused;
        pauseIcon.SetActive(true);
        yield return new WaitWhile(Pausing);
        yield return new WaitUntil(Pausing);
        Time.timeScale = 1;
        GameState = pauseState;
        pauseIcon.SetActive(false);
    }

    public void ManufactureMenu()
    {
        if (GameState != Mode.Paused)
        {
            repairButtons.gameObject.SetActive(false);
            buildUI.SetActive(false);
            factoryButtons.SetActive(!factoryButtons.activeInHierarchy);
        }
    }

    public void BuildMode()
    {
        if (GameState == Mode.Build)
        {
            GameState = Mode.Playing;
            buildUI.SetActive(false);
        }
        else if (GameState != Mode.Paused)
        {
            GameState = Mode.Build;
            factoryButtons.SetActive(false);
            repairButtons.gameObject.SetActive(false);
            buildUI.SetActive(true);
        }
    }

    public void SetMenu()
    {
        if (GameState == Mode.Build)
        {
            bool active = !buildUI.activeInHierarchy;
            buildUI.SetActive(active);
            mainUI.SetActive(active);
        }
    }

    public void RepairObject(int type)
    {
        if (GameState != Mode.Paused) repairingObject.Repair((Health)type, 1);
    }

    protected void CreateFactoryButtons(List<Enum> types)
    {
        Rect buttonRect = ((RectTransform)factoryButton.transform).rect;
        for (int i = 0; i < types.Count; i++)
        {
            GameObject button = Instantiate(factoryButton, factoryButtons.transform);
            ((RectTransform)button.transform).anchoredPosition =
                new Vector2((buttonRect.width + 20) * (i - (types.Count - 1) * 0.5f), buttonRect.height / 2 + 10);
            button.GetComponent<ButtonMethods>().type = types[i];
            button.GetComponentInChildren<Text>().text = types[i] is Resource resource ? names[resource] : types[i].ToString();
        }
    }
    
    private void CheckArea()
    {
        Vector3Int shipArea = Vector3Int.FloorToInt((player.center.position + new Vector3(bounds, bounds)) / (bounds * 2));
        Vector3 shipAreaPos = player.center.position - shipArea * bounds * 2;
        Vector2Int newArea = new Vector2Int(shipArea.x, shipArea.y);
        
        if (Mathf.Abs(shipAreaPos.x) > bounds / 2) TryGenArea(newArea.x += (int)Mathf.Sign(shipAreaPos.x), shipArea.y);
        if (Mathf.Abs(shipAreaPos.y) > bounds / 2) TryGenArea(shipArea.x, newArea.y += (int)Mathf.Sign(shipAreaPos.y));
        if (shipArea.x != newArea.x && shipArea.y != newArea.y) TryGenArea(newArea.x, newArea.y);
    }

    private void TryGenArea(int row, int col)
    {
        Vector2Int area = new Vector2Int(row, col);
        if (!generatedAreas.Contains(area))
        {
            for (int i = 0; i < numAsteroids * Random.Range(0.9f, 1.1f); i++)
            {
                Instantiate(asteroids[(i % 60 == 0) ? 2 : (i % 9 == 1) ? 1 : 0], new Vector3(Range(1f) + 2 * row, Range(1f) +
                    2 * col) * bounds, transform.rotation, asteroidParent);
            }
            generatedAreas.Add(area);
        }
    }

    private bool Pausing() => Input.GetKeyDown(KeyCode.P);
}