using System.Collections.Generic;
using UnityEngine;

namespace Project.Core
{
    public class BoardData : MonoBehaviour
    {
        [SerializeField] private Vector2Int m_OriginalBoardSize;
        [SerializeField] private Vector2Int m_VisibleBoardSize;
        [SerializeField] private List<Block> m_Blocks;

        public IReadOnlyList<Block> VisibleBlocks => m_VisibleBlocks;

        private List<Block> m_VisibleBlocks = new List<Block>();
        private Dictionary<int, Block> m_BlockDictionary;

        public void Init()
        {
            m_BlockDictionary = new Dictionary<int, Block>();

            for (int i = 0; i < m_Blocks.Count; i++)
            {
                Block block = m_Blocks[i];
                m_BlockDictionary.Add(block.Id, block);

                if (block.IsVisible)
                {
                    m_VisibleBlocks.Add(block);
                }
            }
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
    }
}