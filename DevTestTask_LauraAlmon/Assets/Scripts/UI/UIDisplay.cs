using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

public class UIDisplay : MonoBehaviour
{
    [Inject]
    private UISettings m_uiSettings;

    [Inject]
    private GameController m_controller;

    // Displays player points and the remaining time in GUI
    private void Start()
    {
        m_controller.m_points
            .Select(points => string.Format("Points: {0}", points))
            .Subscribe(text => m_uiSettings.Points.text = text);

        m_controller.m_time
            .Select(time => string.Format("Time: {0} s", time))
            .Subscribe(text => m_uiSettings.Time.text = text);

        m_controller.m_finalPoints
            .Select(points => string.Format("{0} points!", points))
            .Subscribe(text => m_uiSettings.FinalPoints.text = text);
    }
}
