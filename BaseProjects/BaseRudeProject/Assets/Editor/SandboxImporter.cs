using Newtonsoft.Json;
using plog.Models;
using Sandbox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEditor.ProBuilder;
using EditorUtility = UnityEditor.EditorUtility;
using Object = UnityEngine.Object;

public class SandboxImporter : EditorWindow
{
	public string pathToSandboxMap;
	public Toggle subdivideFaces;
	public static bool sSubdivideFaces;

	[MenuItem("Tools/RUDE/Import Sandbox Map")]
	[MenuItem("RUDE/Tools/Import Sandbox Map", priority = 1000)]
	public static void OnWindow()
	{
		EditorWindow wnd = GetWindow<SandboxImporter>();
		wnd.titleContent = new GUIContent("Import Sandbox Map");
	}

	public void CreateGUI()
	{
		TextField filePathField = new TextField("Path to the sandbox map");
		filePathField.RegisterValueChangedCallback(e => {
			pathToSandboxMap = e.newValue;
		});
		filePathField.SetValueWithoutNotify(pathToSandboxMap);
		rootVisualElement.Add(filePathField);

		Button openFile = new Button();
		openFile.text = "Open Sandbox Map";
		openFile.clicked += () =>
		{
			string newPath = EditorUtility.OpenFilePanel("Open PITR File", Application.dataPath, "pitr");
			if (!string.IsNullOrEmpty(newPath))
			{
				pathToSandboxMap = newPath;
				filePathField.SetValueWithoutNotify(newPath);
			}
		};
		rootVisualElement.Add(openFile);

		VisualElement topSpace = new VisualElement();
		topSpace.style.height = new StyleLength(10);
		rootVisualElement.Add(topSpace);

		rootVisualElement.Add(new Label("Options"));

		sSubdivideFaces = true;
		subdivideFaces = new Toggle("Subdivide faces");
		subdivideFaces.value = sSubdivideFaces;
		subdivideFaces.RegisterValueChangedCallback((e) => sSubdivideFaces = e.newValue);
		rootVisualElement.Add(subdivideFaces);

		VisualElement bottomSpace = new VisualElement();
		bottomSpace.style.height = new StyleLength(10);
		rootVisualElement.Add(bottomSpace);

		Button importButton = new Button();
		importButton.text = "Import Sandbox Map";
		importButton.clicked += ImportMap;
		rootVisualElement.Add(importButton);
	}

	private Dictionary<string, SpawnableObject> registeredObjects = new Dictionary<string, SpawnableObject>();

	private void RegisterObjects(SpawnableObject[] objs)
	{
		foreach (SpawnableObject spawnableObject in objs)
		{
			if (!string.IsNullOrEmpty(spawnableObject.identifier) && !this.registeredObjects.ContainsKey(spawnableObject.identifier))
			{
				this.registeredObjects.Add(spawnableObject.identifier, spawnableObject);
			}
		}
	}

	public void RebuildObjectList()
	{
		if (this.registeredObjects == null)
		{
			this.registeredObjects = new Dictionary<string, SpawnableObject>();
		}
		this.registeredObjects.Clear();
		var objects = AssetDatabase.LoadAssetAtPath<SpawnableObjectsDatabase>("Assets/ULTRAKILL Addressables/Data/Sandbox/Spawnable Objects Database.asset");
		this.RegisterObjects(objects.objects);
		this.RegisterObjects(objects.enemies);
		this.RegisterObjects(objects.sandboxTools);
		this.RegisterObjects(objects.sandboxObjects);
		this.RegisterObjects(objects.specialSandbox);
	}

	// SandboxSaver
	// Token: 0x06001202 RID: 4610 RVA: 0x0009B5F8 File Offset: 0x000997F8
	private void ApplyData(GameObject go, SavedAlterData[] data)
	{
		if (data == null)
		{
			return;
		}
		IAlter[] componentsInChildren = go.GetComponentsInChildren<IAlter>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			IAlter alterComponent = componentsInChildren[i];
			if (alterComponent.alterKey != null)
			{
				if (!(from d in data
					  select d.Key).Contains(alterComponent.alterKey))
				{
				}
				else
				{
					SavedAlterData savedAlterData = data.FirstOrDefault((SavedAlterData d) => d.Key == alterComponent.alterKey);
					if (savedAlterData != null)
					{
						SavedAlterOption[] options2 = savedAlterData.Options;
						int j = 0;
						while (j < options2.Length)
						{
							SavedAlterOption options = options2[j];
							IAlterOptions<bool> alterOptions;
							if (options.BoolValue == null || (alterOptions = (alterComponent as IAlterOptions<bool>)) == null)
							{
								goto IL_140;
							}
							AlterOption<bool> alterOption = alterOptions.options.FirstOrDefault((AlterOption<bool> o) => o.key == options.Key);
							if (alterOption != null)
							{
								Action<bool> callback = alterOption.callback;
								if (callback == null)
								{
									goto IL_140;
								}
								callback(options.BoolValue.Value);
								goto IL_140;
							}
						IL_20D:
							j++;
							continue;
						IL_140:
							IAlterOptions<float> alterOptions2;
							if (options.FloatValue != null && (alterOptions2 = (alterComponent as IAlterOptions<float>)) != null)
							{
								AlterOption<float> alterOption2 = alterOptions2.options.FirstOrDefault((AlterOption<float> o) => o.key == options.Key);
								if (alterOption2 == null)
								{
									goto IL_20D;
								}
								Action<float> callback2 = alterOption2.callback;
								if (callback2 != null)
								{
									callback2(options.FloatValue.Value);
								}
							}
							IAlterOptions<int> alterOptions3;
							if (options.IntValue == null || (alterOptions3 = (alterComponent as IAlterOptions<int>)) == null)
							{
								goto IL_20D;
							}
							AlterOption<int> alterOption3 = alterOptions3.options.FirstOrDefault((AlterOption<int> o) => o.key == options.Key);
							if (alterOption3 == null)
							{
								goto IL_20D;
							}
							Action<int> callback3 = alterOption3.callback;
							if (callback3 == null)
							{
								goto IL_20D;
							}
							callback3(options.IntValue.Value);
							goto IL_20D;
						}
					}
				}
			}
		}
	}

	private static MethodInfo _proBuilderize = null;
	private static MethodInfo proBuilderize
	{
		get
		{
			if (_proBuilderize == null)
			{
				Type probuilderizeClass = Type.GetType("UnityEditor.ProBuilder.Actions.ProBuilderize, Unity.ProBuilder.Editor");
				_proBuilderize = probuilderizeClass.GetMethod("DoProBuilderize", new Type[] { typeof(IEnumerable<MeshFilter>), typeof(bool) });
			}

			return _proBuilderize;
		}
	}

	public static ProBuilderMesh Probuilderize(MeshFilter mf)
	{
		if (mf.sharedMesh == null)
			return null;

		GameObject go = mf.gameObject;
		Mesh sourceMesh = mf.sharedMesh;
		Material[] sourceMaterials = go.GetComponent<MeshRenderer>()?.sharedMaterials;

		MeshImportSettings settings = new MeshImportSettings()
		{
			quads = true,
			smoothing = false,
			smoothingAngle = 1f
		};

		ProBuilderMesh destination = null;
		try
		{
			destination = Undo.AddComponent<ProBuilderMesh>(go);
			var meshImporter = new MeshImporter(sourceMesh, sourceMaterials, destination);
			meshImporter.Import(settings);

			destination.ToMesh();
			destination.Refresh();
			destination.Optimize();
		}
		catch (System.Exception e)
		{
			Debug.LogWarning("Failed ProBuilderizing: " + go.name + "\n" + e.ToString());
		}

		ProBuilderEditor.Refresh();
		return destination;
	}

	private static Mesh MergeSimilarFaces(Mesh mesh)
	{
		float normalThreshold = 0.99f;

		// Get mesh data
		Vector3[] vertices = mesh.vertices;
		Vector3[] normals = mesh.normals;
		Vector2[] uv = mesh.uv;
		int[] triangles = mesh.triangles;

		// Create a dictionary to store merged vertices
		Dictionary<Vector3, int> mergedVertices = new Dictionary<Vector3, int>();

		// New triangle list after merging
		List<int> newTriangles = new List<int>();
		List<Vector2> newUVs = new List<Vector2>(); // List to hold new UV coordinates

		// Process each triangle
		for (int i = 0; i < triangles.Length; i += 3)
		{
			// Get vertices of the current triangle
			Vector3 v0 = vertices[triangles[i]];
			Vector3 v1 = vertices[triangles[i + 1]];
			Vector3 v2 = vertices[triangles[i + 2]];

			// Calculate average normal of the triangle
			Vector3 averageNormal = (normals[triangles[i]] + normals[triangles[i + 1]] + normals[triangles[i + 2]]) / 3f;

			// Check if the normals are similar enough
			if (Vector3.Dot(normals[triangles[i]], averageNormal) >= normalThreshold &&
				Vector3.Dot(normals[triangles[i + 1]], averageNormal) >= normalThreshold &&
				Vector3.Dot(normals[triangles[i + 2]], averageNormal) >= normalThreshold)
			{
				// Check if vertices are already merged
				int index0, index1, index2;
				if (!mergedVertices.TryGetValue(v0, out index0))
				{
					index0 = mergedVertices.Count;
					mergedVertices.Add(v0, index0);
					newUVs.Add(uv[triangles[i]]); // Add UV for new vertex
				}
				if (!mergedVertices.TryGetValue(v1, out index1))
				{
					index1 = mergedVertices.Count;
					mergedVertices.Add(v1, index1);
					newUVs.Add(uv[triangles[i + 1]]); // Add UV for new vertex
				}
				if (!mergedVertices.TryGetValue(v2, out index2))
				{
					index2 = mergedVertices.Count;
					mergedVertices.Add(v2, index2);
					newUVs.Add(uv[triangles[i + 2]]); // Add UV for new vertex
				}

				// Add merged vertices to new triangle list
				newTriangles.Add(index0);
				newTriangles.Add(index1);
				newTriangles.Add(index2);
			}
			else
			{
				// Add original triangle to new triangle list
				newTriangles.Add(triangles[i]);
				newTriangles.Add(triangles[i + 1]);
				newTriangles.Add(triangles[i + 2]);
			}
		}

		// Create a new mesh with merged vertices
		Mesh mergedMesh = new Mesh();
		mergedMesh.vertices = mergedVertices.Keys.ToArray();
		mergedMesh.triangles = newTriangles.ToArray();
		mergedMesh.uv = newUVs.ToArray(); // Use the new UV coordinates
		mergedMesh.RecalculateNormals(); // Recalculate normals for the merged mesh

		// Assign the merged mesh back to the MeshFilter component
		return mergedMesh;
	}

	public static GameObject CreateFinalBlock(SpawnableObject proceduralTemplate, Vector3 position, Vector3 size, bool liquid = false)
	{
		GameObject gameObject = Object.Instantiate<GameObject>(proceduralTemplate.gameObject);
		gameObject.transform.position = position;
		BrushBlock component = gameObject.GetComponent<BrushBlock>();
		component.sourceObject = proceduralTemplate;
		component.DataSize = size;
		Mesh mesh = SandboxUtils.GenerateProceduralMesh(size, sSubdivideFaces ? liquid : true);
		SandboxProp component2 = gameObject.GetComponent<SandboxProp>();
		component2.sourceObject = proceduralTemplate;
		gameObject.GetComponent<MeshFilter>().mesh = mesh;
		BoxCollider boxCollider = component.OverrideCollider ? component.OverrideCollider : gameObject.GetComponent<BoxCollider>();
		boxCollider.size = size;
		boxCollider.center = boxCollider.size / 2f;
		if (liquid)
		{
			GameObject gameObject2 = new GameObject("LiquidTrigger");
			gameObject2.layer = LayerMask.NameToLayer("SandboxGrabbable");
			gameObject2.transform.parent = gameObject.transform;
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.localRotation = Quaternion.identity;
			gameObject2.transform.localScale = Vector3.one;
			BoxCollider boxCollider2 = gameObject2.AddComponent<BoxCollider>();
			boxCollider2.isTrigger = true;
			boxCollider2.size = size;
			boxCollider2.center = boxCollider.size / 2f;
			component.WaterTrigger = boxCollider2;
			gameObject2.AddComponent<SandboxPropPart>().parent = component2;
		}

		ProBuilderMesh pbMesh = Probuilderize(gameObject.GetComponent<MeshFilter>());
		foreach (var face in pbMesh.faces)
		{
			face.manualUV = false;
			face.uv = new AutoUnwrapSettings()
			{
				anchor = AutoUnwrapSettings.Anchor.LowerLeft,
				fill = AutoUnwrapSettings.Fill.Tile,
				scale = new Vector2(0.25f, 0.25f)
			};
		}
		pbMesh.Refresh(RefreshMask.UV);

		return gameObject;
	}

	private void RecreateBlock(SavedBlock block)
	{
		SpawnableObject spawnableObject;
		if (!this.registeredObjects.TryGetValue(block.ObjectIdentifier, out spawnableObject))
		{
			return;
		}
		GameObject gameObject = CreateFinalBlock(spawnableObject, block.Position.ToVector3(), block.BlockSize.ToVector3(), spawnableObject.isWater);
		gameObject.transform.rotation = block.Rotation.ToQuaternion();
		SandboxProp component = gameObject.GetComponent<SandboxProp>();
		component.sourceObject = this.registeredObjects[block.ObjectIdentifier];
		if (block.Kinematic)
		{
			if (component.TryGetComponent(out Rigidbody rb))
			{
				rb.isKinematic = true;
				rb.velocity = Vector3.zero;
			}

			component.gameObject.isStatic = true;
		}
		else
		{
			if (component.TryGetComponent(out Rigidbody rb))
				rb.isKinematic = false;

			component.gameObject.isStatic = false;
		}
		component.disallowManipulation = block.DisallowManipulation;
		component.disallowFreezing = block.DisallowFreezing;
		this.ApplyData(gameObject, block.Data);
		DestroyImmediate(component);
	}

	public SandboxEnemy RecreateEnemy(SavedGeneric genericObject, bool newSizing)
	{
		SpawnableObject spawnableObject;
		if (!this.registeredObjects.TryGetValue(genericObject.ObjectIdentifier, out spawnableObject))
		{
			return null;
		}
		// GameObject gameObject = Object.Instantiate<GameObject>(spawnableObject.gameObject);
		GameObject gameObject = (GameObject)PrefabUtility.InstantiatePrefab(spawnableObject.gameObject);
		gameObject.transform.position = genericObject.Position.ToVector3();
		if (!newSizing)
		{
			gameObject.transform.localScale = genericObject.Scale.ToVector3();
		}
		KeepInBounds keepInBounds;
		if (gameObject.TryGetComponent<KeepInBounds>(out keepInBounds))
		{
			keepInBounds.ForceApproveNewPosition();
		}
		SandboxEnemy sandboxEnemy = gameObject.AddComponent<SandboxEnemy>();
		sandboxEnemy.enemyId = gameObject.GetComponent<EnemyIdentifier>();
		if (sandboxEnemy.enemyId == null)
			sandboxEnemy.enemyId = gameObject.GetComponentInChildren<EnemyIdentifier>(true);
		sandboxEnemy.sourceObject = this.registeredObjects[genericObject.ObjectIdentifier];
		sandboxEnemy.enemyId.checkingSpawnStatus = false;
		sandboxEnemy.RestoreRadiance(((SavedEnemy)genericObject).Radiance);
		SavedPhysical savedPhysical;
		if ((savedPhysical = (genericObject as SavedPhysical)) != null && savedPhysical.Kinematic)
		{
			sandboxEnemy.Pause(true);
		}
		if (newSizing)
		{
			sandboxEnemy.SetSize(genericObject.Scale.ToVector3());
		}
		sandboxEnemy.disallowManipulation = genericObject.DisallowManipulation;
		sandboxEnemy.disallowFreezing = genericObject.DisallowFreezing;
		this.ApplyData(gameObject, genericObject.Data);
		// MonoSingleton<SandboxNavmesh>.Instance.EnsurePositionWithinBounds(gameObject.transform.position);
		DestroyImmediate(sandboxEnemy);
		return sandboxEnemy;
	}

	private void RecreateProp(SavedProp prop, bool newSizing)
	{
		SpawnableObject spawnableObject;
		if (!this.registeredObjects.TryGetValue(prop.ObjectIdentifier, out spawnableObject))
		{
			return;
		}
		// GameObject gameObject = Object.Instantiate<GameObject>(spawnableObject.gameObject);
		GameObject gameObject = (GameObject)PrefabUtility.InstantiatePrefab(spawnableObject.gameObject);
		gameObject.transform.SetPositionAndRotation(prop.Position.ToVector3(), prop.Rotation.ToQuaternion());
		if (!newSizing)
		{
			gameObject.transform.localScale = prop.Scale.ToVector3();
		}
		SandboxProp component = gameObject.GetComponent<SandboxProp>();
		component.sourceObject = this.registeredObjects[prop.ObjectIdentifier];
		if (newSizing)
		{
			component.SetSize(prop.Scale.ToVector3());
		}
		if (prop.Kinematic)
		{
			if (component.TryGetComponent(out Rigidbody rb))
			{
				rb.isKinematic = true;
				rb.velocity = Vector3.zero;
			}

			component.gameObject.isStatic = true;
		}
		else
		{
			if (component.TryGetComponent(out Rigidbody rb))
				rb.isKinematic = false;

			component.gameObject.isStatic = false;
		}
		component.disallowManipulation = prop.DisallowManipulation;
		component.disallowFreezing = prop.DisallowFreezing;
		this.ApplyData(gameObject, prop.Data);
		DestroyImmediate(component);
	}

	public void ImportMap()
	{
		if (!File.Exists(pathToSandboxMap))
		{
			EditorUtility.DisplayDialog("Error", "Sandbox file not found in the given path", "Close");
			return;
		}

		SandboxSaveData sandboxSaveData;
		try
		{
			sandboxSaveData = JsonConvert.DeserializeObject<SandboxSaveData>(File.ReadAllText(pathToSandboxMap));
		}
		catch (Exception e)
		{
			EditorUtility.DisplayDialog("Error", $"Could not process the sandbox file. Corrupt or invalid file.\n\n{e}", "Close");
			return;
		}

		string savePath = EditorUtility.SaveFilePanel("Save Scene", Application.dataPath, Path.GetFileNameWithoutExtension(pathToSandboxMap), "unity");
		if (string.IsNullOrEmpty(savePath))
			return;

		if (!Path.GetFullPath(savePath).StartsWith(Path.GetFullPath(Application.dataPath)))
		{
			EditorUtility.DisplayDialog("Error", "Scene must be saved somewhere inside the assets folder", "Close");
			return;
		}

		if (File.Exists(savePath))
		{
			EditorUtility.DisplayDialog("Error", "Output file already exists", "Close");
			return;
		}

		if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
		{
			EditorUtility.DisplayDialog("Error", "Current scene must be saved before importing", "Close");
			return;
		}

		Scene sandboxScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
		SceneManager.SetActiveScene(sandboxScene);

		RebuildObjectList();
		Vector3? vector = null;
		Vector3 position = Vector3.zero;
		foreach (SavedProp savedProp in sandboxSaveData.Props)
		{
			this.RecreateProp(savedProp, sandboxSaveData.SaveVersion > 1);
			if (!(savedProp.ObjectIdentifier != "ultrakill.spawn-point"))
			{
				if (vector == null)
				{
					vector = new Vector3?(savedProp.Position.ToVector3());
				}
				else if (Vector3.Distance(position, savedProp.Position.ToVector3()) < Vector3.Distance(position, vector.Value))
				{
					vector = new Vector3?(savedProp.Position.ToVector3());
				}
			}
		}
		foreach (SavedBlock block in sandboxSaveData.Blocks)
		{
			this.RecreateBlock(block);
		}
		foreach (SavedEnemy genericObject in sandboxSaveData.Enemies)
		{
			SandboxEnemy sandboxEnemy = this.RecreateEnemy(genericObject, sandboxSaveData.SaveVersion > 1);
			// sandboxEnemy.Pause(false);
		}

		// Save the scene
		EditorSceneManager.SaveScene(sandboxScene, savePath);
		AssetDatabase.Refresh();
		EditorUtility.DisplayDialog("Success", $"Successfully imported sandbox map!\n\nBlock count: {sandboxSaveData.Blocks.Length}\nEnemy count: {sandboxSaveData.Enemies.Length}\nProp count: {sandboxSaveData.Props.Length}", "Close");
	}
}
