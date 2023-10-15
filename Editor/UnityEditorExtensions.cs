#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;

static class CustomEditorEx
{

    public static object GetPropertyValue(this SerializedProperty property)
    {
        object v = property.serializedObject.targetObject;
        foreach (var p in ResolvePath(property))
        {
            if (int.TryParse(p, out var idx))
            {
                v = ((IList)v)[idx];
            }
            else if (p is string name)
            {
                var mb = v.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                v = mb.GetValue(v);
            }
        }
        return v;
    }

    public static Type GetPropertyType(this SerializedProperty property)
    {
        Type type = property.serializedObject.targetObject.GetType();
        foreach (var p in ResolvePath(property))
        {

            if (int.TryParse(p, out var idx))
            {
                type = type.GetElementType();
            }
            else if (p is string name)
            {
                var mb = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (mb is null)
                {
                    Debug.LogWarning($"cannot find {name}");
                }
                type = mb.FieldType;
            }
        }
        return type;
    }

    internal static IEnumerable<string> ResolvePath(this SerializedProperty property)
    {
        var path = property.propertyPath;
        var match_rules = new[]{
            "^Array\\.data\\[([0-9]+)\\]\\.?",
            "^([^\\.]+)\\.?",
        };
        while (path.Length > 0)
        {
            var prev_len = path.Length;
            foreach (var rule in match_rules)
            {
                var mat = Regex.Match(path, rule);
                if (mat.Groups.Count > 1)
                {
                    var result = mat.Groups[1].Value;
                    path = path[mat.Length..];
                    yield return result;
                    break;
                }
            }
            Assert.IsTrue(path.Length < prev_len, $"resolve path fail '{path}' of '{property.propertyPath}'");
        }
    }
}
#endif