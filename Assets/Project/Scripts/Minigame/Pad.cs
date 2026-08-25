using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Pad : MonoBehaviour
{
    private const string SecretCode = "3479";

    [Header("References")]
    [SerializeField] private TextMeshProUGUI displayText;

    [Header("Configuration")]
    [SerializeField] private int maxDigits = 4;

    [Tooltip("If active, the text shows asterisks instead of the digits entered.")]
    [SerializeField] private bool maskInput = true;
    [SerializeField] private UICounter counter;

    [Header("Result")]
    [Tooltip("Dispatched when the correct code is entered.")]
    public UnityEvent onCorrectCode;

    [Tooltip("Dispatched when the wrong code is entered. Receives the code sent.")]
    public UnityEvent onWrongCode;

    private string currentInput = "";


    public string CurrentInput => currentInput;
    public bool IsComplete => currentInput.Length >= maxDigits;
    public int MaxDigits => maxDigits;

    void Start()
    {
        for (int i = 0; i < maxDigits; i++)
        {
            onCorrectCode.AddListener(counter.UpdateRollbackCount);
        }
        onWrongCode.AddListener(Clear);
    }

    /// <summary>Add a digit (0-9). Called from pad buttons.</summary>
    public void PressDigit(int digit)
    {
        // Validate digit range (0-9)
        if (digit < 0 || digit > 9)
        {
            Debug.LogWarning("Pad: se intentó añadir un dígito inválido (" + digit + ").");
            return;
        }

        if (currentInput.Length >= maxDigits)
            return;

        currentInput += digit;
        UpdateDisplay();
    }

    /// <summary>Delete the last digit.</summary>
    public void Backspace()
    {
        if (currentInput.Length == 0)
            return;

        currentInput = currentInput[..^1]; // Remove last digit
        UpdateDisplay();
    }

    /// <summary>Clear the input.</summary>
    public void Clear()
    {
        currentInput = "";
        UpdateDisplay();
    }

    public void Submit()
    {
        if (currentInput.Length == 0)
            return;

        bool correct = string.Equals(currentInput, SecretCode);

        if (correct)
        {
            onCorrectCode?.Invoke();
        }
        else
        {
            currentInput = "ERROR!";
            UpdateDisplay();
            StartCoroutine(OnWrongCode(2f));

        }
    }

    private IEnumerator OnWrongCode(float delay)
    {
        yield return new WaitForSeconds(delay);
        onWrongCode?.Invoke();
    }

    private void UpdateDisplay()
    {
        if (displayText == null)
            return;

        displayText.text = maskInput && currentInput.Length > 0
            ? new string('*', currentInput.Length)
            : currentInput;
    }
    private void OnValidate()
    {
        if (maxDigits < 1)
            maxDigits = 1;
        UpdateDisplay();
    }
}
