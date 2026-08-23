using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

/// <summary>
/// Punto de entrada único para reproducir diálogos en el juego.
/// Persiste entre escenas (Singleton + DontDestroyOnLoad). Cada escena
/// coloca su propia copia de "Dialog Manager" con los diálogos ya
/// asignados; al cargar una nueva escena, la copia duplicada se destruye
/// y la instancia original (con sus referencias) se conserva.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    private PlayerMovement playerMovement;
    private BombThrower bombThrower;
    private PlayerUI playerUI;

    [Serializable]
    public class SceneDialogueEntry
    {
        [Tooltip("Debe coincidir EXACTAMENTE con el nombre de la escena (SceneManager.GetActiveScene().name).")]
        public string sceneName;

        [Tooltip("Diálogo a mostrar al llamar ShowDialogue en esta escena.")]
        public Dialogue dialogue;
    }

    [Header("Mapeo Escena -> Diálogos")]
    [SerializeField] private List<SceneDialogueEntry> sceneDialogues = new List<SceneDialogueEntry>();
    private Dialogue activeDialogue;

    [SerializeField] private GameObject dialoguePanel;
    public event Action OnDialogueManagerEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        dialoguePanel.SetActive(false);
    }

    /// <summary>
    /// Muestra el diálogo correspondiente a la escena activa y al momento indicado.
    /// </summary>
    public void ShowDialogue()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        SceneDialogueEntry entry = sceneDialogues.Find(e => e.sceneName == sceneName);
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        bombThrower = FindFirstObjectByType<BombThrower>();
        playerUI = FindFirstObjectByType<PlayerUI>();

        if (entry == null)
        {
            Debug.LogWarning($"[DialogueManager] No hay diálogos configurados para la escena '{sceneName}'.");
            return;
        }

        if (entry.dialogue == null)
        {
            Debug.LogWarning($"[DialogueManager] La escena '{sceneName}' no tiene un diálogo de tipo '{typeof(Dialogue).Name}' asignado.");
            return;
        }

        PlayDialogue(entry.dialogue);
    }

    private void PlayDialogue(Dialogue dialogue)
    {
        // Evita que dos diálogos queden activos escuchando input al mismo tiempo.
        if (activeDialogue != null && activeDialogue != dialogue)
        {
            activeDialogue.OnDialogueEnded -= HandleDialogueEnded;
            activeDialogue.gameObject.SetActive(false);
        }

        dialoguePanel.SetActive(true);
        activeDialogue = dialogue;
        activeDialogue.OnDialogueEnded += HandleDialogueEnded;
        if (playerUI != null)
            playerUI.SetActive(false);
        if (playerMovement != null)
            playerMovement.IsMoving = false;
        if (bombThrower != null)
            bombThrower.Active = false;
        activeDialogue.gameObject.SetActive(true);
        activeDialogue.StartDialogue();
    }

    private void HandleDialogueEnded()
    {
        OnDialogueManagerEnded?.Invoke();
        if (activeDialogue == null)
            return;
        activeDialogue.OnDialogueEnded -= HandleDialogueEnded;
        activeDialogue.gameObject.SetActive(false);
        if (playerUI != null)
            playerUI.SetActive(true);
        if (playerMovement != null)
            playerMovement.IsMoving = true;
        if (bombThrower != null)
            bombThrower.Active = true;
        activeDialogue = null;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        dialoguePanel.SetActive(false);

    }

}
