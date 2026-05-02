#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CupkekGames.VFX
{
    [CustomPropertyDrawer(typeof(ShaderPropertyFadeData))]
    public class ShaderPropertyFadeDataDrawer : PropertyDrawer
    {
        private const float Spacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty propertyName = property.FindPropertyRelative("PropertyName");
            SerializedProperty propertyType = property.FindPropertyRelative("PropertyType");

            // Foldout header - show property name if set, otherwise default label
            string headerText = !string.IsNullOrEmpty(propertyName.stringValue)
                ? propertyName.stringValue
                : label.text;

            property.isExpanded = EditorGUI.Foldout(
                new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
                property.isExpanded,
                headerText,
                true
            );

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                float lineHeight = EditorGUIUtility.singleLineHeight;
                float y = position.y + lineHeight + Spacing;

                // Property Name
                Rect nameRect = new Rect(position.x, y, position.width, lineHeight);
                EditorGUI.PropertyField(nameRect, propertyName, new GUIContent("Property Name"));
                y += lineHeight + Spacing;

                // Property Type
                Rect typeRect = new Rect(position.x, y, position.width, lineHeight);
                EditorGUI.PropertyField(typeRect, propertyType, new GUIContent("Type"));
                y += lineHeight + Spacing;

                ShaderPropertyType type = (ShaderPropertyType)propertyType.enumValueIndex;

                switch (type)
                {
                    case ShaderPropertyType.Float:
                        DrawFloatFields(position, property, ref y, lineHeight);
                        break;

                    case ShaderPropertyType.Color:
                        DrawColorFields(position, property, ref y, lineHeight);
                        break;

                    case ShaderPropertyType.ColorAlpha:
                        DrawFloatFields(position, property, ref y, lineHeight);
                        break;

                    case ShaderPropertyType.Vector:
                        DrawVectorFields(position, property, ref y, lineHeight);
                        break;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        private void DrawFloatFields(Rect position, SerializedProperty property, ref float y, float lineHeight)
        {
            SerializedProperty fadeOut = property.FindPropertyRelative("FadeOutValue");
            SerializedProperty fadeIn = property.FindPropertyRelative("FadeInValue");

            Rect fadeOutRect = new Rect(position.x, y, position.width, lineHeight);
            EditorGUI.PropertyField(fadeOutRect, fadeOut, new GUIContent("Fade Out Value"));
            y += lineHeight + Spacing;

            Rect fadeInRect = new Rect(position.x, y, position.width, lineHeight);
            EditorGUI.PropertyField(fadeInRect, fadeIn, new GUIContent("Fade In Value"));
            y += lineHeight + Spacing;
        }

        private void DrawColorFields(Rect position, SerializedProperty property, ref float y, float lineHeight)
        {
            SerializedProperty fadeOutColor = property.FindPropertyRelative("FadeOutColor");
            SerializedProperty fadeInColor = property.FindPropertyRelative("FadeInColor");

            Rect fadeOutRect = new Rect(position.x, y, position.width, lineHeight);
            EditorGUI.PropertyField(fadeOutRect, fadeOutColor, new GUIContent("Fade Out Color"));
            y += lineHeight + Spacing;

            Rect fadeInRect = new Rect(position.x, y, position.width, lineHeight);
            EditorGUI.PropertyField(fadeInRect, fadeInColor, new GUIContent("Fade In Color"));
            y += lineHeight + Spacing;
        }

        private void DrawVectorFields(Rect position, SerializedProperty property, ref float y, float lineHeight)
        {
            SerializedProperty fadeOutVector = property.FindPropertyRelative("FadeOutVector");
            SerializedProperty fadeInVector = property.FindPropertyRelative("FadeInVector");

            Rect fadeOutRect = new Rect(position.x, y, position.width, lineHeight);
            EditorGUI.PropertyField(fadeOutRect, fadeOutVector, new GUIContent("Fade Out Vector"));
            y += lineHeight + Spacing;

            Rect fadeInRect = new Rect(position.x, y, position.width, lineHeight);
            EditorGUI.PropertyField(fadeInRect, fadeInVector, new GUIContent("Fade In Vector"));
            y += lineHeight + Spacing;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float height = lineHeight; // Foldout

            if (!property.isExpanded)
                return height;

            // PropertyName + PropertyType = 2 lines
            height += (lineHeight + Spacing) * 2;

            SerializedProperty propertyType = property.FindPropertyRelative("PropertyType");
            ShaderPropertyType type = (ShaderPropertyType)propertyType.enumValueIndex;

            switch (type)
            {
                case ShaderPropertyType.Float:
                    // FadeOutValue + FadeInValue = 2 lines
                    height += (lineHeight + Spacing) * 2;
                    break;

                case ShaderPropertyType.Color:
                    // FadeOutColor + FadeInColor = 2 lines
                    height += (lineHeight + Spacing) * 2;
                    break;

                case ShaderPropertyType.ColorAlpha:
                    // FadeOutValue (alpha) + FadeInValue (alpha) = 2 lines
                    height += (lineHeight + Spacing) * 2;
                    break;

                case ShaderPropertyType.Vector:
                    // FadeOutVector + FadeInVector = 2 lines
                    height += (lineHeight + Spacing) * 2;
                    break;
            }

            // Bottom padding
            height += Spacing * 2;

            return height;
        }
    }
}
#endif
