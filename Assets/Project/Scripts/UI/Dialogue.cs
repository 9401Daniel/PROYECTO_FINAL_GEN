using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;
using System;

/// <summary>
/// Gestor de diálogos tipo "typewriter" (escribir carácter por carácter).
/// - Un clic mientras se escribe completa la línea (permite leerla).
/// - Un clic posterior la "confirma" y el siguiente avanza a la próxima línea.
/// </summary>
public class Dialogue : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Dialogue")]
    [SerializeField, TextArea(4, 6)] private string[] dialogueLines;
    [SerializeField] private float typingTime = 0.05f;

    [Header("Input")]
    [SerializeField] private InputActionReference interact;

    // Debounce: un mismo clic físico puede generar varios eventos "performed" muy juntos.
    // Nos aseguramos de procesar solo uno por pulsación real.
    [SerializeField, Min(0f)] private float inputDebounce = 0.1f;
    private float lastInputTime = -1f;
    private int lineIndex = 0;
    private bool isTyping = false;

    private bool awaitingAdvance = false;
    private bool dialogueEnded = false;
    private Coroutine typingCoroutine;

    public event Action OnDialogueEnded;

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

    private void OnDialogueInput(InputAction.CallbackContext context)
    {
        // Ignora entradas cuando el diálogo ya terminó o es ruido del mismo clic.
        if (dialogueEnded || IsDebounced())
            return;
        if (isTyping)
        {
            // Clic mientras se escribe: completa la línea (aún requiere confirmar).
            FinishCurrentLine();
        }
        else if (!awaitingAdvance)
        {
            // Línea completa sin confirmar: este clic la confirma para poder leerla.
            awaitingAdvance = true;
        }
        else
        {
            // Línea confirmada: avanza a la siguiente.
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
        isTyping = false;
        awaitingAdvance = false;
        dialogueEnded = false;
        lineIndex = 0;

        dialoguePanel.SetActive(true);
        ShowCurrentLine();
    }

    public void EndDialogue()
    {
        isTyping = false;
        awaitingAdvance = false;
        dialogueEnded = true;

        StopTyping();
        dialoguePanel.SetActive(false);
        dialogueText.text = string.Empty;

        OnDialogueEnded?.Invoke();
    }

    private void ShowCurrentLine()
    {
        // Detiene cualquier escritura en curso antes de iniciar la nueva.
        StopTyping();
        dialogueText.text = string.Empty;
        awaitingAdvance = false;

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
        awaitingAdvance = false;
        typingCoroutine = null;
    }

    private void FinishCurrentLine()
    {
        StopTyping();
        dialogueText.text = dialogueLines[lineIndex];
        isTyping = false;
        awaitingAdvance = false; // completa pero aún sin confirmar: requiere un clic y luego otro para avanzar
    }

    private void NextDialogueLine()
    {
        lineIndex++;
        awaitingAdvance = false;

        if (lineIndex < dialogueLines.Length)
            ShowCurrentLine();
        else
            EndDialogue();
    }

    /// <summary>Detiene cualquier coroutine de escritura activa.</summary>
    private void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }

    /// <summary>
    /// Devuelve true si el evento llega demasiado pronto tras el anterior.
    /// Así un solo clic físico no dispara varias acciones.
    /// </summary>
    private bool IsDebounced()
    {
        float now = Time.unscaledTime;
        if (now - lastInputTime < inputDebounce)
            return true;

        lastInputTime = now;
        return false;
    }
}