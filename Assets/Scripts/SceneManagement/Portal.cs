using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using Unity.Collections;

public class Portal : MonoBehaviour, IPlayerTriggerable
{

    [SerializeField] int sceneToLoad = -1;
    [SerializeField] Transform spawnPoint;
    [SerializeField] DestinationIdentifier destinationPortal;

    PlayerController player;

    public Transform SpawnPoint
    {
        get { return spawnPoint; }
    }
    public void OnPlayerTriggered(PlayerController player)
    {
        StartCoroutine(SwitchScene());
        this.player = player;
    }

    IEnumerator SwitchScene()
    {

        DontDestroyOnLoad(gameObject);

        GameController.Instance.PauseGame(true);

        yield return SceneManager.LoadSceneAsync(sceneToLoad);

        var destPortal = FindObjectsOfType<Portal>().First(x => x != this && x.destinationPortal == destinationPortal);
        player.Character.SetPositionAndSnapToTile(destPortal.spawnPoint.position);

        GameController.Instance.PauseGame(false);

        Destroy(gameObject);
    }

    public enum DestinationIdentifier {A, B, C, D, E}
    
}
