﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(GameStateManager))]
public class MementoManager : MonoBehaviour
{
    private List<GameObject> _mementos = new List<GameObject>();
    private readonly object _claimLock = new object();
    private List<GameObject> _claimableMementos = new List<GameObject>();

    private GameStateManager _gameStateManager;

    private AudioSource audioSource;
    public AudioClip[] objectTaken;

    public bool TryClaimMemento(out Memento memento)
    {
        memento = null;

        lock (_claimLock)
        {
            if (_claimableMementos.Count > 0)
            {
                var randomIndex = Random.Range(0, _claimableMementos.Count - 1);
                memento = _claimableMementos[randomIndex].GetComponent<Memento>();
                _claimableMementos.RemoveAt(randomIndex);

                return true;
            }
        }

        return false;
    }

    public void ForfeitMemento(Memento memento)
    {
        lock (_claimLock)
        {
            _claimableMementos.Add(memento.gameObject);
        }
    }

    public void DestroyMemento(Memento claimedMemento)
    {
        _gameStateManager.currentFeels -= claimedMemento.theFeels;
        _claimableMementos.Remove(claimedMemento.gameObject);
        _mementos.Remove(claimedMemento.gameObject);
        Destroy(claimedMemento.gameObject);

        audioSource.PlayOneShot(objectTaken[Random.Range(0, objectTaken.Length)]);
        _gameStateManager.itemStolen = true;
    }

    private void Start()
    {
        _mementos = GameObject.FindGameObjectsWithTag("Memento").ToList();
        _claimableMementos.AddRange(_mementos);
        _gameStateManager = GetComponent<GameStateManager>();

        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (_mementos.Count <= 0)
        {
            _gameStateManager.gameState = GameStateManager.GameState.GameOver;
        }
    }
}
