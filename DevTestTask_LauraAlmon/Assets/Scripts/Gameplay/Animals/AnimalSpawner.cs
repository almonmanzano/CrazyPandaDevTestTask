using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalSpawner
{
    readonly Animal.Factory m_animalFactory;
    readonly List<Animal> m_animals = new List<Animal>();

    public AnimalSpawner(Animal.Factory animalFactory)
    {
        m_animalFactory = animalFactory;
    }

    public Animal AddAnimal()
    {
        var animal = m_animalFactory.Create();
        animal.transform.SetParent(null);
        m_animals.Add(animal);

        return animal;
    }

    public void RemoveAnimal(Animal animal)
    {
        if (m_animals.Contains(animal))
        {
            animal.Dispose();
            m_animals.Remove(animal);
        }
    }
}
