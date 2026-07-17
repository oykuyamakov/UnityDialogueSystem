using UnityEngine;
using UnityEngine.UI;

namespace Cadi.Scripts.UI.Extensions
{
    [AddComponentMenu("Layout/Better Grid Layout Group", 152)]
    public class BetterGridLayoutGroup : LayoutGroup
    {
        public enum Corner
        {
            UpperLeft = 0,
            UpperRight = 1,
            LowerLeft = 2,
            LowerRight = 3
        }

        public enum Axis
        {
            Horizontal = 0,
            Vertical = 1
        }

        public enum Constraint
        {
            Flexible = 0,
            FixedColumnCount = 1,
            FixedRowCount = 2
        }

        public enum IncompleteRowAlignment
        {
            Default,
            Center,
            End
        }

        [SerializeField]
        private Corner m_StartCorner = Corner.UpperLeft;

        public Corner StartCorner
        {
            get => m_StartCorner;
            set => SetProperty(ref m_StartCorner, value);
        }

        [SerializeField]
        private Axis m_StartAxis = Axis.Horizontal;

        public Axis StartAxis
        {
            get => m_StartAxis;
            set => SetProperty(ref m_StartAxis, value);
        }

        [SerializeField]
        private Vector2 m_CellSize = new Vector2(100, 100);

        public Vector2 CellSize
        {
            get => m_CellSize;
            set => SetProperty(ref m_CellSize, value);
        }

        [SerializeField]
        private Vector2 m_Spacing = Vector2.zero;

        public Vector2 Spacing
        {
            get => m_Spacing;
            set => SetProperty(ref m_Spacing, value);
        }

        [SerializeField]
        private Constraint m_GridConstraint = Constraint.Flexible;

        public Constraint GridConstraint
        {
            get => m_GridConstraint;
            set => SetProperty(ref m_GridConstraint, value);
        }

        [SerializeField]
        [Min(1)]
        private int m_ConstraintCount = 2;

        public int ConstraintCount
        {
            get => m_ConstraintCount;
            set => SetProperty(ref m_ConstraintCount, Mathf.Max(1, value));
        }

        [SerializeField]
        private IncompleteRowAlignment m_IncompleteRowAlignment = IncompleteRowAlignment.Default;

        public IncompleteRowAlignment IncompleteRowAlign
        {
            get => m_IncompleteRowAlignment;
            set => SetProperty(ref m_IncompleteRowAlignment, value);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            m_ConstraintCount = Mathf.Max(1, m_ConstraintCount);
        }
#endif

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            int childCount = rectChildren.Count;

            if (childCount == 0)
            {
                SetLayoutInputForAxis(padding.horizontal, padding.horizontal, -1, 0);
                return;
            }

            int minColumns;
            int preferredColumns;

            if (m_GridConstraint == Constraint.FixedColumnCount)
            {
                minColumns = preferredColumns = m_ConstraintCount;
            }
            else if (m_GridConstraint == Constraint.FixedRowCount)
            {
                minColumns = preferredColumns = CeilDivide(childCount, m_ConstraintCount);
            }
            else
            {
                minColumns = 1;
                preferredColumns = Mathf.CeilToInt(Mathf.Sqrt(childCount));
            }

            float minWidth = CalculateRequiredSpace(
                padding.horizontal,
                CellSize.x,
                Spacing.x,
                minColumns
            );

            float preferredWidth = CalculateRequiredSpace(
                padding.horizontal,
                CellSize.x,
                Spacing.x,
                preferredColumns
            );

            SetLayoutInputForAxis(minWidth, preferredWidth, -1, 0);
        }

        public override void CalculateLayoutInputVertical()
        {
            int childCount = rectChildren.Count;

            if (childCount == 0)
            {
                SetLayoutInputForAxis(padding.vertical, padding.vertical, -1, 1);
                return;
            }

            int minRows;

            if (m_GridConstraint == Constraint.FixedColumnCount)
            {
                minRows = CeilDivide(childCount, m_ConstraintCount);
            }
            else if (m_GridConstraint == Constraint.FixedRowCount)
            {
                minRows = m_ConstraintCount;
            }
            else
            {
                float width = rectTransform.rect.width;
                int cellCountX = CalculateFlexibleCellCount(
                    width,
                    padding.horizontal,
                    CellSize.x,
                    Spacing.x
                );

                minRows = CeilDivide(childCount, cellCountX);
            }

            float minHeight = CalculateRequiredSpace(
                padding.vertical,
                CellSize.y,
                Spacing.y,
                minRows
            );

            SetLayoutInputForAxis(minHeight, minHeight, -1, 1);
        }

        public override void SetLayoutHorizontal()
        {
            SetCellsAlongAxis(0);
        }

        public override void SetLayoutVertical()
        {
            SetCellsAlongAxis(1);
        }

        private void SetCellsAlongAxis(int axis)
        {
            int childCount = rectChildren.Count;

            if (axis == 0)
            {
                for (int i = 0; i < childCount; i++)
                {
                    RectTransform rect = rectChildren[i];

                    m_Tracker.Add(
                        this,
                        rect,
                        DrivenTransformProperties.Anchors |
                        DrivenTransformProperties.AnchoredPosition |
                        DrivenTransformProperties.SizeDelta
                    );

                    rect.anchorMin = Vector2.up;
                    rect.anchorMax = Vector2.up;
                    rect.sizeDelta = CellSize;
                }

                return;
            }

            if (childCount == 0)
            {
                return;
            }

            float width = rectTransform.rect.size.x;
            float height = rectTransform.rect.size.y;

            int cellCountX = 1;
            int cellCountY = 1;

            if (m_GridConstraint == Constraint.FixedColumnCount)
            {
                cellCountX = m_ConstraintCount;
                cellCountY = CeilDivide(childCount, cellCountX);
            }
            else if (m_GridConstraint == Constraint.FixedRowCount)
            {
                cellCountY = m_ConstraintCount;
                cellCountX = CeilDivide(childCount, cellCountY);
            }
            else
            {
                cellCountX = CalculateFlexibleCellCount(
                    width,
                    padding.horizontal,
                    CellSize.x,
                    Spacing.x
                );

                cellCountY = CalculateFlexibleCellCount(
                    height,
                    padding.vertical,
                    CellSize.y,
                    Spacing.y
                );
            }

            int cornerX = (int)m_StartCorner % 2;
            int cornerY = (int)m_StartCorner / 2;

            int cellsPerMainAxis;
            int actualCellCountX;
            int actualCellCountY;

            if (m_StartAxis == Axis.Horizontal)
            {
                cellsPerMainAxis = Mathf.Max(1, cellCountX);
                actualCellCountX = Mathf.Clamp(cellCountX, 1, childCount);

                if (m_GridConstraint == Constraint.FixedRowCount)
                {
                    actualCellCountY = Mathf.Min(cellCountY, childCount);
                }
                else
                {
                    actualCellCountY = Mathf.Clamp(
                        cellCountY,
                        1,
                        CeilDivide(childCount, cellsPerMainAxis)
                    );
                }
            }
            else
            {
                cellsPerMainAxis = Mathf.Max(1, cellCountY);
                actualCellCountY = Mathf.Clamp(cellCountY, 1, childCount);

                if (m_GridConstraint == Constraint.FixedColumnCount)
                {
                    actualCellCountX = Mathf.Min(cellCountX, childCount);
                }
                else
                {
                    actualCellCountX = Mathf.Clamp(
                        cellCountX,
                        1,
                        CeilDivide(childCount, cellsPerMainAxis)
                    );
                }
            }

            Vector2 requiredSpace = new Vector2(
                actualCellCountX * CellSize.x + Mathf.Max(0, actualCellCountX - 1) * Spacing.x,
                actualCellCountY * CellSize.y + Mathf.Max(0, actualCellCountY - 1) * Spacing.y
            );

            Vector2 startOffset = new Vector2(
                GetStartOffset(0, requiredSpace.x),
                GetStartOffset(1, requiredSpace.y)
            );

            int lastLogicalRowIndex = -1;
            int itemsInLastRow = 0;

            if (m_StartAxis == Axis.Horizontal)
            {
                int rowCount = CeilDivide(childCount, cellsPerMainAxis);
                lastLogicalRowIndex = rowCount - 1;

                itemsInLastRow = childCount - lastLogicalRowIndex * cellsPerMainAxis;

                if (itemsInLastRow <= 0)
                {
                    itemsInLastRow = cellsPerMainAxis;
                }
            }

            for (int i = 0; i < childCount; i++)
            {
                int logicalX;
                int logicalY;

                if (m_StartAxis == Axis.Horizontal)
                {
                    logicalX = i % cellsPerMainAxis;
                    logicalY = i / cellsPerMainAxis;
                }
                else
                {
                    logicalX = i / cellsPerMainAxis;
                    logicalY = i % cellsPerMainAxis;
                }

                int positionX = logicalX;
                int positionY = logicalY;

                if (cornerX == 1)
                {
                    positionX = actualCellCountX - 1 - positionX;
                }

                if (cornerY == 1)
                {
                    positionY = actualCellCountY - 1 - positionY;
                }

                float extraX = GetIncompleteRowExtraOffsetX(
                    logicalY,
                    lastLogicalRowIndex,
                    itemsInLastRow,
                    cellsPerMainAxis,
                    cornerX
                );

                SetChildAlongAxis(
                    rectChildren[i],
                    0,
                    startOffset.x + extraX + (CellSize.x + Spacing.x) * positionX,
                    CellSize.x
                );

                SetChildAlongAxis(
                    rectChildren[i],
                    1,
                    startOffset.y + (CellSize.y + Spacing.y) * positionY,
                    CellSize.y
                );
            }
        }

        private float GetIncompleteRowExtraOffsetX(
            int logicalY,
            int lastLogicalRowIndex,
            int itemsInLastRow,
            int cellsPerMainAxis,
            int cornerX
        )
        {
            if (m_StartAxis != Axis.Horizontal)
            {
                return 0f;
            }

            if (m_IncompleteRowAlignment == IncompleteRowAlignment.Default)
            {
                return 0f;
            }

            if (logicalY != lastLogicalRowIndex)
            {
                return 0f;
            }

            if (itemsInLastRow >= cellsPerMainAxis)
            {
                return 0f;
            }

            int emptySlots = cellsPerMainAxis - itemsInLastRow;
            float stepX = CellSize.x + Spacing.x;

            float offset = 0f;

            if (m_IncompleteRowAlignment == IncompleteRowAlignment.Center)
            {
                offset = emptySlots * 0.5f * stepX;
            }
            else if (m_IncompleteRowAlignment == IncompleteRowAlignment.End)
            {
                offset = emptySlots * stepX;
            }

            if (cornerX == 1)
            {
                offset = -offset;
            }

            return offset;
        }

        private static int CalculateFlexibleCellCount(
            float availableSize,
            float paddingSize,
            float cellSize,
            float spacing
        )
        {
            float step = cellSize + spacing;

            if (step <= 0f)
            {
                return int.MaxValue;
            }

            return Mathf.Max(
                1,
                Mathf.FloorToInt((availableSize - paddingSize + spacing + 0.001f) / step)
            );
        }

        private static float CalculateRequiredSpace(
            float paddingSize,
            float cellSize,
            float spacing,
            int count
        )
        {
            if (count <= 0)
            {
                return paddingSize;
            }

            return paddingSize + (cellSize + spacing) * count - spacing;
        }

        private static int CeilDivide(int value, int divisor)
        {
            divisor = Mathf.Max(1, divisor);
            return Mathf.CeilToInt(value / (float)divisor);
        }
    }
}