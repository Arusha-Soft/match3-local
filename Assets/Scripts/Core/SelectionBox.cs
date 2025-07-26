using Project.InputHandling;
using System.Collections;
using UnityEngine;

namespace Project.Core
{
    public class SelectionBox : MovableTile
    {
        [SerializeField] private GameObject m_SelectionBox;
        [SerializeField] private int m_DefaultBlockId = 8;

        public int CurrentBlockId { private set; get; }

        private BoardInputHandler m_Input;
        private BoardData m_BoardData;
        private Coroutine m_Checking;
        private Block m_TargetMoveBlock;

        private bool m_IsInMoving => m_TargetMoveBlock != null;

        public void Init(BoardInputHandler input, BoardData boardData)
        {
            m_Input = input;
            m_BoardData = boardData;
            CurrentBlockId = m_DefaultBlockId;

            m_SelectionBox.SetActive(true);
            m_SelectionBox.transform.position = m_BoardData.GetBlockById(m_DefaultBlockId).transform.position;
            m_Checking = StartCoroutine(CheckAndMove());
        }

        private IEnumerator CheckAndMove()
        {
            while (true)
            {
                if (!m_IsInMoving && !m_Input.SelectIsPressed)
                {
                    if (m_Input.IsUp)
                    {
                        if (m_BoardData.TryGetUpBlockIdOf(CurrentBlockId, out int blockId))
                        {
                            DoMove(blockId);
                        }
                    }
                    else if (m_Input.IsDown)
                    {
                        if (m_BoardData.TryGetDownBlockIdOf(CurrentBlockId, out int blockId))
                        {
                            DoMove(blockId);
                        }
                    }
                    else if (m_Input.IsRight)
                    {
                        if (m_BoardData.TryGetRightIdOf(CurrentBlockId, out int blockId))
                        {
                            DoMove(blockId);
                        }
                    }
                    else if (m_Input.IsLeft)
                    {
                        if (m_BoardData.TryGetLeftIdOf(CurrentBlockId, out int blockId))
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
                TryMove(m_TargetMoveBlock.transform);
            }
        }

        protected override void OnFinishMoving(MovableTile movableTile)
        {
            CurrentBlockId = m_TargetMoveBlock.Id;
            m_TargetMoveBlock = null;
        }
    }
}