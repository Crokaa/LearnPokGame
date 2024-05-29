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

    int currentLine = 0;
    Dialog dialog;

    public event Action OnShowDialog;
    public event Action OnCloseDialog;
    bool isTyping;
    Action onDialogFinished;

    public static DialogManager Instance { get; private set; }
    public bool IsShowing { get; set; }


    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator ShowDialogText(string text, bool waitForInput=true) {

        IsShowing = true;
        dialogBox.SetActive(true);

        yield return TypeDialog(text);

        if (waitForInput)
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.X));

        dialogBox.SetActive(false);
        IsShowing = false;
    }

    public IEnumerator ShowDialog(Dialog dialog, Action onFinished=null)
    {

        yield return new WaitForEndOfFrame();

        OnShowDialog?.Invoke();

        IsShowing = true;
        this.dialog = dialog;
        onDialogFinished = onFinished;

        dialogBox.SetActive(true);
        StartCoroutine(TypeDialog(dialog.Lines[0]));

    }

    public IEnumerator TypeDialog(string line)
    {

        isTyping = true;
        dialogText.text = "";
        foreach (var letter in line.ToCharArray())
        {

            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
        isTyping = false;

    }

    public void HandleUpdate()
    {
        if ((Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.X)) && !isTyping)
        {
            ++currentLine;
            if (currentLine < dialog.Lines.Count)
                StartCoroutine(TypeDialog(dialog.Lines[currentLine]));

            else
            {
                IsShowing = false;
                currentLine = 0;
                dialogBox.SetActive(false);
                onDialogFinished?.Invoke();
                OnCloseDialog?.Invoke();
            }
        }
    }
}
