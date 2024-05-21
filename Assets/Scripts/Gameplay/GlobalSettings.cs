using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
    [SerializeField] Color highlightedColor;

    public Color HighlightedColor { get { return highlightedColor; } }

    public static GlobalSettings Instance { get; private set; }

    void Awake () {

        Instance = this;
    }
}
