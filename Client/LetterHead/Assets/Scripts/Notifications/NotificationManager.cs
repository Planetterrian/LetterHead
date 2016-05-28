using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoxelBusters.NativePlugins;

public class NotificationManager : Singleton<NotificationManager>
{
    
    private void Start()
    {
        NPBinding.NotificationService.RegisterNotificationTypes(NotificationType.Alert | NotificationType.Badge | NotificationType.Sound);
    }

    // Register for callbacks
    void OnEnable()
    {
        //Triggered when registration for remote notification event is done.
        NotificationService.DidFinishRegisterForRemoteNotificationEvent += DidFinishRegisterForRemoteNotificationEvent;

        //Triggered when local notification was clicked for launching the app.
        NotificationService.DidLaunchWithLocalNotificationEvent += DidLaunchWithLocalNotificationEvent;

        //Triggered when remote notification was clicked for launching the app.
        NotificationService.DidLaunchWithRemoteNotificationEvent += DidLaunchWithRemoteNotificationEvent;

        //Triggered when a local notification is received.
        NotificationService.DidReceiveLocalNotificationEvent += DidReceiveLocalNotificationEvent;

        //Triggered when a remote notification is received.
        NotificationService.DidReceiveRemoteNotificationEvent += DidReceiveRemoteNotificationEvent;
    }

    private void DidReceiveRemoteNotificationEvent(CrossPlatformNotification _notification)
    {
        Debug.Log("Recieved notification: " + _notification.AlertBody);
    }

    private void DidReceiveLocalNotificationEvent(CrossPlatformNotification _notification)
    {
        Debug.Log("Recieved local notification: " + _notification.AlertBody);
    }

    private void DidLaunchWithRemoteNotificationEvent(CrossPlatformNotification _notification)
    {
        
    }

    private void DidLaunchWithLocalNotificationEvent(CrossPlatformNotification _notification)
    {
        
    }

    private void DidFinishRegisterForRemoteNotificationEvent(string _devicetoken, string _error)
    {
        if (!string.IsNullOrEmpty(_error))
        {
            Debug.LogError("Error getting GSM token: " + _error);
            return;
        }

        Debug.Log("Got GSM token: " + _devicetoken);
    }

    //Un-Registering once done.    
    void OnDisable()
    {
        NotificationService.DidFinishRegisterForRemoteNotificationEvent -= DidFinishRegisterForRemoteNotificationEvent;
        NotificationService.DidLaunchWithLocalNotificationEvent -= DidLaunchWithLocalNotificationEvent;
        NotificationService.DidLaunchWithRemoteNotificationEvent -= DidLaunchWithRemoteNotificationEvent;
        NotificationService.DidReceiveLocalNotificationEvent -= DidReceiveLocalNotificationEvent;
        NotificationService.DidReceiveRemoteNotificationEvent -= DidReceiveRemoteNotificationEvent;
    }

    public void RegisterForNotifications()
    {
        //Register for remote notification
        NPBinding.NotificationService.RegisterForRemoteNotifications();
    }

    public void UnregisterForNotifications()
    {
        //Unregister for remote notification
        NPBinding.NotificationService.UnregisterForRemoteNotifications();
    }
}