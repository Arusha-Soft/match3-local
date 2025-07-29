using System.Collections.Generic;
using UnityEngine;

namespace Project.Core
{
    public class BoardData : MonoBehaviour
    {
        [SerializeField] private Vector2Int m_OriginalBoardSize;
        [SerializeField] private Vector2Int m_VisibleBoardSize;
        [SerializeField] private int m_HitBuffer = 10;
        [SerializeField] private List<Block> m_Blocks;

        public IReadOnlyList<Block> VisibleBlocks => m_VisibleBlocks;
        public Vector2Int OriginalBoardSize => m_OriginalBoardSize;
        public Vector2Int VisibleBoardSize => m_VisibleBoardSize;

        private int m_BlockCount;

        private BoardIdentity m_BoardIdentity;

        private List<Block> m_VisibleBlocks = new List<Block>();
        private Dictionary<int, Block> m_BlockDictionary;

        private List<RaycastHit2D> m_VerticalHitBuffer;
        private List<RaycastHit2D> m_HorizontalHitBuffer;

        public void Init(BoardIdentity boardIdentity)
        {
            m_BlockDictionary = new Dictionary<int, Block>();
            m_VerticalHitBuffer = new List<RaycastHit2D>(m_HitBuffer);
            m_HorizontalHitBuffer = new List<RaycastHit2D>(m_HitBuffer);

            m_BoardIdentity = boardIdentity;

            for (int i = 0; i < m_Blocks.Count; i++)
            {
                Block block = m_Blocks[i];
                m_BlockDictionary.Add(block.Id, block);

                if (block.IsVisible)
                {
                    m_VisibleBlocks.Add(block);
                }
            }

            m_BlockCount = m_Blocks.Count;
        }

        public Block GetBlockById(int id)
        {
            return m_BlockDictionary[id];
        }

        public bool TryGetUpBlockIdOf(int id, out int blockId)
        {
            blockId = id - m_OriginalBoardSize.y;
            bool result = blockId >= 0;
            return result;
        }

        public bool TryGetDownBlockIdOf(int id, out int blockId)
        {
            blockId = id + m_OriginalBoardSize.y;
            bool result = blockId <= m_Blocks[m_Blocks.Count - 1].Id;
            return result;
        }

        public bool TryGetRightIdOf(int id, out int blockId)
        {
            blockId = id + 1;
            bool result = blockId <= m_Blocks[m_Blocks.Count - 1].Id;
            return result;
        }

        public bool TryGetLeftIdOf(int id, out int blockId)
        {
            blockId = id - 1;
            bool result = blockId >= 0;
            return result;
        }

        public bool TryGetBlockAt(int rowNumber, int columnNumber, out Block resultBlock)
        {
            int blockId = (rowNumber * m_OriginalBoardSize.y) + columnNumber;
            resultBlock = GetBlockById(blockId);
            return resultBlock != null;
        }

        public IReadOnlyList<Block> GetColumnBlocksAtId(int blockId)
        {
            List<Block> result = new List<Block>(m_OriginalBoardSize.y);

            int columnIndex = blockId % m_OriginalBoardSize.x;

            for (int i = columnIndex; i < m_BlockCount; i += m_OriginalBoardSize.x)
            {
                result.Add(GetBlockById(i));
            }

            return result;
        }

        public IReadOnlyList<Block> GetRowBlocksAtId(int blockId)
        {
            List<Block> result = new List<Block>(m_OriginalBoardSize.x);
            int rowNumber = GetRowNumberById(blockId);
            int startBlockId = rowNumber * m_OriginalBoardSize.y;

            for (int i = startBlockId; i < (startBlockId + m_OriginalBoardSize.y); i++)
            {
                result.Add(GetBlockById(i));
            }

            return result;
        }

        public IReadOnlyList<Block> GetBlokcsAtRow(int rowIndex, bool isVisible = false)
        {
            List<Block> result = new List<Block>(m_OriginalBoardSize.x);

            for (int i = (rowIndex * m_OriginalBoardSize.x); i < (m_OriginalBoardSize.x * (rowIndex + 1)); i++)
            {
                Block block = GetBlockById(i);
                if (isVisible)
                {
                    if (block.IsVisible)
                    {
                        result.Add(block);
                    }
                }
                else
                {
                    result.Add(block);
                }
            }

            return result;
        }

        public IReadOnlyList<Block> GetBlocksAtColumn(int columnIndex)
        {
            List<Block> result = new List<Block>(m_OriginalBoardSize.x);

            for (int i = columnIndex; i < m_BlockCount; i += m_OriginalBoardSize.x)
            {
                result.Add(GetBlockById(i));
            }

            return result;
        }

        public int GetRowNumberById(int id)
        {
            return id / m_OriginalBoardSize.x;
        }

        public int GetColumnNumberById(int id)
        {
            return id % m_OriginalBoardSize.x;
        }

        public IReadOnlyList<Cookie> GetColumnCookiesAtId(int blockId)
        {
            TryGetBlockAt(0, GetColumnNumberById(blockId), out Block firstBlockInColumn);

            ContactFilter2D filter2D = new ContactFilter2D()
            {
                useTriggers = true
            };

            Physics2D.Raycast(firstBlockInColumn.transform.position, Vector2.down, filter2D, m_VerticalHitBuffer); //vertical

            return CheckBuffer(m_VerticalHitBuffer);
        }

        public IReadOnlyList<Cookie> GetRowCookiesAtId(int blockId)
        {
            TryGetBlockAt(GetRowNumberById(blockId), 0, out Block firstBlockInRaw);

            ContactFilter2D filter2D = new ContactFilter2D()
            {
                useTriggers = true
            };

            Physics2D.Raycast(firstBlockInRaw.transform.position, Vector2.right, filter2D, m_HorizontalHitBuffer); //horizontal

            return CheckBuffer(m_HorizontalHitBuffer);
        }

        private IReadOnlyList<Cookie> CheckBuffer(List<RaycastHit2D> hitBuffer)
        {
            List<Cookie> result = new List<Cookie>();

            for (int i = 0; i < hitBuffer.Count; i++)
            {
                RaycastHit2D hit = hitBuffer[i];
                if (hit.collider != null)
                {
                    if (hit.collider.TryGetComponent<Cookie>(out Cookie component))
                    {
                        if (component.Owner == m_BoardIdentity)
                        {
                            result.Add(component);
                        }
                    }
                }
            }
            hitBuffer.Clear();
            return result;
        }
    }
}