using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI
{
    public sealed class UITableXView : UITableView
    {
         protected override float GetSize(UITableCell cell)
        {
            float size = contentSpacing;
            if (m_GridLayout != null)
            {
                size += m_GridLayout.cellSize.x;
            }
            else
            {
                size += LayoutUtility.GetPreferredWidth(cell.rectTransform);
            }
            return size;
        }

        protected override float GetDimension(Vector2 vector)
        {
            return -vector.x;
        }

        protected override Vector2 GetVector(float value)
        {
            return new Vector2(-value, 0);
        }

        protected override void Awake()
        {
            direction = LoopScrollRectDirection.Horizontal;
            base.Awake();
            horizontal = true;
            vertical = false;
            var layout = content.GetComponent<GridLayoutGroup>();
            if (layout != null && layout.constraint != GridLayoutGroup.Constraint.FixedRowCount)
            {
                Debug.LogError("[LoopHorizontalScrollRect] unsupported GridLayoutGroup constraint");
            }
        }

        protected override bool UpdateItems(Bounds viewBounds, Bounds contentBounds)
        {
            bool changed = false;

            // special case: handling move several page in one frame
            if (viewBounds.max.x < contentBounds.min.x && itemTypeEnd > itemTypeStart)
            {
                float currentSize = contentBounds.size.x;
                float elementSize = (currentSize - contentSpacing * (CurrentLines - 1)) / CurrentLines;
                ReturnToTempPool(false, itemTypeEnd - itemTypeStart);
                itemTypeEnd = itemTypeStart;

                int offsetCount = Mathf.FloorToInt((contentBounds.min.x - viewBounds.max.x) / (elementSize + contentSpacing));
                if (CellTotalCount >= 0 && itemTypeStart - offsetCount * contentConstraintCount < 0)
                {
                    offsetCount = Mathf.FloorToInt((float)(itemTypeStart) / contentConstraintCount);
                }
                itemTypeStart -= offsetCount * contentConstraintCount;
                if (CellTotalCount >= 0)
                {
                    itemTypeStart = Mathf.Max(itemTypeStart, 0);
                }
                itemTypeEnd = itemTypeStart;

                float offset = offsetCount * (elementSize + contentSpacing);
                content.anchoredPosition -= new Vector2(offset + (reverseDirection ? currentSize : 0), 0);
                contentBounds.center -= new Vector3(offset + currentSize / 2, 0, 0);
                contentBounds.size = Vector3.zero;

                changed = true;
            }

            if (viewBounds.min.x > contentBounds.max.x && itemTypeEnd > itemTypeStart)
            {
                int maxItemTypeStart = -1;
                if (CellTotalCount >= 0)
                {
                    maxItemTypeStart = Mathf.Max(0, CellTotalCount - (itemTypeEnd - itemTypeStart));
                    maxItemTypeStart = (maxItemTypeStart / contentConstraintCount) * contentConstraintCount;
                }
                float currentSize = contentBounds.size.x;
                float elementSize = (currentSize - contentSpacing * (CurrentLines - 1)) / CurrentLines;
                ReturnToTempPool(true, itemTypeEnd - itemTypeStart);
                // TODO: fix with contentConstraint?
                itemTypeStart = itemTypeEnd;
            
                int offsetCount = Mathf.FloorToInt((viewBounds.min.x - contentBounds.max.x) / (elementSize + contentSpacing));
                if (maxItemTypeStart >= 0 && itemTypeStart + offsetCount * contentConstraintCount > maxItemTypeStart)
                {
                    offsetCount = Mathf.FloorToInt((float)(maxItemTypeStart - itemTypeStart) / contentConstraintCount);
                }
                itemTypeStart += offsetCount * contentConstraintCount;
                if (CellTotalCount >= 0)
                {
                    itemTypeStart = Mathf.Max(itemTypeStart, 0);
                }
                itemTypeEnd = itemTypeStart;

                float offset = offsetCount * (elementSize + contentSpacing);
                content.anchoredPosition += new Vector2(offset + (reverseDirection ? 0 : currentSize), 0);
                contentBounds.center += new Vector3(offset + currentSize / 2, 0, 0);
                contentBounds.size = Vector3.zero;

                changed = true;
            }

            if (viewBounds.max.x < contentBounds.max.x - threshold - m_ContentRightPadding)
            {
                float size = DeleteItemAtEnd(), totalSize = size;
                while (size > 0 && viewBounds.max.x < contentBounds.max.x - threshold - m_ContentRightPadding - totalSize)
                {
                    size = DeleteItemAtEnd();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }

            if (viewBounds.min.x > contentBounds.min.x + threshold + m_ContentLeftPadding)
            {
                float size = DeleteItemAtStart(), totalSize = size;
                while (size > 0 && viewBounds.min.x > contentBounds.min.x + threshold + m_ContentLeftPadding + totalSize)
                {
                    size = DeleteItemAtStart();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }

            if (viewBounds.max.x > contentBounds.max.x - m_ContentRightPadding)
            {
                float size = NewItemAtEnd(), totalSize = size;
                while (size > 0 && viewBounds.max.x > contentBounds.max.x - m_ContentRightPadding + totalSize)
                {
                    size = NewItemAtEnd();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }

            if (viewBounds.min.x < contentBounds.min.x + m_ContentLeftPadding)
            {
                float size = NewItemAtStart(), totalSize = size;
                while (size > 0 && viewBounds.min.x < contentBounds.min.x + m_ContentLeftPadding - totalSize)
                {
                    size = NewItemAtStart();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }

            if (changed)
            {
                ClearTempPool();
            }

            return changed;
        }
    }
}