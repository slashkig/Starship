using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static SpaceGame.GameMethods;

public class ButtonMethods : MonoBehaviour
{
    public Enum type;

    void Start()
    {
        gameObject.AddComponent<EventTrigger>();
        if (name.StartsWith("Factory Button")) GetComponent<Button>().onClick.AddListener(FactoryButton);
    }

    private void FactoryButton() => Manager.player.Manufacture(type);
}