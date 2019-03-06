using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ScriptableObject class to create different types of animals
// with different speeds and points
[CreateAssetMenu(fileName = "New animal", menuName = "Dev Test Task/Create new animal", order = 1)]
public class AnimalType : ScriptableObject
{
    public GameObject Prefab;
    public int Points = 1;
    public float Speed = 1f;
    public float PatrolSpeed = 1f;
}
