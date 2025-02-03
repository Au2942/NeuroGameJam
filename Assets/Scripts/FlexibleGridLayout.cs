//Modified from tutorial by GameDevGuide: https://www.youtube.com/watch?v=CGsEJToeXmA
using UnityEngine;
using UnityEngine.UI;

public class FlexibleGridLayout : LayoutGroup
{

    public enum FitType
    {
        Uniform,
        Width,
        Height,
        FixedRows,
        FixedColumns
    }
    public FitType fitType;
    [Min(1)] public int rows = 1;
    [Min(1)] public int columns = 1;
    public Vector2 preferredCellSize;
    public Vector2 cellSize;
    public Vector2 spacing;
    public bool fitX;
    public bool fitY;
    public bool fitCellInParent = true;
    public bool keepCellAspectRatio = false;


    public override void CalculateLayoutInputVertical()
    {
        base.CalculateLayoutInputHorizontal();

        if(rectChildren.Count == 0)
        {
            return;
        }

        if (fitType == FitType.Width || fitType == FitType.Height || fitType == FitType.Uniform)
        {
            fitX = true;
            fitY = true;

            float sqrRt = Mathf.Sqrt(rectChildren.Count);
            rows = Mathf.CeilToInt(sqrRt);
            columns = Mathf.CeilToInt(sqrRt);
        }

        if (fitType == FitType.Width || fitType == FitType.FixedColumns)
        {
            rows = Mathf.CeilToInt(rectChildren.Count / (float)columns);
        }

        if (fitType == FitType.Height || fitType == FitType.FixedRows)
        {
            columns = Mathf.CeilToInt(rectChildren.Count / (float)rows);
        }
        
        float parentWidth = rectTransform.rect.width;
        float parentHeight = rectTransform.rect.height;

        float cellWidth = preferredCellSize.x;
        float cellHeight = preferredCellSize.y;


        if (fitX || fitY)
        {
            cellWidth = (parentWidth - padding.left - padding.right + spacing.x * (columns-1)) / columns;
            cellHeight = (parentHeight - padding.top - padding.bottom + spacing.y * (rows-1)) / rows;
        }
        else if (fitCellInParent)
        {
            if(parentWidth < preferredCellSize.x*columns + spacing.x*(columns-1) + padding.left + padding.right)
            {
                cellWidth = (parentWidth - padding.left - padding.right - spacing.x*(columns-1)) / columns;
            }
            else
            {
                cellWidth = preferredCellSize.x;
            }
            if(parentHeight < preferredCellSize.y*rows + spacing.y*(rows-1) + padding.top + padding.bottom)
            {
                cellHeight = (parentHeight - padding.top - padding.bottom - spacing.y*(rows-1)) / rows;
            }
            else
            {
                cellHeight = preferredCellSize.y;
            }
        }

        float aspectRatio = preferredCellSize.x / preferredCellSize.y;
        if (keepCellAspectRatio)
        {
            if(cellWidth / cellHeight > aspectRatio)
            {
                cellWidth = cellHeight * aspectRatio;
            }
            else
            {
                cellHeight = cellWidth / aspectRatio;
            }
        }

        cellSize.x = cellWidth;
        cellSize.y = cellHeight;


        int columnCount;
        int rowCount;

        float xNoPadding = cellSize.x * columns + spacing.x * (columns - 1);
        float yNoPadding = cellSize.y * rows + spacing.y * (rows - 1);
        float startOffsetX = GetStartOffset(0, xNoPadding);
        float startOffsetY = GetStartOffset(1, yNoPadding);

        //TODO - fix problem where alignment causes one side of padding to be ignored

        for(int i = 0; i < rectChildren.Count; i++)
        {
            rowCount = i / columns;
            columnCount = i % columns;

            var item = rectChildren[i];

            float xPos;
            float yPos;

            xPos = startOffsetX + (cellSize.x * columnCount) + (spacing.x * columnCount);
            yPos = startOffsetY + (cellSize.y * rowCount) + (spacing.y * rowCount);
            
            SetChildAlongAxis(item, 0, xPos, cellSize.x);
            SetChildAlongAxis(item, 1, yPos, cellSize.y);
        }
    }

    public override void SetLayoutHorizontal()
    {
    }

    public override void SetLayoutVertical()
    {
    }
}