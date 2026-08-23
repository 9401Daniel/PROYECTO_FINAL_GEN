using UnityEngine;
using UnityEngine.EventSystems;

namespace Minigame
{
    /// <summary>
    /// A single draggable puzzle piece built as a UI element (Canvas / RectTransform).
    /// It snaps to its assigned slot when dropped close enough, and returns to its
    /// starting position when dropped elsewhere.
    ///
    /// Drag is driven by the EventSystem (IPointerDownHandler / IDragHandler /
    /// IPointerUpHandler), so the piece needs a Graphic raycast target (e.g. an
    /// Image) and the Canvas must have a GraphicRaycaster.
    ///
    /// Works with irregular pieces: detection is done by distance to the slot, not
    /// by the sprite shape, so any silhouette can be used.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class PuzzlePiece : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Target slot")]
        [Tooltip("The slot this piece belongs to. If empty, the slot with the same PieceId is found at runtime.")]
        [SerializeField] private PuzzleSlot targetSlot;

        [Tooltip("Optional id used to match a slot automatically when no target slot is assigned.")]
        [SerializeField] private string pieceId;

        [Header("Drag behaviour")]

        [Tooltip("How fast the piece moves back to its start position when released.")]
        [SerializeField] private float returnSpeed = 1200f;

        [Header("Ordering")]
        [Tooltip("If true, the piece is brought to the front of its siblings while dragging.")]
        [SerializeField] private bool bringToFront = true;

        private RectTransform rectTransform;
        private Vector2 startAnchoredPosition;
        private Vector2 startSizeDelta;
        private Vector3 startLocalScale;
        private Quaternion startRotation;
        private Vector2 dragOffset;
        private bool dragging;
        private bool placed;

        public string PieceId => pieceId;
        public bool IsPlaced => placed;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            startAnchoredPosition = rectTransform.anchoredPosition;
            startSizeDelta = rectTransform.sizeDelta;
            startLocalScale = rectTransform.localScale;
            startRotation = rectTransform.rotation;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (placed || eventData.button != PointerEventData.InputButton.Left)
                return;

            dragging = true;

            if (bringToFront)
                rectTransform.SetAsLastSibling();

            // Compute the offset between the pointer and the piece's current position,
            // expressed in the coordinate space of the piece's parent.
            RectTransform parent = rectTransform.parent as RectTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 pointerLocal);

            dragOffset = rectTransform.anchoredPosition - pointerLocal;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!dragging || placed)
                return;

            RectTransform parent = rectTransform.parent as RectTransform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parent,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 pointerLocal))
            {
                rectTransform.anchoredPosition = pointerLocal + dragOffset;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!dragging || placed)
                return;

            dragging = false;
            TryDrop();
        }

        private void Update()
        {
            if (placed || dragging)
                return;

            if (rectTransform.anchoredPosition != startAnchoredPosition)
                ReturnToStart();
        }

        private void TryDrop()
        {
            PuzzleSlot slot = ResolveTargetSlot();
            if (slot != null && SlotIsWithinRange(slot))
            {
                slot.TryPlacePiece(this, out placed);
            }
            // Keep the current rotation; helps non-square pieces fit their slot.
        }

        private bool SlotIsWithinRange(PuzzleSlot slot)
        {
            RectTransform slotRect = slot.RectTransform;
            if (slotRect == null)
                return false;

            float allowedDistance = slot.SnapDistance;
            float distance = Vector2.Distance(rectTransform.anchoredPosition, slotRect.anchoredPosition);
            return distance <= allowedDistance;
        }

        private void ReturnToStart()
        {
            rectTransform.anchoredPosition = Vector2.MoveTowards(
                rectTransform.anchoredPosition,
                startAnchoredPosition,
                returnSpeed * Time.deltaTime);

            rectTransform.rotation = Quaternion.RotateTowards(
                rectTransform.rotation,
                startRotation,
                returnSpeed * 0.1f * Time.deltaTime);

            if (Vector2.Distance(rectTransform.anchoredPosition, startAnchoredPosition) < 0.5f)
            {
                rectTransform.anchoredPosition = startAnchoredPosition;
                rectTransform.rotation = startRotation;
                rectTransform.sizeDelta = startSizeDelta;
                rectTransform.localScale = startLocalScale;
            }
        }

        private PuzzleSlot ResolveTargetSlot()
        {
            if (targetSlot != null && targetSlot.PieceId == pieceId)
                return targetSlot;

            if (!string.IsNullOrEmpty(pieceId))
            {
                foreach (PuzzleSlot slot in FindObjectsByType<PuzzleSlot>(FindObjectsSortMode.None))
                {
                    if (string.Equals(slot.PieceId, pieceId))
                        return slot;
                }
            }
            return null;
        }

        public void RestoreStartTransform()
        {
            dragging = false;
            placed = false;
            rectTransform.anchoredPosition = startAnchoredPosition;
            rectTransform.sizeDelta = startSizeDelta;
            rectTransform.localScale = startLocalScale;
            rectTransform.rotation = startRotation;
        }
    }
}
