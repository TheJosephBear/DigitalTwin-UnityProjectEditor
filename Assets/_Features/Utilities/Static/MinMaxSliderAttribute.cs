using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Shows a Vector2 field as a min/max slider (x = min, y = max) between the given limits, with number fields on both sides.
/// </summary>
public class MinMaxSliderAttribute : PropertyAttribute {
    public readonly float Min;
    public readonly float Max;

    public MinMaxSliderAttribute(float min, float max) {
        Min = min;
        Max = max;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
public class MinMaxSliderDrawer : PropertyDrawer {
    const float FieldWidth = 50f;
    const float Spacing = 4f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        if (property.propertyType != SerializedPropertyType.Vector2) {
            EditorGUI.LabelField(position, label.text, "Use MinMaxSlider with Vector2");
            return;
        }

        MinMaxSliderAttribute limits = (MinMaxSliderAttribute)attribute;
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, label);

        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        Vector2 range = property.vector2Value;
        float min = range.x;
        float max = range.y;

        Rect minRect = new Rect(position.x, position.y, FieldWidth, position.height);
        Rect sliderRect = new Rect(minRect.xMax + Spacing, position.y, position.width - 2f * (FieldWidth + Spacing), position.height);
        Rect maxRect = new Rect(sliderRect.xMax + Spacing, position.y, FieldWidth, position.height);

        EditorGUI.BeginChangeCheck();
        min = EditorGUI.FloatField(minRect, min);
        EditorGUI.MinMaxSlider(sliderRect, ref min, ref max, limits.Min, limits.Max);
        max = EditorGUI.FloatField(maxRect, max);
        if (EditorGUI.EndChangeCheck()) {
            min = Mathf.Clamp(min, limits.Min, limits.Max);
            max = Mathf.Clamp(max, min, limits.Max);
            property.vector2Value = new Vector2(min, max);
        }

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}
#endif
