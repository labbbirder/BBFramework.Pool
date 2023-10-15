using System;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace BBFramework.Pool.Editor
{
    [CustomPropertyDrawer(typeof(IPool), useForChildren: true)]
    class PoolEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            IPool pool = property.GetPropertyValue() as IPool;
            EditorGUI.LabelField(position, label.text, pool?.GetType().Name ?? "<null>");

            EditorGUILayout.BeginVertical(GUI.skin.box);
            // EditorGUI.indentLevel += 1;
            EditorGUILayout.LabelField("Cache Type", pool.Cache?.GetType().Name ?? "<null>");
            if (pool.Cache != null) DrawContent(pool);
            EditorGUILayout.EndVertical();
            // EditorGUI.indentLevel -= 1;
        }

        void DrawContent(IPool pool)
        {
            // toggle = EditorGUILayout.BeginFoldoutHeaderGroup(toggle, "content");
            // EditorGUILayout.LabelField("Cached Types", pool.Cache.Count().ToString());

            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(GUILayout.Height(1)), Color.gray);
            EditorGUILayout.BeginHorizontal();
            var w = EditorGUIUtility.currentViewWidth;
            EditorGUILayout.LabelField("type", new GUIStyle()
            {
                normal = new GUIStyleState()
                {
                    textColor = Color.gray,
                },

            }, GUILayout.Width(w / 2));
            EditorGUILayout.LabelField("in pool", new GUIStyle()
            {
                normal = new GUIStyleState()
                {
                    textColor = Color.gray,
                },
            }, GUILayout.Width(w / 4));
            EditorGUILayout.LabelField("out of pool", new GUIStyle()
            {
                normal = new GUIStyleState()
                {
                    textColor = Color.gray,
                },
            }, GUILayout.Width(w / 4));
            EditorGUILayout.EndHorizontal();
            // var rawColor = GUI.backgroundColor;
            var idx = 0;
            foreach (var rec in pool)
            {
                idx++;
                // GUI.backgroundColor = idx%2==0?rawColor:new Color(0,0,0,0.3f);
                // GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
                EditorGUI.DrawRect(EditorGUILayout.GetControlRect(GUILayout.Height(1)), Color.gray);
                var rect = EditorGUILayout.BeginHorizontal();
                if (idx % 2 == 1)
                {
                    EditorGUI.DrawRect(rect, new Color(0, 0, 0, 0.3f));
                }
                EditorGUILayout.LabelField(rec.type.Name, GUILayout.Width(w / 2));
                EditorGUILayout.LabelField(rec.pooledCount.ToString(), GUILayout.Width(w / 4));
                EditorGUILayout.LabelField(rec.outsideCount.ToString(), GUILayout.Width(w / 4));
                EditorGUILayout.EndHorizontal();
            }
            // GUI.backgroundColor = rawColor;
            // EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}