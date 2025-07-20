using Project.InputHandling;
using System.Collections;
using UnityEngine;

namespace Project.Core
{
    public class SelectionBoxMover : MovableTile
    {
        [SerializeField] private GameObject m_SelectionBox;
        [SerializeField] private int m_DefaultBlockId = 8;

        private BoardInputHandler m_Input;
        private BoardData m_BoardData;
        private Coroutine m_Checking;
        private int m_CurrentBlockId;
        private Block m_TargetMoveBlock;

        private bool m_IsInMoving => m_TargetMoveBlock != null;

        public void Init(BoardInputHandler input, BoardData boardData)
        {
            m_Input = input;
            m_BoardData = boardData;
            m_CurrentBlockId = m_DefaultBlockId;

            m_SelectionBox.SetActive(true);
            m_SelectionBox.transform.position = m_BoardData.GetBlockById(m_DefaultBlockId).transform.position;
            m_Checking = StartCoroutine(CheckAndMove());
        }

        private IEnumerator CheckAndMove()
        {
            while (true)
            {
                if (!m_IsInMoving)
                {
                    if (m_Input.IsUp)
                    {
                        if (m_BoardData.TryGetUpBlockIdOf(m_CurrentBlockId, out int blockId))
                        {
                            DoMove(blockId);
                        }
                    }
                    else if (m_Input.IsDown)
                    {
                        if (m_BoardData.TryGetDownBlockIdOf(m_CurrentBlockId, out int blockId))
                        {
                            DoMove(blockId);
                        }
                    }
                    else if (m_Input.IsRight)
                    {
                        if (m_BoardData.TryGetRightIdOf(m_CurrentBlockId, out int blockId))
                        {
                            DoMove(blockId);
                        }
                    }
                    else if (m_Input.IsLeft)
                    {
                        if (m_BoardData.TryGetLeftIdOf(m_CurrentBlockId, out int blockId))
                        {
                            DoMove(blockId);
                        }
                    }
                }
                yield return null;
            }
        }

        private void DoMove(int blockId)
        {
            Block block = m_BoardData.GetBlockById(blockId);
            if (block.IsVisible)
            {
                m_TargetMoveBlock = block;
                Move(m_TargetMoveBlock.transform);
            }
        }

        protected override void OnFinishMoving(MovableTile movableTile)
        {
            m_CurrentBlockId = m_TargetMoveBlock.Id;
            m_TargetMoveBlock = null;
        }
    }
}