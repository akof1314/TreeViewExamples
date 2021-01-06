using System.Collections;
using System.Collections.Generic;
using UnityEditor.WuHuan;
using UnityEngine;

[CreateAssetMenu(fileName = "SimpleDemo2", menuName = "SimpleDemo2 Setting", order = 1)]
public class SimpleDemo2 : ScriptableObject
{
    [SerializeField]
    public SimpleListView simpleListView;
}
