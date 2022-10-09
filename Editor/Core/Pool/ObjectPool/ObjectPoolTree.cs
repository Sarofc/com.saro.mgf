using System.Collections.Generic;
using System.Linq;
using Saro.Pool;
using Saro.Utility;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;


namespace Saro.Core
{
    internal class ObjectPoolTree : TreeView
    {
        private static Texture2D tex_warn = EditorGUIUtility.FindTexture("console.warnicon.sml");

        internal ObjectPoolTree(TreeViewState state, MultiColumnHeaderState mchs) : base(state, new MultiColumnHeader(mchs))
        {
            showBorder = true;
            showAlternatingRowBackgrounds = true;

            multiColumnHeader.canSort = false;
            //multiColumnHeader.sortingChanged += OnSortingChanged;
        }

        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            var _item = item as ObjectPoolItem;
            if (_item == null) return false;
            if (_item.pool == null) return false;
            return _item.pool.GetType().FullName.Contains(search, System.StringComparison.OrdinalIgnoreCase);
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };

            if (root.children == null)
                root.children = new List<TreeViewItem>();

            foreach (var pool in ObjectPoolChecker.s_ObjectPools)
            {
                root.AddChild(new ObjectPoolItem(pool));
            }

            return root;
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override bool CanBeParent(TreeViewItem item)
        {
            return false;
        }

        protected override bool CanChangeExpandedState(TreeViewItem item)
        {
            return false;
        }

        #region MyRegion

        internal static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            return new MultiColumnHeaderState(GetColumns());
        }
        private static MultiColumnHeaderState.Column[] GetColumns()
        {
            var retVal = new MultiColumnHeaderState.Column[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(nameof(SortOption.PoolType), ""),
                    minWidth = 250,
                    width = 400,
                    maxWidth = 420,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = true,
                    autoResize = true,
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(nameof(SortOption.RentCount), ""),
                    minWidth = 100,
                    width = 100,
                    maxWidth = 100,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = true,
                    autoResize = false,
                 },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(nameof(SortOption.ReturnCount), ""),
                    minWidth = 100,
                    width = 100,
                    maxWidth = 100,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = true,
                    autoResize = false,
                 },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(nameof(SortOption.CountAll), ""),
                    minWidth = 100,
                    width = 100,
                    maxWidth = 100,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = true,
                    autoResize = false,
                 },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(nameof(SortOption.CountInactive), ""),
                    minWidth = 100,
                    width = 100,
                    maxWidth = 100,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = true,
                    autoResize = false,
                 },
            };

            return retVal;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                CellGUI(args.GetCellRect(i), args.item as ObjectPoolItem, args.GetColumn(i), ref args);
        }

        private void CellGUI(Rect cellRect, ObjectPoolItem item, int column, ref RowGUIArgs args)
        {
            if (args.item.icon == null)
                extraSpaceBeforeIconAndLabel = 16f;
            else
                extraSpaceBeforeIconAndLabel = 0f;

            Color old = GUI.color;
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch ((SortOption)column)
            {
                case SortOption.PoolType:
                    DefaultGUI.Label(cellRect, item.PoolType, args.selected, args.focused);
                    break;
                case SortOption.RentCount:
                    DefaultGUI.Label(cellRect, item.RentCount.ToString(), args.selected, args.focused);
                    break;
                case SortOption.ReturnCount:
                    DefaultGUI.Label(cellRect, item.ReturnCount.ToString(), args.selected, args.focused);
                    break;
                case SortOption.CountAll:
                    DefaultGUI.Label(cellRect, item.CountAll.ToString(), args.selected, args.focused);
                    break;
                case SortOption.CountInactive:
                    DefaultGUI.Label(cellRect, item.CountInactive.ToString(), args.selected, args.focused);
                    break;
                default:
                    break;
            }

            GUI.color = old;
        }

        #region Sort

        internal enum SortOption
        {
            PoolType,
            RentCount,
            ReturnCount,
            CountAll,
            CountInactive,
        }

        private SortOption[] m_SortOptions =
        {
            SortOption.PoolType,
            SortOption.RentCount,
            SortOption.ReturnCount,
            SortOption.CountAll,
            SortOption.CountInactive,
        };

        private void OnSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            SortIfNeeded(rootItem, GetRows());
        }

        private void SortIfNeeded(TreeViewItem root, IList<TreeViewItem> rows)
        {
            if (rows.Count <= 1)
                return;

            if (multiColumnHeader.sortedColumnIndex == -1)
                return;

            SortByColumn();

            rows.Clear();
            for (int i = 0; i < root.children.Count; i++)
                rows.Add(root.children[i]);

            Repaint();
        }

        private void SortByColumn()
        {
            var sortedColumns = multiColumnHeader.state.sortedColumns;

            if (sortedColumns.Length == 0)
                return;

            List<ObjectPoolItem> assetList = new List<ObjectPoolItem>();
            foreach (var item in rootItem.children)
            {
                assetList.Add(item as ObjectPoolItem);
            }
            var orderedItems = InitialOrder(assetList, sortedColumns);

            rootItem.children = orderedItems.Cast<TreeViewItem>().ToList();
        }

        private IOrderedEnumerable<ObjectPoolItem> InitialOrder(IEnumerable<ObjectPoolItem> myTypes, int[] columnList)
        {
            SortOption sortOption = m_SortOptions[columnList[0]];
            bool ascending = multiColumnHeader.IsSortedAscending(columnList[0]);
            switch (sortOption)
            {
                case SortOption.PoolType:
                    return myTypes.Order(l => l.PoolType, ascending);
                case SortOption.RentCount:
                    return myTypes.Order(l => l.RentCount, ascending);
                case SortOption.ReturnCount:
                    return myTypes.Order(l => l.ReturnCount, ascending);
                case SortOption.CountAll:
                    return myTypes.Order(l => l.CountAll, ascending);
                case SortOption.CountInactive:
                    return myTypes.Order(l => l.CountInactive, ascending);
                default:
                    return myTypes.Order(l => l.displayName, ascending);
            }
        }

        #endregion

        #endregion
    }
}
