using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{

    [SerializeField] GameObject dialogBox;
    [SerializeField] Text dialogText;
    [SerializeField] int lettersPerSecond;

    public event Action OnShowDialog;
    public event Action OnCloseDialog;

    public static DialogManager Instance { get; private set; }
    public bool IsShowing { get; set; }


    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator ShowDialogText(string text, bool waitForInput = true, bool autoClose = true)
    {

        OnShowDialog?.Invoke();
        IsShowing = true;
        dialogBox.SetActive(true);

        yield return TypeDialog(text);

        if (waitForInput)
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.X));

        if (autoClose)
        {

            dialogBox.SetActive(false);
            IsShowing = false;
            OnCloseDialog?.Invoke();
        }
    }

    public IEnumerator ShowDialog(Dialog dialog)
    {

        yield return new WaitForEndOfFrame();

        OnShowDialog?.Invoke();
        IsShowing = true;
        dialogBox.SetActive(true);

        foreach (var line in dialog.Lines)
        {

            yield return TypeDialog(line);
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.X));
        }

        dialogBox.SetActive(false);
        IsShowing = false;
        OnCloseDialog?.Invoke();
    }

    public IEnumerator TypeDialog(string line)
    {

        dialogText.text = "";
        foreach (var letter in line.ToCharArray())
        {

            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }

    }

    public void HandleUpdate()
    {

    }
}
