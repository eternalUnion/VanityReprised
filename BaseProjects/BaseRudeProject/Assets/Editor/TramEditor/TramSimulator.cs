using System;
using System.Collections;
using System.Collections.Generic;
using Train;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Tram))]
public class TramSimulator : Editor
{
	SerializedProperty poweredOn;
	SerializedProperty bonkSound;
	SerializedProperty deathZones;
	SerializedProperty speed;
	SerializedProperty connectedTrams;
	SerializedProperty currentPoint;

	void OnEnable()
	{
		poweredOn = serializedObject.FindProperty(nameof(Tram.poweredOn));
		bonkSound = serializedObject.FindProperty(nameof(Tram.bonkSound));
		deathZones = serializedObject.FindProperty(nameof(Tram.deathZones));
		speed = serializedObject.FindProperty(nameof(Tram.speed));
		connectedTrams = serializedObject.FindProperty(nameof(Tram.connectedTrams));
		currentPoint = serializedObject.FindProperty(nameof(Tram.currentPoint));
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.PropertyField(poweredOn);
		EditorGUILayout.PropertyField(bonkSound);
		EditorGUILayout.PropertyField(deathZones);
		EditorGUILayout.PropertyField(speed);
		EditorGUILayout.PropertyField(connectedTrams);
		EditorGUILayout.PropertyField(currentPoint);

		Tram script = target as Tram;

		if (Application.isPlaying)
		{
			EditorGUILayout.Space(15);

			GUILayout.BeginHorizontal();
			var control = script.GetComponentInChildren<TramControl>();
			if (GUILayout.Button("Speed up"))
			{
				if (control != null)
					control.SpeedUp(1);
			}
			if (GUILayout.Button("Speed down"))
			{
				if (control != null)
					control.SpeedDown(1);
			}
			if (GUILayout.Button("Zap"))
			{
				if (control != null)
					control.Zap();
			}
			GUILayout.Label($"Current speed: {(control == null ? "<controller not found>" : control.currentSpeedStep.ToString())}");
			GUILayout.EndHorizontal();

			EditorGUILayout.Space(15);

			GUILayout.Label($"Computed speed: {script.computedSpeed}");
			var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
			EditorGUI.ProgressBar(rect, script.inheritedSpeedMultiplier, $"Inherited speed multiplier: {script.inheritedSpeedMultiplier}");
			
			EditorGUILayout.Space(15);

			if (script.currentPath == null)
			{
				GUILayout.Label("<color=red>No path</color>", new GUIStyle() { richText = true });
			}
			else
			{
				var path = script.currentPath;

				GUILayout.Label($"<color=lime>({path.start.name}) {(script.movementDirection == TramMovementDirection.Forward ? "-->" : "<--")} ({path.end.name})</color>", new GUIStyle() { richText = true });
				var bigRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight * 2);
				EditorGUI.ProgressBar(bigRect, path.Progress, $"{path.distanceTravelled} / {path.DistanceTotal}");

				if (path.IsDeadEnd(script.movementDirection))
				{
					GUILayout.Label($"<color=yellow>Approaching dead end!</color>", new GUIStyle() { richText = true });
					GUILayout.Label($"<color=yellow>Stop behaviour: {path.end.stopBehaviour}</color>", new GUIStyle() { richText = true });
				}
			}
		}

		serializedObject.ApplyModifiedProperties();
	}
}
