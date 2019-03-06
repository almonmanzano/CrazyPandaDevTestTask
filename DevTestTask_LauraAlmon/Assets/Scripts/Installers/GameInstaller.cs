using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class GameInstaller : MonoInstaller<GameInstaller>
{
    [SerializeField]
    private GameSettings m_settings;

    [SerializeField]
    private UISettings m_uiSettings;

    public override void InstallBindings()
    {
        Container.BindInstance(m_settings);
        Container.BindInstance(m_uiSettings);

        Container.BindInterfacesAndSelfTo<GameController>().AsSingle();
        Container.Bind<AnimalSpawner>().AsSingle();

        Container.BindFactory<Animal, Animal.Factory>()
            .FromMonoPoolableMemoryPool(x => x
            .WithInitialSize(m_settings.PoolSize)
            .FromComponentInNewPrefab(m_settings.AnimalPrefab)
            .UnderTransformGroup("Animals"));
    }
}

[Serializable]
public class GameSettings
{
    public int TotalTime;
    public float PlayerSpeed;
    public float RangeToPoint;
    public float PlayerRangeToAnimal;
    public GameObject AnimalPrefab;
    public int PoolSize;
    public AnimalType[] AnimalTypes;
    public float SpawningTime;
    public int NumberOfMaxAnimalsOnTheField;
    public int NumberOfMaxAnimalsInTheGroup;
    public int NumberOfInitialAnimals;
    public GameObject Bound;
    public float BoundMultiplier;
    public float DistanceBetweenUnits;
    public float UnloadingTime;
    public float TimeSpeedIncrement;
    public float SpeedIncrement;
    public float GameOverTime;
}

[Serializable]
public class UISettings
{
    public Text Points;
    public Text Time;
    public GameObject WaitingPanel;
    public GameObject GameOverPanel;
    public Text FinalPoints;
}