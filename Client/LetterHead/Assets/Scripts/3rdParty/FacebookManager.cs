using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Facebook.Unity;
using UnityEngine;

class FacebookManager : Singleton<FacebookManager>
{
    void Start()
    {
        FB.Init(() =>
        {
            Debug.Log("Facebook Initilized");
        });
    }
}
