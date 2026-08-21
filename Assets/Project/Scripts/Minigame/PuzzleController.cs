using UnityEngine;
using UnityEngine.Events;

namespace Minigame
{
    /// <summary>
    /// Tracks all puzzle slots and reports when the whole puzzle is solved.
    /// Add this to a parent object that holds the slots (or anywhere in the scene).
    /// </summary>
    public class PuzzleController : MonoBehaviour
    {
        [Header("Slots")]
        [Tooltip("All the slots that make up this puzzle. Found automatically if empty.")]
        [SerializeField] private PuzzleSlot[] slots;

        [Header("Events")]
        [Tooltip("Fired when every slot has a correctly placed piece.")]
        public UnityEvent onPuzzleSolved;

        [Tooltip("Fired every time a piece is placed, with the count of solved slots.")]
        public UnityEvent<int> onProgressChanged;

        public int TotalPieces { get; private set; }
        public int PlacedCount { get; private set; }
        public bool IsSolved { get; private set; }

        private void Awake()
        {
            if (slots == null || slots.Length == 0)
                slots = GetComponentsInChildren<PuzzleSlot>(true);

            if (slots == null)
                slots = new PuzzleSlot[0];

            TotalPieces = slots.Length;
        }

        private void OnEnable()
        {
            foreach (PuzzleSlot slot in slots)
            {
                if (slot != null)
                    slot.onPiecePlaced.AddListener(HandlePiecePlaced);
            }
        }

        private void OnDisable()
        {
            foreach (PuzzleSlot slot in slots)
            {
                if (slot != null)
                    slot.onPiecePlaced.RemoveListener(HandlePiecePlaced);
            }
        }

        private void HandlePiecePlaced(PuzzlePiece piece)
        {
            PlacedCount++;
            onProgressChanged?.Invoke(PlacedCount);

            if (!IsSolved && PlacedCount >= TotalPieces)
            {
                IsSolved = true;
                onPuzzleSolved?.Invoke();
            }
        }
    }
}
