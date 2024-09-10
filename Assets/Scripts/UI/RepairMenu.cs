using TMPro;
using UnityEngine;

public class RepairMenu : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] GameObject armorButton;
    
    public void OnDisplay(BuildableObject target)
    {
        nameText.text = target.gameObject.name + (target is ModularComponent modular && modular.defaultComponent != null ?
            $" - {modular.defaultComponent.name}" : "");
        armorButton.SetActive(target.stats.startingHealth[1].value != 0);
    }
}