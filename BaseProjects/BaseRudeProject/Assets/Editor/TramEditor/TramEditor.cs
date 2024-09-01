using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Train;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;

[CustomEditor(typeof(TrainTrackPoint))]
public class TramEditor : Editor
{
	bool forwardPathFold = true;

	SerializedProperty forwardPoints;
	SerializedProperty backwardPoints;
	SerializedProperty stopBehaviour;
	SerializedProperty forwardPath;
	SerializedProperty backwardPath;

	SerializedProperty forwardCurveSettings_curve;
	SerializedProperty forwardCurveSettings_angle;
	SerializedProperty forwardCurveSettings_flipCurve;

	private static void SetIcon(GameObject go, Texture2D icon)
	{
		var editorGUIUtilityType = typeof(EditorGUIUtility);
		var bindingFlags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
		var args = new object[] { go, icon };
		editorGUIUtilityType.InvokeMember("SetIconForObject", bindingFlags, null, null, args);
	}

	void OnEnable()
	{
		forwardPoints = serializedObject.FindProperty(nameof(TrainTrackPoint.forwardPoints));
		backwardPoints = serializedObject.FindProperty(nameof(TrainTrackPoint.backwardPoints));
		stopBehaviour = serializedObject.FindProperty(nameof(TrainTrackPoint.stopBehaviour));
		forwardPath = serializedObject.FindProperty(nameof(TrainTrackPoint.forwardPath));
		backwardPath = serializedObject.FindProperty(nameof(TrainTrackPoint.backwardPath));

		forwardCurveSettings_curve = serializedObject.FindProperty($"{nameof(TrainTrackPoint.forwardCurveSettings)}.{nameof(TrackCurveSettings.curve)}");
		forwardCurveSettings_angle = serializedObject.FindProperty($"{nameof(TrainTrackPoint.forwardCurveSettings)}.{nameof(TrackCurveSettings.angle)}");
		forwardCurveSettings_flipCurve = serializedObject.FindProperty($"{nameof(TrainTrackPoint.forwardCurveSettings)}.{nameof(TrackCurveSettings.flipCurve)}");

		serializedObject.Update();
		TrainTrackPoint script = target as TrainTrackPoint;

		SetIcon(script.gameObject, (Texture2D)EditorGUIUtility.IconContent("sv_icon_dot9_pix16_gizmo").image);
		
		if (script.instanceId != script.gameObject.GetInstanceID())
		{
			IEnumerable<GameObject> GetAllGameObjects(GameObject parent)
			{
				yield return parent;

				foreach (Transform childTrans in parent.transform)
					foreach (GameObject child in GetAllGameObjects(childTrans.gameObject))
						yield return child;
			}

			GameObject original = null;
			foreach (GameObject rootObj in script.gameObject.scene.GetRootGameObjects())
			{
				original = GetAllGameObjects(rootObj).Where(go => go.GetInstanceID() == script.instanceId).FirstOrDefault();
				if (original != null)
					break;
			}

			if (original != null && original.TryGetComponent(out TrainTrackPoint originalPoint))
			{
				Undo.RecordObjects(new Object[2] { script, originalPoint }, "Connect Train Tracks");

				// Try connect backward
				if (originalPoint.forwardPoints.Count != 0 && originalPoint.backwardPoints.Count == 0)
				{
					originalPoint.backwardPoints.Add(script);
					script.forwardPoints.Clear();
					script.backwardPoints.Clear();
					script.forwardPoints.Add(originalPoint);
				}
				else
				{
					originalPoint.forwardPoints.Add(script);
					script.forwardPoints.Clear();
					script.backwardPoints.Clear();
					script.backwardPoints.Add(originalPoint);
				}
			}

			script.instanceId = script.gameObject.GetInstanceID();
		}

		serializedObject.ApplyModifiedProperties();
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.PropertyField(forwardPoints);
		EditorGUILayout.PropertyField(backwardPoints);
		EditorGUILayout.PropertyField(stopBehaviour);
		EditorGUILayout.PropertyField(forwardPath);
		EditorGUILayout.PropertyField(backwardPath);

		TrainTrackPoint script = target as TrainTrackPoint;
		if (script.forwardPoints.Count != 0 && script.forwardPoints[0] != null)
		{
			forwardPathFold = EditorGUILayout.Foldout(forwardPathFold, "Forward Path");
			if (forwardPathFold)
			{
				EditorGUI.indentLevel++;

				EditorGUILayout.PropertyField(forwardCurveSettings_curve);
				if (script.forwardCurveSettings.curve == PathInterpolation.SphericalAutomatic)
					EditorGUILayout.PropertyField(forwardCurveSettings_angle);
				if (script.forwardCurveSettings.curve == PathInterpolation.SphericalAutomatic)
					EditorGUILayout.PropertyField(forwardCurveSettings_flipCurve);

				EditorGUI.indentLevel--;
			}
		}

		if (script.forwardCurveSettings.curve != PathInterpolation.SphericalManual && script.forwardCurveSettings.handle != null)
		{
			DestroyImmediate(script.forwardCurveSettings.handle.gameObject);
			script.forwardCurveSettings.handle = null;
		}
		else if (script.forwardCurveSettings.curve == PathInterpolation.SphericalManual && script.forwardCurveSettings.handle == null)
		{
			GameObject handle = new GameObject();
			handle.name = "Handle";
			script.forwardCurveSettings.handle = handle.transform;
			handle.transform.parent = script.transform;

			var forward = script.forwardPoints.FirstOrDefault();
			if (forward != null)
			{
				Vector3 direction = forward.transform.position - script.transform.position;
				Vector3 perpendicular = new Vector3(direction.z, 0, -direction.x).normalized;

				handle.transform.position = (script.transform.position + forward.transform.position) * 0.5f;
				handle.transform.position -= perpendicular * Vector3.Distance(script.transform.position, forward.transform.position) * 0.5f;
			}
			else
				handle.transform.position = script.transform.position + script.transform.forward * 5f;

			SetIcon(handle, (Texture2D)EditorGUIUtility.IconContent("sv_icon_dot15_pix16_gizmo").image);
		}

		serializedObject.ApplyModifiedProperties();
	}
}
