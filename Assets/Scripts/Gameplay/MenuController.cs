using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{

    [SerializeField] GameObject menu;
    List<Text> menuItems;
    int currentSelected;
    public Action<int> OnSelected;
    public Action GoBack;

    void Start()
    {
        currentSelected = 0;
        menuItems = menu.GetComponentsInChildren<Text>().ToList();
    }

    public void OpenMenu()
    {
        menu.SetActive(true);
        UpdateMenuSelection();
    }

    public void CloseMenu()
    {
        menu.SetActive(false);
    }

    public void HandleUpdate()
    {

        int prevSelected = currentSelected;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            currentSelected = currentSelected == menuItems.Count - 1 ? 0 : currentSelected + 1;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentSelected = currentSelected == 0 ? menuItems.Count - 1 : currentSelected - 1;
        else if (Input.GetKeyDown(KeyCode.X) || (Input.GetKeyDown(KeyCode.Z) && currentSelected == menuItems.Count - 1))
            GoBack.Invoke();
        else if(Input.GetKeyDown(KeyCode.Z))
            OnSelected.Invoke(currentSelected);


        if (prevSelected != currentSelected)
            UpdateMenuSelection();
    }

    void UpdateMenuSelection()
    {

        for (int i = 0; i < menuItems.Count; i++)
        {

            if (i == currentSelected)
                menuItems[i].color = GlobalSettings.Instance.HighlightedColor;

            else
                menuItems[i].color = Color.black;
        }

    }
}
