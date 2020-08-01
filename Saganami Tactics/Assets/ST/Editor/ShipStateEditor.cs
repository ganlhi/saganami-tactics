using System;
using ST.Scriptable;
using UnityEditor;
using UnityEngine;

namespace ST.Editor
{
    [CustomPropertyDrawer(typeof(ShipState))]
    public class ShipStatePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var nameProp = property.FindPropertyRelative("name");
            label.text = "Ship" + (nameProp.stringValue.Length > 0 ? ": "+nameProp.stringValue : string.Empty);
            
            EditorGUI.PropertyField(position, property, label, true);
            if (property.isExpanded)
            {
                if (GUI.Button(new Rect(position.xMin + 30f, position.yMax - 40f, position.width - 30f, 20f),
                    "Generate ID"))
                {
                    var idProp = property.FindPropertyRelative("uid");
                    idProp.stringValue = Utils.GenerateId();
                }

                if (GUI.Button(new Rect(position.xMin + 30f, position.yMax - 20f, position.width - 30f, 20f),
                    "Rotate forward"))
                {
                    var velProp = property.FindPropertyRelative("velocity");
                    var rotProp = property.FindPropertyRelative("rotation");

                    rotProp.quaternionValue = Quaternion.LookRotation(velProp.vector3Value);
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
                return EditorGUI.GetPropertyHeight(property) + 50f;
            return EditorGUI.GetPropertyHeight(property);
        }
    }
}