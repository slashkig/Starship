using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using static SpaceGame.GameMethods;

public class TestManager : GameManager
{
    public PositionConstraint viewTarget;
    public Transform ship;
    public bool playerDead = false;
    
    void Awake()
    {
        CreateFactoryButtons(recipies.Keys.ToList());
        GameState = SpaceGame.Mode.Playing;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) StartCoroutine(Pause());
        if (Input.GetKeyDown(KeyCode.T) && !playerDead)
        {
            if (viewTarget.GetSource(0).sourceTransform == ship)
            {
                viewTarget.SetSource(0, AimSource(transform));
                Camera.main.orthographicSize = 5;
                player.playerNumber = 2;
                player = GetComponent<Ship>();
                player.playerNumber = 1;
                ship.GetComponentInChildren<Engine>().enabled = false;
                GetComponentInChildren<SimpleMovement>().enabled = true;
            }
            else
            {
                viewTarget.SetSource(0, AimSource(ship));
                Camera.main.orthographicSize = 15;
                player.playerNumber = 2;
                player = ship.GetComponent<Ship>();
                player.playerNumber = 1;
                ship.GetComponentInChildren<Engine>().enabled = true;
                GetComponentInChildren<SimpleMovement>().enabled = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            foreach (Ship ship in FindObjectsOfType<Ship>())
            {
                ship.playerNumber = 3;
                ship.Target = null;
            }
        }
    }

    public override void SetPlayerUI(bool active) => playerDead = !active;
}
