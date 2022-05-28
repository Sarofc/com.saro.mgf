using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Saro.UI
{
    internal class SuperScrollRectImpl
    {
        public List<ScrollRectCell> CellList => m_CellList;

        /// <summary>
        /// 是否为网格
        /// </summary>
        public bool IsGrid => m_BaseSuperScrollRect.isGrid;
        /// <summary>
        /// 垂直 or 水平
        /// </summary>
        public bool Vertical => m_BaseSuperScrollRect.vertical;
        /// <summary>
        /// 每帧更新的cell个数
        /// </summary>
        public int MaxUpdateCountPerFrame { get; set; } = 4;
        /// <summary>
        /// 网格的个数
        /// </summary>
        private int GridNum => m_CellNum * m_Segment;
        /// <summary>
        /// 每个cell的宽高
        /// </summary>
        private float m_CellWidth, m_CellHeight;
        /// <summary>
        /// 如果是grid，vertical，则为列数，horizontal，则为行数
        /// </summary>
        private int m_Segment;
        /// <summary>
        /// 可见区域起始下标
        /// </summary>
        private int m_MinVisibleIndex;
        /// <summary>
        /// 可见区域结尾下标
        /// </summary>
        private int m_MaxVisibleIndex => Mathf.Min(m_MinVisibleIndex + GridNum, DataProvider.GetCellCount()) - 1;
        /// <summary>
        /// 行 or 列 的个数，根据direction决定
        /// </summary>
        private int m_CellNum;
        /// <summary>
        /// cell预制体
        /// </summary>
        private RectTransform CellPrefab => m_BaseSuperScrollRect.cellPrefab;
        private RectTransform Viewport => m_BaseSuperScrollRect.viewport;
        private ISuperScrollRectDataProvider DataProvider => m_BaseSuperScrollRect.DataProvider;
        private RectTransform Content => m_BaseSuperScrollRect.content;
        private BaseSuperScrollRect m_BaseSuperScrollRect;
        private readonly Stack<GameObject> m_GameObjectPool = new();
        private readonly List<ScrollRectCell> m_CellList = new();
        private readonly Dictionary<int, ScrollRectCell> m_Index2Cell = new();
        /// <summary>
        /// cell加载task队列
        /// </summary>
        private readonly Queue<int> m_TaskQueue = new();
        /// <summary>
        /// 已加载cell集合
        /// </summary>
        private readonly HashSet<int> m_CellLoadedSet = new();
        private Coroutine m_TaskProcessor;

        public void DoAwake(BaseSuperScrollRect baseSuperScrollRect)
        {
            m_BaseSuperScrollRect = baseSuperScrollRect;
            m_Segment = (IsGrid ? baseSuperScrollRect.segment : 1);

            m_CellWidth = CellPrefab.sizeDelta.x + baseSuperScrollRect.spacing.x;
            m_CellHeight = CellPrefab.sizeDelta.y + baseSuperScrollRect.spacing.y;
            m_CellNum = (Vertical ? Mathf.CeilToInt(Viewport.rect.height / m_CellHeight) : Mathf.CeilToInt(Viewport.rect.width / m_CellWidth)) + 1;
            DoReset();
        }

        public void DoStart()
        {
            SetAnchorTopAndLeft(Content);
            UpdateBounds();
            var size = Content.rect.size;
            var num = Mathf.CeilToInt((float)(DataProvider.GetCellCount() / m_Segment)) + 1;
            var vector = Vertical ? new Vector2(size.x, (float)num * m_CellHeight) : new Vector2((float)num * m_CellWidth, size.y);
            vector += m_BaseSuperScrollRect.padding;
            Content.sizeDelta = vector;
            DoReset();
            CheckVisibility();
            m_TaskProcessor = m_BaseSuperScrollRect.StartCoroutine(ProcessTasks());
        }

        public void JumpTo(int index)
        {
            index = Mathf.Clamp(index, 0, DataProvider.GetCellCount() - 1);
            var anchoredPosition = Content.anchoredPosition;
            var num = index / m_Segment;
            if (Vertical)
            {
                anchoredPosition = new(anchoredPosition.x, (float)num * m_CellHeight);
            }
            else
            {
                anchoredPosition = new((float)num * m_CellWidth, anchoredPosition.y);
            }
            Content.anchoredPosition = anchoredPosition;
            Refresh();
        }

        public void Refresh()
        {
            UpdateBounds();
            CheckVisibility();
        }

        private void DoReset()
        {
            ClearCacheCells(false);
            CellPrefab.gameObject.SetActive(true);
            SetAnchorTopAndLeft(CellPrefab);
            CellPrefab.gameObject.SetActive(false);
            if (m_TaskProcessor != null)
            {
                m_BaseSuperScrollRect.StopCoroutine(m_TaskProcessor);
            }
            m_TaskQueue.Clear();
            m_CellLoadedSet.Clear();
            m_CellList.Clear();
            m_Index2Cell.Clear();
        }

        public void ClearCacheCells(bool distroy = false)
        {
            m_TaskQueue.Clear();
            m_CellLoadedSet.Clear();
            foreach (ScrollRectCell scrollRectCell_ in m_CellList)
            {
                ReleaseCellGameObject(scrollRectCell_);
            }
            m_CellList.Clear();
            if (distroy)
            {
                var count = m_GameObjectPool.Count;
                for (int i = 0; i < count; i++)
                {
                    GameObject gameObject = m_GameObjectPool.Pop();
                    if (gameObject != null)
                    {
                        Object.Destroy(gameObject);
                    }
                }
                m_GameObjectPool.Clear();
                return;
            }
            // shrunk
            var num = m_CellNum * m_Segment;
            var num2 = m_GameObjectPool.Count - num;
            for (int j = 0; j < num2; j++)
            {
                GameObject go = m_GameObjectPool.Pop();
                if (go != null)
                {
                    Object.Destroy(go);
                }
            }
        }

        public void ClearCache()
        {
            ClearCacheCells(true);
            CheckVisibility();
        }

        private void UpdateBounds()
        {
            m_CellWidth = CellPrefab.sizeDelta.x + m_BaseSuperScrollRect.spacing.x;
            m_CellHeight = CellPrefab.sizeDelta.y + m_BaseSuperScrollRect.spacing.y;
            m_CellNum = (Vertical ? Mathf.CeilToInt(Viewport.rect.height / m_CellHeight) : Mathf.CeilToInt(Viewport.rect.width / m_CellWidth)) + 1;
            var anchoredPosition = Content.anchoredPosition;
            var num = Vertical ? Mathf.CeilToInt(anchoredPosition.y / m_CellHeight) : Mathf.CeilToInt(-anchoredPosition.x / m_CellWidth);
            m_MinVisibleIndex = Mathf.Max(0, num - 1) * m_Segment;
        }

        private void EnqueueTask(int index)
        {
            m_TaskQueue.Enqueue(index);
        }

        private void CheckVisibility()
        {
            m_Index2Cell.Clear();
            foreach (ScrollRectCell scrollRectCell in m_CellList)
            {
                m_Index2Cell[scrollRectCell.index] = scrollRectCell;
            }
            m_CellList.Clear();

            // load visible cells
            for (int i = m_MinVisibleIndex; i <= m_MaxVisibleIndex; i++)
            {
                if (!m_Index2Cell.ContainsKey(i))
                {
                    EnqueueTask(i);
                }
                else
                {
                    m_CellList.Add(m_Index2Cell[i]);
                }
                m_Index2Cell.Remove(i);
            }

            // destroy invisible cells
            foreach (var keyValuePair in m_Index2Cell)
            {
                var value = keyValuePair.Value;
                m_CellLoadedSet.Remove(keyValuePair.Key);
                ReleaseCellGameObject(value);
            }
        }

        private void SetAnchor(RectTransform rect, Vector2 anchor)
        {
            var width = rect.rect.width;
            var height = rect.rect.height;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(width, height);
        }

        private IEnumerator ProcessTasks()
        {
            while (true)
            {
                var num = 0;
                while (num < MaxUpdateCountPerFrame && m_TaskQueue.Count != 0)
                {
                    var index = m_TaskQueue.Dequeue();

                    // 在可见区域 且 没有被加载，就要加载数据了
                    if (IsVisible(index) && !m_CellLoadedSet.Contains(index))
                    {
                        var rectTransform = GetCellGameObject();
                        rectTransform.anchoredPosition = GetAnchorPositionByIndex(index);
                        m_CellList.Add(new ScrollRectCell
                        {
                            item = rectTransform,
                            index = index
                        });
                        m_CellLoadedSet.Add(index);
                        DataProvider.SetCell(rectTransform.gameObject, index);
                        num++;
                    }
                }
                yield return null;
            }
        }

        private bool IsVisible(int index)
        {
            return index >= m_MinVisibleIndex && index <= m_MaxVisibleIndex;
        }

        private void SetAnchorTopAndLeft(RectTransform rect)
        {
            SetAnchor(rect, new Vector2(0f, 1f));
        }

        private void ReleaseCellGameObject(ScrollRectCell cell)
        {
            var go = cell.item.gameObject;
            if (go == null)
            {
                return;
            }
            go.SetActive(false);
            m_GameObjectPool.Push(go);
        }

        private RectTransform GetCellGameObject()
        {
            if (m_GameObjectPool.Count > 0)
            {
                var go = m_GameObjectPool.Pop();
                if (go != null)
                {
                    go.gameObject.SetActive(true);
                    return go.transform as RectTransform;
                }
            }
            var rect = Object.Instantiate(CellPrefab.gameObject).transform as RectTransform;
            rect.gameObject.SetActive(true);
            rect.SetParent(Content, false);
            return rect;
        }

        private Vector2 GetAnchorPositionByIndex(int index)
        {
            int num;
            int num2;
            if (!Vertical)
            {
                num = index % m_Segment;
                num2 = index / m_Segment;
            }
            else
            {
                num = index / m_Segment;
                num2 = index % m_Segment;
            }
            return new Vector2((float)num2 * m_CellWidth, (float)(-(float)num) * m_CellHeight) + new Vector2(m_BaseSuperScrollRect.padding.x, -m_BaseSuperScrollRect.padding.y);
        }
    }
}
