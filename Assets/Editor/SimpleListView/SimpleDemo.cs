using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.WuHuan;
using UnityEngine;

public class SimpleDemo : EditorWindow
{
    [MenuItem("TreeView Examples/SimpleDemo")]
    static void ShowWindow()
    {
        // Get existing open window or if none, make a new one:
        var window = GetWindow<SimpleDemo>();
        window.titleContent = new GUIContent("SimpleDemo");
        window.Show();
    }

    [SerializeField]
    public SimpleListView simpleListView;
    [SerializeField]
    public SimpleListView simpleListView2;

    private void OnEnable()
    {
        if (simpleListView == null)
        {
            simpleListView = new SimpleListView();
        }

        if (simpleListView.IsNotInitialized())
        {
            simpleListView.Init();
            SimpleListViewItem item1 = new SimpleListViewItem("item1");
            SimpleListViewItem item2 = new SimpleListViewItem("item2");
            SimpleListViewItem item3 = new SimpleListViewItem("item3");

            simpleListView.items.Add(item1);
            simpleListView.items.Add(item2);
            simpleListView.items.Add(item3);
            simpleListView.EndUpdate();
        }

        if (simpleListView2 == null)
        {
            simpleListView2 = new SimpleListView();
        }

        if (simpleListView2.IsNotInitialized())
        {
            simpleListView2.Init(new[]{
                new SimpleColumnHeader("Item Column"),
                new SimpleColumnHeader("Column 2"),
                new SimpleColumnHeader("Column 3"),
                new SimpleColumnHeader("Column 4"),
            });

            simpleListView2.showSearchField = true;
            simpleListView2.showAddButton = true;
            simpleListView2.showRemoveButton = true;
            simpleListView2.canMultiSelect = true;
            simpleListView2.onItemSelectionChangedCallback = OnItemSelectionChangedCallback;
            simpleListView2.onItemDoubleClickedCallback = OnItemDoubleClickedCallback;
            simpleListView2.onItemContextClickedCallback = OnItemContextClickedCallback;
            simpleListView2.onItemDrawCallback = OnItemDrawCallback;

            SimpleListViewItem item1 = new SimpleListViewItem("item4");
            item1.subItems.Add("1");
            item1.subItems.Add("2");
            item1.subItems.Add("3");
            SimpleListViewItem item2 = new SimpleListViewItem("item5");
            item2.subItems.Add("4");
            item2.subItems.Add("5");
            item2.subItems.Add("6");
            SimpleListViewItem item3 = new SimpleListViewItem("item6");
            item3.subItems.Add("7");
            item3.subItems.Add("8");
            item3.subItems.Add("9");

            simpleListView2.items.Add(item1);
            simpleListView2.items.Add(item2);
            simpleListView2.items.Add(item3);
            for (int i = 0; i < 1000; i++)
            {
                simpleListView2.items.Add(new SimpleListViewItem(i.ToString()));
            }
            simpleListView2.EndUpdate();
        }
    }

    private void OnItemDrawCallback(SimpleListView arg1, SimpleListViewItem item, int column, Rect rect, bool arg4, bool arg5)
    {
        if (column == 0)
        {
            item.displayName = EditorGUI.TextField(rect, item.displayName);
        }
        else
        {
            EditorGUI.TextField(rect, String.Empty);
        }
    }

    private void OnItemContextClickedCallback(SimpleListView arg1, int arg2)
    {
        Debug.Log("右键 " + arg1.items[arg2].displayName);
    }

    private void OnItemDoubleClickedCallback(SimpleListView arg1, int arg2)
    {
        Debug.Log("双击 " + arg1.items[arg2].displayName);
    }

    private void OnItemSelectionChangedCallback(SimpleListView arg1, IList<int> arg2)
    {
        foreach (var idx in arg2)
        {
            Debug.Log("选中了 " + arg1.items[idx].displayName);
        }
    }

    void OnGUI()
    {
        simpleListView.OnGUI(new Rect(10, 10, 200, 800));
        simpleListView2.OnGUI(new Rect(220, 10, 200, 800));
    }
}
