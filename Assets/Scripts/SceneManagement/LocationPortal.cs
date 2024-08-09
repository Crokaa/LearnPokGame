using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LocationPortal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] Transform spawnPoint;
    [SerializeField] DestinationIdentifier destinationPortal;

    PlayerController player;
    Fader fader;

    public Transform SpawnPoint
    {
        get { return spawnPoint; }
    }

    private void Start()
    { 
        fader = FindObjectOfType<Fader>();
    }

    public void OnPlayerTriggered(PlayerController player)
    {
        StartCoroutine(Teleport());
        this.player = player;
    }

    public bool KeepTriggering => false;

    IEnumerator Teleport()
    {

        GameController.Instance.PauseGame(true);
        yield return fader.FadeIn(0.5f);


        var destPortal = FindObjectsOfType<LocationPortal>().First(x => x != this && x.destinationPortal == destinationPortal);
        player.Character.SetPositionAndSnapToTile(destPortal.spawnPoint.position);

        yield return fader.FadeOut(0.5f);
        GameController.Instance.PauseGame(false);
    }
}
