using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Quaternion))]
public class EulerAnglesProperty : PropertyDrawer {

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		Vector3 euler = property.quaternionValue.eulerAngles;
		Vector3 euler2 = EditorGUI.Vector3Field(position, property.name, euler);
		if (euler != euler2)
		{
			property.quaternionValue = Quaternion.Euler(euler2);
		}
	}
}
