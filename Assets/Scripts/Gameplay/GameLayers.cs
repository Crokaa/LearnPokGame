using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField] LayerMask solidObjects;
    [SerializeField] LayerMask grassLayer;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask fovLayer;

    public LayerMask SolidObjects
    {
        get { return solidObjects; }
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

    public static GameLayers Instance { get; set; }

    private void Awake () {
        Instance = this;
    }
}
