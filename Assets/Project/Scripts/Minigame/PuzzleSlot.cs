using UnityEngine;
using UnityEngine.Events;

namespace Minigame
{
    /// <summary>
    /// A target location for one puzzle piece, built as a UI element (RectTransform).
    /// When the matching piece is dropped close enough, the slot snaps it in place
    /// and fires events.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class PuzzleSlot : MonoBehaviour
    {
        [Header("Matching")]
        [Tooltip("Must match the PieceId of the piece that belongs here.")]
        [SerializeField] private string pieceId;

        [Tooltip("Extra tolerance to the snap distance, added on top of the piece's own snapDistance.")]
        [SerializeField] private float tolerance = 0f;

        [Header("Events")]
        [Tooltip("Fired once when a piece is placed in this slot. Receives the placed piece.")]
        public UnityEvent<PuzzlePiece> onPiecePlaced;

        private RectTransform rectTransform;

        public string PieceId => pieceId;
        public RectTransform RectTransform => rectTransform;
        public PuzzlePiece PlacedPiece { get; private set; }

        public bool IsOccupied => PlacedPiece != null;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void TryPlacePiece(PuzzlePiece piece, out bool success)
        {
            success = false;

            if (piece == null || piece.PieceId != pieceId || IsOccupied)
                return;

            // Snap the piece to the slot's exact position and rotation.
            RectTransform pieceRect = piece.GetComponent<RectTransform>();
            pieceRect.anchoredPosition = rectTransform.anchoredPosition;
            pieceRect.rotation = rectTransform.rotation;

            PlacedPiece = piece;
            onPiecePlaced?.Invoke(piece);
            success = true;
        }

        public float SnapDistance => tolerance;
    }
}
