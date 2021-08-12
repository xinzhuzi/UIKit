using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace UIKit
{
    [CustomEditor(typeof(UITableView), true)]
    internal class UITableViewEditor : Editor
    {
        
        
        private int _index = 0;
        private float _speed = 1000;
        bool lastReverseDirection;

        public void OnEnable()
        {
            var scroll = (UITableView)target;
            lastReverseDirection = scroll.reverseDirection;
        }

        public override void OnInspectorGUI ()
        {
            base.OnInspectorGUI();
            var scroll = (UITableView)target;
            if (lastReverseDirection != scroll.reverseDirection)
            {
                lastReverseDirection = scroll.reverseDirection;
                if (scroll.horizontal)
                {
                    if (lastReverseDirection)
                    {
                        scroll.content.anchorMin = Vector2.zero;
                        scroll.content.anchorMax = new Vector2(0, 1);
                        scroll.content.pivot = new Vector2(0, 0.5f);
                    }
                    else
                    {
                        scroll.content.anchorMin = new Vector2(1, 0);
                        scroll.content.anchorMax = new Vector2(1, 1);
                        scroll.content.pivot = new Vector2(1, 0.5f);
                    }
                }
                else
                {
                    if (lastReverseDirection)
                    {
                        scroll.content.anchorMin = Vector2.zero;
                        scroll.content.anchorMax = new Vector2(1, 0);
                        scroll.content.pivot = new Vector2(0.5f, 0);
                    }
                    else
                    {
                        scroll.content.anchorMin = new Vector2(0, 1);
                        scroll.content.anchorMax = Vector2.one;
                        scroll.content.pivot = new Vector2(0.5f, 1);
                    }
                }
            }
            
            
            EditorGUILayout.Space();
            GUI.enabled = Application.isPlaying;
            
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Clear"))
            {
                scroll.ClearCells();
            }
            if (GUILayout.Button("Refresh"))
            {
                scroll.RefreshCells();
            }
            if(GUILayout.Button("Refill"))
            {
                scroll.RefillCells();
            }
            if(GUILayout.Button("RefillFromEnd"))
            {
                scroll.RefillCellsFromEnd();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = 45;
            float w = (EditorGUIUtility.currentViewWidth - 100) / 2;
            EditorGUILayout.BeginHorizontal();
            _index = EditorGUILayout.IntField("Index", _index, GUILayout.Width(w));
            _speed = EditorGUILayout.FloatField("Speed", _speed, GUILayout.Width(w));
            if(GUILayout.Button("Scroll", GUILayout.Width(45)))
            {
                scroll.ScrollToCell(_index, _speed);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
