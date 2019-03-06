using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public abstract class Character : MonoBehaviour
{
    // We need to inject Game Settings
    [Inject]
    [HideInInspector] public GameSettings m_settings;
    [HideInInspector] public float m_speed;
    
    // We need to inject Game Controller
    [Inject]
    [HideInInspector] public GameController m_gameController;

    [HideInInspector] public Vector2 m_targetPosition;

    private float m_rangeToPoint;
    private Rigidbody2D m_rigidbody;

    private void Awake()
    {
        m_rangeToPoint = m_settings.RangeToPoint;
        m_targetPosition = transform.position;
        m_rigidbody = GetComponent<Rigidbody2D>();
    }

    public abstract void GetTargetPosition();

    // Character's and group's movement realized in the same way
    public void Move()
    {
        if ((tag == "Player" && m_gameController.Player.State == PlayerState.Pasive) ||
            m_gameController.State != GameState.Playing)
        {
            m_targetPosition = transform.position;
            m_rigidbody.velocity = Vector2.zero;
            return;
        }

        GetTargetPosition();

        Vector2 movement = new Vector2(m_targetPosition.x - transform.position.x,
            m_targetPosition.y - transform.position.y).normalized;
        m_rigidbody.velocity = movement * m_speed;

        GetComponent<SpriteRenderer>().flipX = GetComponent<Rigidbody2D>().velocity.x > 0;

        // To avoid flickering
        if (IsAlreadyThere())
        {
            m_targetPosition = transform.position;
            m_rigidbody.velocity = Vector2.zero;
        }
    }

    private bool IsAlreadyThere()
    {
        return (Mathf.Abs(m_targetPosition.x - transform.position.x) <= m_rangeToPoint &&
            Mathf.Abs(m_targetPosition.y - transform.position.y) <= m_rangeToPoint);
    }
}
