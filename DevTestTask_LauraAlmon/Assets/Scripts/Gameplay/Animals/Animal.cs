using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

// Realization of Object Pool pattern by using Zenject Memory Pools
public class Animal : Character, IPoolable<IMemoryPool>, IDisposable
{
    private AnimalType[] m_animalTypes;
    private GameObject m_bound;
    private float m_boundMultiplier;

    private AnimalType m_animalType;

    private IMemoryPool m_pool;

    private List<Animal> m_animalsInTheGroup;

    private Herdsman m_herdsman;

    private Vector2 m_rnd;

    [Inject]
    private void Construct()
    {
        m_animalTypes = m_settings.AnimalTypes;
        m_bound = m_settings.Bound;
        m_boundMultiplier = m_settings.BoundMultiplier;
        m_animalsInTheGroup = m_gameController.AnimalsInTheGroup;
        m_rnd = UnityEngine.Random.insideUnitCircle * m_settings.DistanceBetweenUnits;
    }

    public override void GetTargetPosition()
    {
        // Animals in the group do not lump together
        // They are randomly located inside a circle around the Herdsman
        if (m_animalsInTheGroup.Contains(this))
        {
            m_targetPosition = m_gameController.Player.transform.position;
            m_targetPosition += m_rnd;
            m_speed = m_animalType.Speed;
        }
        else if (GetComponent<Rigidbody2D>().velocity == Vector2.zero)
        {
            // Animals waiting on the field move around to random places
            m_targetPosition = GetRandomPosition();
            m_speed = m_animalType.PatrolSpeed;
        }
    }

    private void Update()
    {
        Move();
    }

    private void Reset()
    {
        // The animal becomes a new random one
        int rnd = UnityEngine.Random.Range(0, m_animalTypes.Length);
        m_animalType = m_animalTypes[rnd];
        SetType();

        // We get a new random position
        transform.position = GetRandomPosition();
    }

    public float Speed
    {
        get { return m_speed; }
        set { m_speed = value; }
    }

    public int Points
    {
        get { return m_animalType.Points; }
    }

    public AnimalType Type
    {
        get { return m_animalType; }
    }

    private void SetType()
    {
        GetComponent<SpriteRenderer>().sprite = m_animalType.Prefab.GetComponent<SpriteRenderer>().sprite;
        GetComponent<BoxCollider2D>().size = m_animalType.Prefab.GetComponent<BoxCollider2D>().size;
        m_speed = m_animalType.Speed;
    }

    private Vector2 GetRandomPosition()
    {
        // Using a multiplier in order to keep a percentage of the screen without animals, at the bounds
        float width = (Camera.main.aspect * Camera.main.orthographicSize) * m_boundMultiplier;
        float height = Camera.main.orthographicSize * m_boundMultiplier;

        float x = UnityEngine.Random.Range(-width, width);
        float y = UnityEngine.Random.Range(-height, height);
        Vector2 position = new Vector2(x, y);

        // Now it is (0, 0) because it is in the midle of the screen,
        // but actually it could be any other value
        Vector2 boundPosition = m_bound.transform.position;
        Vector2 boundSize = m_bound.GetComponent<BoxCollider2D>().size;

        // We do not want to spawn animals into the animal pen
        // so, if it is that case, we keep looking for another random position
        while (position.x > boundPosition.x - boundSize.x && position.x < boundPosition.x + boundSize.x &&
                position.y > boundPosition.y - boundSize.y && position.y < boundPosition.y + boundSize.y)
        {
            x = UnityEngine.Random.Range(-width, width);
            y = UnityEngine.Random.Range(-height, height);
            position = new Vector2(x, y);
        }

        return position;
    }

    public void Dispose()
    {
        m_pool.Despawn(this);
    }

    public void OnDespawned()
    {
        m_pool = null;
    }

    public void OnSpawned(IMemoryPool pool)
    {
        m_pool = pool;

        Reset();
    }

    public class Factory : PlaceholderFactory<Animal>
    {
    }
}
