using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PhysicalSimulator : EditorWindow
{
    bool simulationActive = false;  // If phisics simulation is active
    public GameObject[] targetGameObjects;  // array of objecs that receive phisics

    // Add menu named "Physical Simulator" to the Window menu
    [MenuItem("Automation/Physical Simulator")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        PhysicalSimulator window = (PhysicalSimulator)EditorWindow.GetWindow(typeof(PhysicalSimulator));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Simulation", EditorStyles.boldLabel);  // Header
        GUILayout.Label(simulationActive ? "Ativada " : "Desativada", EditorStyles.label);  // Show if simulation is active or inactive

        // Show list of game objects
        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty goProperty = so.FindProperty("targetGameObjects");
        EditorGUILayout.PropertyField(goProperty, true); // True means show children
        so.ApplyModifiedProperties(); // Remember to apply modified properties


        // Horisontal layout for run and stop buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Run"))
            StartPhysics();

        if (GUILayout.Button("Stop"))
            StopPhysics();

        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Start physics in editor
    /// </summary>
    private void StartPhysics()
    {
        if (simulationActive)   // if already active, return
            return;

        AddMeshAndRigidBody();  // Add meshs and rigidBody to object list
        simulationActive = true;    // set simulation bool to true
        Physics.autoSimulation = false; // Set Autosimulation to true
    }

    /// <summary>
    /// Stop Physics in edit mode
    /// </summary>
    private void StopPhysics()
    {
        if (!simulationActive)  // if already inactive, return
            return;

        RemoveMeshAndRigidBody();   // Remove colliders and rigidBody from objects
        simulationActive = false;   // Set simulation bool to false
        Physics.autoSimulation = true;  // Set Autosimulation to true
    }

    private void Update()
    {
        // if Simulation bool is active, simulate physics
        if(simulationActive)
            Physics.Simulate(Time.fixedDeltaTime);
    }

    /// <summary>
    /// Add mesh collider and rigidbody to target objecs in list
    /// </summary>
    void AddMeshAndRigidBody()
    {
        foreach (var item in targetGameObjects)
        {
            item.AddComponent<Rigidbody>(); // Add rigid body
            item.AddComponent<MeshCollider>().convex = true;    // Add mesh colider and set convex to false
        }
    }

    /// <summary>
    /// Remove Mesh Collider and Rigid body from target objecs in list
    /// </summary>
    void RemoveMeshAndRigidBody()
    {
        foreach (var item in targetGameObjects)
        {
            DestroyImmediate(item.GetComponent<Rigidbody>());
            DestroyImmediate(item.GetComponent<MeshCollider>());
        }
    }

}
