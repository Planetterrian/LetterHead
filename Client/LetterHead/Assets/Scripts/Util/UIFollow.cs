using UnityEngine;
using System.Collections;

public class UIFollow : MonoBehaviour {

    public Transform objectToFollow;

    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void OnSpawned()
    {

    }

    void Update()
    {
        if(!objectToFollow)
            return;

        var pos = RectTransformUtility.WorldToScreenPoint(Camera.main, objectToFollow.position);

        transform.position = pos;
    }
}
