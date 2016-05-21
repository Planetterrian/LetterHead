using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PersistManager : MonoBehaviour
{
    public GameObject persistCanvas;

    private void Awake()
    {
    }

    private void Start()
    {
        persistCanvas.SetActive(true);
    }
}