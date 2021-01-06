using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityEditor.WuHuan
{
    [Serializable]
    public class SimpleListView
    {
        [SerializeField]
        private TreeViewState m_TreeViewState;
        [SerializeField]
        private MultiColumnHeaderState m_MultiColumnHeaderState;
        private MultiColumnHeader m_MultiColumnHeader;
        private SimpleTreeView m_TreeView;
        private SearchField m_SearchField;

        public List<SimpleListViewItem> items { get; private set; }

        /// <summary>
        /// 显示搜索框
        /// </summary>
        public bool showSearchField { get; set; }

        /// <summary>
        /// 显示添加按钮
        /// </summary>
        public bool showAddButton { get; set; }

        /// <summary>
        /// 显示移除按钮
        /// </summary>
        public bool showRemoveButton { get; set; }

        /// <summary>
        /// 能否多选
        /// </summary>
        public bool canMultiSelect { get; set; }

        // 事件
        public Action<SimpleListView, IList<int>> onItemSelectionChangedCallback;
        public Action<SimpleListView, int> onItemDoubleClickedCallback;
        public Action<SimpleListView, int> onItemContextClickedCallback;
        public Action<SimpleListView> onItemAddCallback;
        public Action<SimpleListView, int> onItemRemoveCallback;
        public Action<SimpleListView, SimpleListViewItem, int, Rect, bool, bool> onItemDrawCallback;

        public SimpleListView()
        {
            m_TreeViewState = new TreeViewState();
        }

        public bool IsNotInitialized()
        {
            return items == null;
        }

        public void Init(SimpleColumnHeader[] columns = null)
        {
            if (items != null)
            {
                return;
            }
            items = new List<SimpleListViewItem>();

            if (columns == null)
            {
                m_TreeView = new SimpleTreeView(this, m_TreeViewState);
            }
            else
            {
                MultiColumnHeaderState.Column[] columnList = new MultiColumnHeaderState.Column[columns.Length];
                for (int i = 0; i < columns.Length; i++)
                {
                    columnList[i] = new MultiColumnHeaderState.Column
                    {
                        headerContent = new GUIContent(columns[i].text),
                        width = columns[i].width,
                        canSort = false,
                        allowToggleVisibility = false
                    };
                }

                MultiColumnHeaderState headerState = new MultiColumnHeaderState(columnList);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_MultiColumnHeaderState, headerState))
                {
                    MultiColumnHeaderState.OverwriteSerializedFields(m_MultiColumnHeaderState, headerState);
                }
                m_MultiColumnHeaderState = headerState;
                m_MultiColumnHeader = new MultiColumnHeader(m_MultiColumnHeaderState) { height = 21f };
                m_TreeView = new SimpleTreeView(this, m_TreeViewState, m_MultiColumnHeader);
            }
            m_SearchField = new SearchField();
            m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;
        }

        public void EndUpdate()
        {
            m_TreeView.Reload();
        }

        public void OnGUI(Rect rect)
        {
            if (showSearchField || showAddButton || showRemoveButton)
            {
                Rect rectSearch = new Rect(rect.x, rect.y, rect.width, 16f);
                rect.height -= 16f;
                rect.y += 16f;

                if (showAddButton)
                {
                    rectSearch.width -= 20;
                }
                if (showRemoveButton)
                {
                    rectSearch.width -= 20;
                }
                if (showSearchField)
                {
                    m_TreeView.searchString = m_SearchField.OnToolbarGUI(rectSearch, m_TreeView.searchString);
                }

                rectSearch.xMin = rectSearch.xMax + 2;
                rectSearch.width = 20;
                if (showAddButton)
                {
                    if (GUI.Button(rectSearch, EditorGUIUtility.TrTextContent("", "Add to list"), "OL Plus"))
                    {
                        int count = items.Count;
                        if (onItemAddCallback != null)
                        {
                            onItemAddCallback(this);
                        }
                        else
                        {
                            items.Add(new SimpleListViewItem("New Item"));
                        }

                        if (count != items.Count)
                        {
                            EndUpdate();
                            m_TreeView.SetSelection(new[] { items.Count - 1 }, TreeViewSelectionOptions.RevealAndFrame | TreeViewSelectionOptions.FireSelectionChanged);
                        }
                    }
                    rectSearch.xMin = rectSearch.xMax - 2;
                    rectSearch.width = 20;
                }

                if (showRemoveButton)
                {
                    if (GUI.Button(rectSearch, EditorGUIUtility.TrTextContent("", "Remove selection from list"), "OL Minus"))
                    {
                        int[] selectionIds = (int[])m_TreeView.GetSelection();
                        if (selectionIds.Length > 0)
                        {
                            int count = items.Count;
                            Array.Sort(selectionIds);
                            for (int i = selectionIds.Length - 1; i >= 0; i--)
                            {
                                if (onItemRemoveCallback != null)
                                {
                                    onItemRemoveCallback(this, i);
                                }
                                else
                                {
                                    items.RemoveAt(selectionIds[i]);
                                }
                            }

                            if (count != items.Count)
                            {
                                m_TreeView.SetSelection(new List<int>(), TreeViewSelectionOptions.FireSelectionChanged);
                                EndUpdate();
                            }
                        }
                    }
                }
            }
            m_TreeView.OnGUI(rect);
        }
    }

    public class SimpleListViewItem : TreeViewItem
    {
        private List<string> m_SubItems;

        public List<string> subItems
        {
            get
            {
                if (m_SubItems == null)
                {
                    m_SubItems = new List<string>();
                }

                return m_SubItems;
            }
        }

        public SimpleListViewItem(string text) : base(0, 0, text)
        {
        }
    }

    public class SimpleColumnHeader
    {
        public int width { get; }

        public string text { get; }

        public SimpleColumnHeader(string text)
        {
            this.text = text;
            this.width = 50;
        }

        public SimpleColumnHeader(string text, int width)
        {
            this.text = text;
            this.width = width;
        }
    }

    class SimpleTreeView : TreeView
    {
        private SimpleListView m_ListView;

        public SimpleTreeView(SimpleListView listView, TreeViewState treeViewState)
            : base(treeViewState)
        {
            Init(listView);
        }

        public SimpleTreeView(SimpleListView listView, TreeViewState state, MultiColumnHeader multiColumnHeader)
            : base(state, multiColumnHeader)
        {
            Init(listView);
        }

        private void Init(SimpleListView listView)
        {
            m_ListView = listView;
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            rowHeight = 18f;
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };

            if (m_ListView.items.Count > 0)
            {
                for (var i = 0; i < m_ListView.items.Count; i++)
                {
                    var item = m_ListView.items[i];
                    item.id = i;
                    root.AddChild(item);
                }
            }

            return root;
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            // 这个函数不能删掉，否则会导致不能支持空项的情况
            IList<TreeViewItem> rows = base.BuildRows(root);
            return rows;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            if (multiColumnHeader == null)
            {
                CellGUI(args.rowRect, (SimpleListViewItem)args.item, 0, ref args);
            }
            else
            {
                var item = (SimpleListViewItem)args.item;

                for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                {
                    CellGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
                }
            }
        }

        void CellGUI(Rect cellRect, SimpleListViewItem item, int column, ref RowGUIArgs args)
        {
            if (m_ListView.onItemDrawCallback != null)
            {
                m_ListView.onItemDrawCallback(m_ListView, item, column, cellRect, args.selected, args.focused);
                return;
            }

            if (column == 0)
            {
                cellRect.xMin += DefaultStyles.label.margin.left;
                DefaultGUI.Label(cellRect, item.displayName, args.selected, args.focused);
            }
            else
            {
                if (item.subItems.Count >= column)
                {
                    DefaultGUI.Label(cellRect, item.subItems[column - 1], args.selected, args.focused);
                }
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (m_ListView.onItemSelectionChangedCallback != null)
            {
                m_ListView.onItemSelectionChangedCallback(m_ListView, selectedIds);
            }
        }

        protected override void DoubleClickedItem(int id)
        {
            if (m_ListView.onItemDoubleClickedCallback != null)
            {
                m_ListView.onItemDoubleClickedCallback(m_ListView, id);
            }
        }

        protected override void ContextClickedItem(int id)
        {
            if (m_ListView.onItemContextClickedCallback != null)
            {
                m_ListView.onItemContextClickedCallback(m_ListView, id);
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return m_ListView.canMultiSelect;
        }
    }
}
