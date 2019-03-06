using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

public enum PlayerState
{
    Active,
    Pasive
}

public class Herdsman : Character
{
    private List<Animal> m_animalsOnTheField;
    private float m_rangeToAnimal;
    private int m_numberMaxAnimalsInTheGroup;

    private PlayerState m_state;

    private Vector2 m_initialPosition;

    private float m_baseSpeed;

    public void Start()
    {
        m_state = PlayerState.Pasive;
        m_baseSpeed = m_settings.PlayerSpeed;
        m_initialPosition = transform.position;
        m_rangeToAnimal = m_settings.PlayerRangeToAnimal;
        m_numberMaxAnimalsInTheGroup = m_settings.NumberOfMaxAnimalsInTheGroup;
        m_animalsOnTheField = m_gameController.AnimalsOnTheField;
        m_gameController.Player = this;
    }

    public void Init()
    {
        m_speed = m_baseSpeed;
        transform.position = m_initialPosition;
        m_targetPosition = transform.position;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        // Every t1 time (by default - 10s) the player's speed increases
        Observable.FromCoroutine<int>(observer => IncreaseSpeedWithTime(observer)).Subscribe(
            i => IncreaseSpeed()); ;
    }

    private IEnumerator IncreaseSpeedWithTime(IObserver<int> observer)
    {
        while (m_gameController.State == GameState.Playing)
        {
            yield return new WaitForSeconds(m_settings.TimeSpeedIncrement);
            observer.OnNext(0);
        }
        
        observer.OnCompleted();
    }

    // Updates the group speed
    private void IncreaseSpeed()
    {
        m_baseSpeed += m_settings.SpeedIncrement;
        m_speed = m_baseSpeed;

        foreach (Animal animal in m_gameController.AnimalsInTheGroup)
        {
            animal.Speed = animal.Type.Speed;
            if (animal.Speed < m_speed)
            {
                m_speed = animal.Speed;
            }
            else
            {
                animal.Speed = m_speed;
            }
        }
    }

    private void Update()
    {
        if (m_state == PlayerState.Pasive)
        {
            m_targetPosition = transform.position;
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;

            return;
        }

        CheckForAnimals();

        GetTargetPosition();
        Move();
    }

    public PlayerState State
    {
        get { return m_state; }
        set { m_state = value; }
    }

    private void CheckForAnimals()
    {
        if (m_gameController.AnimalsInTheGroup.Count < m_numberMaxAnimalsInTheGroup)
        {
            Animal animal = GetAnimalInRange();
            if (animal != null)
            {
                animal.Speed = animal.Type.Speed;
                m_gameController.LetAnimalFollowPlayer(animal);

                // Changing the speed depending on the animal
                if (animal.Speed < m_speed)
                {
                    m_speed = animal.Speed;
                }
                else
                {
                    animal.Speed = m_speed;
                }
            }
        }
    }

    public override void GetTargetPosition()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private Animal GetAnimalInRange()
    {
        foreach (Animal animal in m_animalsOnTheField)
        {
            if (Vector2.Distance(animal.transform.position, transform.position) < m_rangeToAnimal)
            {
                return animal;
            }
        }

        return null;
    }

    public void UnloadAnimals()
    {
        m_gameController.UnloadAnimals();
        m_speed = m_baseSpeed;
    }
}
