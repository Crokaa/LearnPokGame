using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField] LayerMask solidObjects;
    [SerializeField] LayerMask collision;
    [SerializeField] LayerMask grassLayer;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask fovLayer;
    [SerializeField] LayerMask portalLayer;
    [SerializeField] LayerMask triggerLayer;

    public LayerMask SolidObjects
    {
        get { return solidObjects; }
    }

      public LayerMask Collision
    {
        get { return collision; }
    }


    public LayerMask GrassLayer
    {
        get { return grassLayer; }
    }

    public LayerMask InteractableLayer
    {
        get { return interactableLayer; }
    }

    public LayerMask PlayerLayer
    {
        get { return playerLayer; }
    }

    public LayerMask FovLayer
    {
        get { return fovLayer; }
    }

    public LayerMask PortalLayer
    {
        get { return portalLayer; }
    }

    public LayerMask TriggerableLayers {
        get {
            return GrassLayer | FovLayer | PortalLayer | triggerLayer;
        }
    }

    public static GameLayers Instance { get; set; }

    private void Awake () {
        Instance = this;
    }
}
