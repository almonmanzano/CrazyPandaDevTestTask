using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UniRx;
using UnityEngine;
using Zenject;

public enum GameState
{
    Waiting,
    Playing,
    GameOver
}

public class GameController : IInitializable, ITickable
{
    // Reactive Properties using UniRx
    public ReactiveProperty<int> m_points = new ReactiveProperty<int>(0);
    public ReactiveProperty<int> m_time = new ReactiveProperty<int>();
    public ReactiveProperty<int> m_finalPoints = new ReactiveProperty<int>();

    // We need to inject Animal Spawner in order to spawn new animals
    [Inject]
    private AnimalSpawner m_spawner;

    // We need to inject Game Settings in order to get:
    // - Spawning Time
    // - Number Of Max Animals On The Field
    // - Number Of Max Animals In The Group
    // - Number Of Initial Animals
    [Inject]
    private GameSettings m_settings;
    private float m_spawningTime;
    private int m_numberOfMaxAnimalsOnTheField;
    private int m_numberOfInitialAnimals;

    [Inject]
    private UISettings m_uiSettings;

    private float m_timeSinceLastSpawn = 0f;
    
    private List<Animal> m_animalsOnTheField = new List<Animal>();
    private List<Animal> m_animalsInTheGroup = new List<Animal>();

    private Herdsman m_herdsman;

    private GameState m_state = GameState.Waiting;

    public void Initialize()
    {
        m_spawningTime = m_settings.SpawningTime;
        m_numberOfMaxAnimalsOnTheField = m_settings.NumberOfMaxAnimalsOnTheField;
        m_numberOfInitialAnimals = m_settings.NumberOfInitialAnimals;

        // Shows the message to start the game
        m_uiSettings.WaitingPanel.SetActive(true);
    }

    private void ResetGame()
    {
        m_time.Value = m_settings.TotalTime;
        ResetPoints();
        
        foreach (Animal animal in m_animalsInTheGroup)
        {
            m_spawner.RemoveAnimal(animal);
        }
        m_animalsInTheGroup.Clear();

        foreach (Animal animal in m_animalsOnTheField)
        {
            m_spawner.RemoveAnimal(animal);
        }
        m_animalsOnTheField.Clear();

        // We put some animals at the start
        for (int i = 0; i < m_numberOfInitialAnimals; i++)
        {
            AddAnimal();
        }
    }

    private void StartGame()
    {
        // To avoid moving the Herdsman while clicking for starting the game
        Input.ResetInputAxes();

        // Initializes the player
        m_herdsman.Init();

        m_uiSettings.WaitingPanel.SetActive(false);
        m_uiSettings.GameOverPanel.SetActive(false);
        m_state = GameState.Playing;
        m_herdsman.State = PlayerState.Active;
        StartCountdown();
    }

    private void StartCountdown()
    {
        Observable.FromCoroutine<int>(observer => Countdown(m_time.Value, observer)).Subscribe(
            i => DecreaseTime(1),
            () => GameOver());
    }

    // Countdown from T (by default - 30 seconds)
    private IEnumerator Countdown(int duration, IObserver<int> observer)
    {
        if (duration < 0)
        {
            observer.OnError(new ArgumentOutOfRangeException());
        }

        var count = duration;
        while (count > 0)
        {
            observer.OnNext(m_time.Value);
            count--;
            yield return new WaitForSeconds(1);
        }

        observer.OnCompleted();
    }

    public Herdsman Player
    {
        get { return m_herdsman; }
        set { m_herdsman = value; }
    }

    public GameState State
    {
        get { return m_state; }
    }

    private void AddAnimal()
    {
        Animal animal = m_spawner.AddAnimal();
        m_animalsOnTheField.Add(animal);
    }

    public void Tick()
    {
        switch (m_state)
        {
            case GameState.Waiting:
                UpdateWaiting();
                break;
            case GameState.Playing:
                UpdatePlaying();
                break;
            default:
                break;
        }
    }

    private void UpdateWaiting()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ResetGame();
            StartGame();
        }
    }

    private void UpdatePlaying()
    {
        // If there are more animals on the field than C (by default - 10),
        // new animals do not appear until one of the waiting animals join the group
        if (m_timeSinceLastSpawn >= m_spawningTime &&
            m_animalsOnTheField.Count < m_numberOfMaxAnimalsOnTheField)
        {
            AddAnimal();
            m_timeSinceLastSpawn = 0f;
        }

        m_timeSinceLastSpawn += Time.deltaTime;
    }

    private void GameOver()
    {
        m_uiSettings.GameOverPanel.SetActive(true);
        m_finalPoints.Value = m_points.Value;
        m_state = GameState.GameOver;
        m_herdsman.State = PlayerState.Pasive;

        Observable.Timer(TimeSpan.FromSeconds(m_settings.GameOverTime))
            .Subscribe(x =>
            {
                m_state = GameState.Waiting;
            });
    }

    public void LetAnimalFollowPlayer(Animal animal)
    {
        m_animalsOnTheField.Remove(animal);
        m_animalsInTheGroup.Add(animal);
    }

    public List<Animal> AnimalsOnTheField
    {
        get { return m_animalsOnTheField; }
    }

    public List<Animal> AnimalsInTheGroup
    {
        get { return m_animalsInTheGroup; }
    }

    public void ResetPoints()
    {
        m_points.Value = 0;
    }

    public void AddPoints(int points)
    {
        m_points.Value += points;
    }

    private void DecreaseTime(int t)
    {
        m_time.Value -= t;
    }

    public void UnloadAnimals()
    {
        if (m_animalsInTheGroup.Count == 0)
        {
            return;
        }

        // Unloading of the animals in the animal pen takes time (by default - 0.5s)
        // During this time the Herdsman stays pasive
        m_herdsman.State = PlayerState.Pasive;
        Observable.Timer(TimeSpan.FromSeconds(m_settings.UnloadingTime)) // Using UniRx
            .Subscribe(x =>
            {
                if (m_state != GameState.Playing)
                {
                    return;
                }
                foreach (Animal animal in m_animalsInTheGroup)
                {
                    // When an animal gets to the animal pen, it does not get deleted
                    // It is used for the new spoun on the field
                    m_spawner.RemoveAnimal(animal);
                    AddPoints(animal.Points);
                }
                m_animalsInTheGroup.Clear();
                m_herdsman.State = PlayerState.Active;
            });
    }
}
