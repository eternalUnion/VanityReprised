using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class NavMeshBaker
{
    static IEnumerable<GameObject> GetAllGameobjects(GameObject o)
    {
        yield return o;
        foreach (Transform t in o.transform)
            foreach (var child in GetAllGameobjects(t.gameObject))
                yield return child;
    }

	static IEnumerable<GameObject> GetAllSceneObjects(Scene s)
    {
        foreach (var rootObj in s.GetRootGameObjects())
            foreach (var obj in GetAllGameobjects(rootObj))
                yield return obj;
    }

    static List<GameObject> GetParentObjectsToActivate(GameObject parent)
    {
        List<GameObject> toActivate = new List<GameObject>();
        while (parent != null)
        {
            if (!parent.activeSelf)
                toActivate.Add(parent);

            if (parent.transform.parent != null)
                parent = parent.transform.parent.gameObject;
            else
                parent = null;
        }

        return toActivate;
    }

	[MenuItem("Tools/Nav Mesh Baker")]
    static void Bake()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        List<GameObject> objToDeactivate = new List<GameObject>();
        try
        {
            foreach (var obj in GetAllSceneObjects(currentScene))
            {
                var flags = GameObjectUtility.GetStaticEditorFlags(obj);
                if ((flags & StaticEditorFlags.NavigationStatic) == 0)
                    continue;

                if (!obj.activeSelf)
                {
                    objToDeactivate.Add(obj);
                    obj.SetActive(true);
                }

                if (obj.transform.parent == null)
                    continue;

                foreach (var parent in GetParentObjectsToActivate(obj.transform.parent.gameObject))
                {
                    objToDeactivate.Add(parent);
                    parent.SetActive(true);
                }
            }

            UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
		}
        finally
        {
            foreach (var obj in objToDeactivate)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }
    }
}
