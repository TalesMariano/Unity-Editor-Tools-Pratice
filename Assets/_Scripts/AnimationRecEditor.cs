using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class AnimationRecEditor : EditorWindow
{
    static Transform target;    // Target object to record animation
    static Transform[] targetAndChilds; // transform list of target object and all its childs

    // Folder Path
    static string savePath;     // Path to save animation
    static string animationName;    // Animation clip file name


    // Keys
    const string keyStartRec = "_i";    // Key shortcut to start recording
    const string keyEndRec = "_o";      // Key shortcut to stop recording
    const string keyDeleteRec = "_p";   // Key shortcut to delete recording

    static float timeStep = 0.25f;      // How often its is recorded


    // Internal vars
    static bool recording;
    static double nextTime;
    static int currentStep;
    static Dictionary<string, List<Keyframe>>[] dictKeysArray;
    static AnimationClip animClip;

    // Add menu named "My Window" to the Window menu
    [MenuItem("AnimationRec/Settings")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        AnimationRecEditor window = (AnimationRecEditor)EditorWindow.GetWindow(typeof(AnimationRecEditor));
        window.Show();
    }

    void OnGUI()
    {

        GUILayout.Label("Animation Recorder Settings", EditorStyles.boldLabel); // Header



        // Show shotcut keys
        EditorGUILayout.LabelField("Key Start Rec", keyStartRec[1].ToString().ToUpper(), EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Key End Rec", keyEndRec[1].ToString().ToUpper(), EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Key Delete Rec", keyDeleteRec[1].ToString().ToUpper(), EditorStyles.boldLabel);


        var targetObject = EditorGUILayout.ObjectField("Object to Record", target, typeof(Transform), true);    // Object to Record

        // When there is a new target selected
        if (targetObject != null && target != targetObject as Transform)
        {
            target = targetObject as Transform;
            targetAndChilds = target.GetComponentsInChildren<Transform>();
            animationName = GenerateName(target);

        }

        target = targetObject as Transform; // appli editor field to variable


        // Save Path
        if (savePath == null)   // if path is null, get path from active scene
            savePath = GetScenePath(SceneManager.GetActiveScene());

        savePath = EditorGUILayout.TextField("Save Path", savePath);    // Save Path text field

        /*  // Old Save path, with button to load path
         *  // Path loaded is not conpatible with internal unity path
        EditorGUILayout.BeginHorizontal();
         Get defauth value
        if (savePath == null)   // if path is null, get path from active scene
            savePath = GetScenePath(SceneManager.GetActiveScene());

        savePath = EditorGUILayout.TextField("Save Path", savePath);    // Save Path text field
        if (GUILayout.Button("...", GUILayout.Width(20)))
        {
        savePath = EditorUtility.OpenFolderPanel("Select Path", savePath, savePath);
        }
        EditorGUILayout.EndHorizontal();
        */

        // Generate animation name
        if (animationName == null && target != null)
            animationName = GenerateName(target);   //animationName = target.name + "_Animation";



        animationName = EditorGUILayout.TextField("Animation Clip Name", animationName);

        timeStep = EditorGUILayout.FloatField("Rec Time Step", timeStep);

    }

    private void Update()
    {
        // If its recording and next step time has reached
        if (recording && nextTime < EditorApplication.timeSinceStartup)
        {
            nextTime = EditorApplication.timeSinceStartup + timeStep;   // set next step time
            RecordStep();   
        }
    }

    /// <summary>
    /// Record Single Step
    /// </summary>
    void RecordStep()
    {
        AddStepDictionary(currentStep * timeStep, targetAndChilds); // save transforms values to dictionary
        currentStep++;
    }


    #region RecAnim

    /// <summary>
    /// Generate new dictionary array prepared to record "Transform" animations keys of multiple transforms
    /// </summary>
    /// <param name="numTransforms"> dictionary array lenght, or Number of transforms dictionary it will record </param>
    static void NewDictionary(int numTransforms)
    {
        dictKeysArray = new Dictionary<string, List<Keyframe>>[numTransforms]; // Chreate new array

        for (int i = 0; i < dictKeysArray.Length; i++)
        {
            dictKeysArray[i] = new Dictionary<string, List<Keyframe>>();    // Instantiate dictionary

            // Add keys based on transforms interna variables
            dictKeysArray[i].Add("localPosition.x", new List<Keyframe>());
            dictKeysArray[i].Add("localPosition.y", new List<Keyframe>());
            dictKeysArray[i].Add("localPosition.z", new List<Keyframe>());

            dictKeysArray[i].Add("localScale.x", new List<Keyframe>());
            dictKeysArray[i].Add("localScale.y", new List<Keyframe>());
            dictKeysArray[i].Add("localScale.z", new List<Keyframe>());

            dictKeysArray[i].Add("localRotation.x", new List<Keyframe>());
            dictKeysArray[i].Add("localRotation.y", new List<Keyframe>());
            dictKeysArray[i].Add("localRotation.z", new List<Keyframe>());
            dictKeysArray[i].Add("localRotation.w", new List<Keyframe>());
        }
    }

    /// <summary>
    /// Add a new keyframe to all transforms
    /// </summary>
    /// <param name="frameTime"></param>
    /// <param name="transforms"></param>
    void AddStepDictionary(float frameTime, Transform[] transforms)
    {
        for (int i = 0; i < transforms.Length; i++)
        {
            dictKeysArray[i]["localPosition.x"].Add(new Keyframe(frameTime, transforms[i].localPosition.x));
            dictKeysArray[i]["localPosition.y"].Add(new Keyframe(frameTime, transforms[i].localPosition.y));
            dictKeysArray[i]["localPosition.z"].Add(new Keyframe(frameTime, transforms[i].localPosition.z));

            dictKeysArray[i]["localScale.x"].Add(new Keyframe(frameTime, transforms[i].localScale.x));
            dictKeysArray[i]["localScale.y"].Add(new Keyframe(frameTime, transforms[i].localScale.y));
            dictKeysArray[i]["localScale.z"].Add(new Keyframe(frameTime, transforms[i].localScale.z));

            dictKeysArray[i]["localRotation.x"].Add(new Keyframe(frameTime, transforms[i].localRotation.x));
            dictKeysArray[i]["localRotation.y"].Add(new Keyframe(frameTime, transforms[i].localRotation.y));
            dictKeysArray[i]["localRotation.z"].Add(new Keyframe(frameTime, transforms[i].localRotation.z));
            dictKeysArray[i]["localRotation.w"].Add(new Keyframe(frameTime, transforms[i].localRotation.w));
        }
    }

    /// <summary>
    /// Transform dictionaty array in Animation Curve, adn apply to animation Clip
    /// </summary>
    static void AddCurveToAnimClip()
    {
        for (int i = 0; i < dictKeysArray.Length; i++)
        {
            Dictionary<string, List<Keyframe>> dict = dictKeysArray[i];
            foreach (var item in dict)
            {
                string transformName = GetPathRelativeParent(targetAndChilds[i], target);

                animClip.SetCurve(transformName, typeof(Transform), item.Key, new AnimationCurve(item.Value.ToArray()));
            }
        }

    }

    /// <summary>
    /// Get path relative to object and Big parent
    /// Specific for animation clip parent and child paths
    /// </summary>
    /// <param name="targetTransform"></param>
    /// <param name="animationTransform"></param>
    /// <returns></returns>
    static string GetPathRelativeParent(Transform targetTransform, Transform animationTransform)
    {
        string path = "";

        // If target transform is animation transform, path is ""
        if (targetTransform == animationTransform)
            return path;

        Transform t = targetTransform;

        while (t != animationTransform && t.parent != null)
        {
            path = t.name + path;
            t = t.parent;

            if (t == animationTransform)
                return path;

            path = "/" + path;
        }

        return path;
    }

    #endregion

    /// <summary>
    /// Item Menu
    /// Start Recording
    /// </summary>
    [MenuItem("AnimationRec/StartRec " + keyStartRec)]
    static void StartRec()
    {
        // If is already recording, or there is no target, end method
        if (recording || target == null)    
            return;

        Debug.Log("Start Recording");

        // Reset Vars
        animClip = new AnimationClip();
        currentStep = 0;
        NewDictionary(targetAndChilds.Length);

        recording = true;

        nextTime = EditorApplication.timeSinceStartup + timeStep; // set next step time
    }

    /// <summary>
    /// Item Menu
    /// End Recording and Save Animation File File
    /// </summary>
    [MenuItem("AnimationRec/EndRec " + keyEndRec)]
    static void EndRec()
    {
        if (!recording)
            return;

        recording = false;

        AddCurveToAnimClip();
        SaveAnimFile();
        Debug.Log("End Recording");
    }

    /// <summary>
    /// Delete Animation Clip
    /// </summary>
    [MenuItem("AnimationRec/DeleteRec " + keyDeleteRec)]
    static void DeleteRec()
    {
        string deletePath = savePath + animationName + ".anim";

        if (AssetDatabase.LoadAssetAtPath(deletePath, typeof(AnimationClip)) != null)  // Check if animation file exists
        {
            AssetDatabase.DeleteAsset(deletePath);
            Debug.Log(deletePath + " Deleted");
        }
        else
        {
            Debug.Log("File to be deleted not found");
        }
    }

    /// <summary>
    /// Save Animation Clip file on Assets
    /// </summary>
    static void SaveAnimFile()
    {
        AnimationClip clip = animClip;
        clip.legacy = true;
        AssetDatabase.CreateAsset(clip, ValidadeNameIncrementing(animationName, savePath)); //  // savePath + animationName + ".anim"
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// Generate Animation name based on transform object
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    static string GenerateName(Transform obj)
    {
        return obj.name +"_Animation";
    }

    /// <summary>
    /// Return a valid name, not existing already
    /// </summary>
    /// <param name="_name"></param>
    /// <param name="_savePath"></param>
    /// <returns></returns>
    static string ValidadeName(string _name, string _savePath)
    {
        int attempts = 0;
        string resultName = _name;

        //fix path name
        if (_savePath[_savePath.Length - 1] != '/')
            _savePath += "/";

        // Add an incremental number on the end of name until there is no file with same name 
        while (AssetDatabase.LoadAssetAtPath(_savePath + resultName + ".anim", typeof(AnimationClip)) == null)
        {
            attempts++;
            resultName = _name + attempts;
        }

        return _savePath + resultName + ".anim";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_name"></param>
    /// <param name="_savePath"></param>
    /// <returns>String: "SavePath" / "AnimName" .anim </returns>
    static string ValidadeNameIncrementing(string _name, string _savePath)
    {
        int attempts = 0;
        string resultName = _name;

        //fix path name if it doesn't end in "/"
        if (_savePath[_savePath.Length - 1] != '/')
            _savePath += "/";

        // Add an incremental number on the end of name until there is no file with same name 
        while (AssetDatabase.LoadAssetAtPath(_savePath + resultName + ".anim", typeof(AnimationClip)) != null)
        {
            attempts++;
            resultName = _name + " " +  attempts;

            //Debug.Log(_savePath + resultName + ".anim");
        }

        return _savePath + resultName + ".anim";
    }

    /// <summary>
    /// Get scene folder 
    /// </summary>
    /// <param name="scene">Current open scene</param>
    /// <returns>Scene folder path, starting in "Assets"</returns>
    string GetScenePath(Scene scene)
    {
        string scenePath = scene.path;
        string result = "";

        // Remove .../Scene.unity
        string[] s = scenePath.Split('/');

        if (s.Length > 1)
        {
            for (int i = 0; i < s.Length - 1; i++)
            {
                result += s[i] + "/";
            }
        }
        return result;
    }
}
