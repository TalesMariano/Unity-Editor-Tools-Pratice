using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ScriptCleaner : MonoBehaviour
{
    [MenuItem("GameObject/Automation/Remove scripts", false, 1)]
    static void RemoveScriptsObject(MenuCommand menuCommand)
    {
        //Get selected object
        GameObject selectObject = Selection.activeTransform.gameObject;

        // Get all monobehaviors
        MonoBehaviour[] allMonos = GetArrayMonoBehaviour(selectObject);

        // Delete all monobehavior
        for (int i = 0; i < allMonos.Length; i++)
        {
            DestroyImmediate(allMonos[i]);
        }

        // Show Message
        EditorUtility.DisplayDialog("Scripts removed from object and childs", "Scripts Removed. \n This cannot be undone", "Ok");

        // Set object dirty
        EditorUtility.SetDirty(selectObject);
            
    }

    /// <summary>
    /// Get all Monobehaviors from objects and childs
    /// </summary>
    /// <param name="gameObject">GameObject to get MonoBehaviours</param>
    /// <returns>MonoBehaviours from object and childs</returns>
    static MonoBehaviour[] GetArrayMonoBehaviour(GameObject gameObject)
    {
        MonoBehaviour[] result = gameObject.GetComponentsInChildren<MonoBehaviour>();
        return result;
    }
}
