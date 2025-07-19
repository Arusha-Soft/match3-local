using UnityEngine;

namespace Project.Core
{

    public class GridGenerator : MonoBehaviour
    {
        [SerializeField] private Transform m_GridBackground;
        [SerializeField] private Transform m_BlocksParent;
        [SerializeField] private float m_DefaultTileSize = 1f;
        
        [SerializeField] private GridData m_DefaultGridData;

        public GridData GridData => m_DefaultGridData;
        public BaseGrid[,] Grids { private set; get; }

        [ContextMenu("Init")]
        public void Init()
        {
            GenerateGrid(m_DefaultGridData);
        }

        public void GenerateGrid(GridData gridData)
        {
            Grids = new BaseGrid[gridData.X, gridData.Y];

            float xScale = gridData.X * m_DefaultTileSize + (gridData.XSpacing * m_DefaultTileSize * (gridData.X - 1));
            float yScale = gridData.Y * m_DefaultTileSize + (gridData.YSpacing * m_DefaultTileSize * (gridData.Y - 1));
            Vector3 newScale = new Vector3(xScale, yScale, 1);

            m_GridBackground.gameObject.SetActive(true);
            m_GridBackground.transform.localScale = newScale;

            int gridCount = gridData.X * gridData.Y;
            float xShift = (newScale.x / -2f) + m_DefaultTileSize / 2f;
            float yShift = (newScale.y / -2f) + m_DefaultTileSize / 2f;

            int xCounter = 0;
            int yCounter = 0;

            float xPosition;
            float yPosition;
            Vector3 worldPosition = Vector3.one;

            for (int i = 0; i < gridCount; i++)
            {
                float spacingX = (xCounter > 0 ? gridData.XSpacing : 0) * xCounter;
                float spacingY = (yCounter > 0 ? gridData.YSpacing : 0) * yCounter;
                xPosition = xShift + (m_DefaultTileSize * xCounter) + spacingX;
                yPosition = yShift + (m_DefaultTileSize * yCounter) + spacingY;
                worldPosition.x = xPosition;
                worldPosition.y = yPosition;
                worldPosition += m_GridBackground.position;

                BaseGrid grid = Instantiate(gridData.GridPrefab, m_BlocksParent);
                grid.name = $"{grid.name}_X:{xCounter}_Y:{yCounter}";
                grid.transform.position = worldPosition;
                Grids[xCounter,yCounter] = grid;

                if (xCounter > 0 && (i + 1) % gridData.X == 0)
                {
                    xCounter = 0;
                    yCounter++;
                }
                else
                {
                    xCounter++;
                }
            }

            newScale.x += gridData.XPadding;
            newScale.y += gridData.YPadding;
            m_GridBackground.localScale = newScale;
        }
    }

    [System.Serializable]
    public struct GridData
    {
        public BaseGrid GridPrefab;
        public int X;
        public int Y;
        public float XSpacing;
        public float YSpacing;
        public float XPadding;
        public float YPadding;
    }
}