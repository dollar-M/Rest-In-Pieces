using System.Collections.Generic;
using UnityEngine;

public class ObjectProperties : MonoBehaviour
{
    // This stores all objects that can be phased through
    public static List<ObjectProperties> phaseableObjects = new List<ObjectProperties>();

    [Header("Object properties")]
    public bool isPhaseable = false;

    void OnEnable()
    {
        // If this object is phaseable, add it to the list
        if (isPhaseable && !phaseableObjects.Contains(this))
        {
            phaseableObjects.Add(this);
        }
    }

    void OnDisable()
    {
        // If this object gets disabled or destroyed, remove it from the list
        if (phaseableObjects.Contains(this))
        {
            phaseableObjects.Remove(this);
        }
    }
}