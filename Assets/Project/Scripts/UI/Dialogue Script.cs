using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class DialogueScript : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Dialogue")]
    [SerializeField, TextArea(4, 6)] private string[] dialogueLines;
    [SerializeField] private float typingTime = 0.05f;

    [Header("Input")]
    [SerializeField] private InputActionReference interact;

    private int lineIndex = 0;

    private bool didDialogueStart = false;
    private bool isTyping = false;

    private Coroutine typingCoroutine;

    private void OnEnable()
    {
        if (interact != null)
        {
            interact.action.Enable();
            interact.action.performed += OnDialogueInput;
        }
    }

    private void OnDisable()
    {
        if (interact != null)
        {
            interact.action.performed -= OnDialogueInput;
            interact.action.Disable();
        }
    }

    private void Start()
    {
        StartDialogue();
    }

    private void OnDialogueInput(InputAction.CallbackContext context)
    {
        if (!didDialogueStart)
        {
            StartDialogue();
        }
        else if (isTyping)
        {
            FinishCurrentLine();
        }
        else
        {
            NextDialogueLine();
        }
    }

    public void StartDialogue()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning("No Dialogue Lines Set");
            return;
        }

        didDialogueStart = true;
        lineIndex = 0;

        dialoguePanel.SetActive(true);

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(ShowLine());
    }

    private IEnumerator ShowLine()
    {
        isTyping = true;
        dialogueText.text = string.Empty;

        foreach (char ch in dialogueLines[lineIndex])
        {
            dialogueText.text += ch;

            yield return new WaitForSeconds(typingTime);
        }

        isTyping = false;
        typingCoroutine = null;
    }

    private void FinishCurrentLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueText.text = dialogueLines[lineIndex];
        isTyping = false;
    }

    private void NextDialogueLine()
    {
        lineIndex++;

        if (lineIndex < dialogueLines.Length)
        {
            ShowCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        didDialogueStart = false;
        isTyping = false;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialoguePanel.SetActive(false);
    }
}