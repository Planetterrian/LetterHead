using UnityEngine;
using System.Collections;

public class TransformSync : MonoBehaviour
{
    public Transform target;
    private Transform xfrm;

    public bool syncPosition = true;
    public bool syncRotation = true;
    public bool syncScale = true;

    public Vector3 positionOffset = Vector3.zero;

	// Use this for initialization
	void Awake()
	{
	    xfrm = transform;
	}
	
	// Update is called once per frame
	void Update ()
	{
        if (!target)
            return;
	    
        if (syncPosition)
	        xfrm.transform.position = (target.position + positionOffset);

        if (syncScale)
            xfrm.transform.localScale = target.localScale;

        if(syncRotation)
            xfrm.transform.rotation = target.rotation;
	}
}
