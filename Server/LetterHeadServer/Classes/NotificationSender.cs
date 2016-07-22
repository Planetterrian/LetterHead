using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using LetterHeadServer.Models;
using Newtonsoft.Json.Linq;
using PushSharp.Apple;
using PushSharp.Core;
using PushSharp.Google;

namespace LetterHeadServer.Classes
{
    public class NotificationSender
    {
        private static string androidSenderId = "392465604696";
        private static string androidToken = "AIzaSyC3rg8uTKVtfER88CRDWYejpX2ccvLHwK0";

//        private const string certb64 = "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";
        private const string certb64 = "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";

        private static readonly byte[] certBytes;

        static NotificationSender()
        {
            certBytes = Convert.FromBase64String(certb64);
        }

        public void SendIOS(User user, NotificationDetails message)
        {
            // Configuration (NOTE: .pfx can also be used here)
            //var config = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Sandbox, HttpContext.Current.Server.MapPath("~/App_Data/letterhead-push.pfx"), "letterhead");
            var config = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Production, certBytes, "letterhead");
            //var config = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Sandbox, 

            // Create a new broker
            var apnsBroker = new ApnsServiceBroker(config);

            // Wire up events
            apnsBroker.OnNotificationFailed += (notification, aggregateEx) => {

                aggregateEx.Handle(ex => {

                    // See what kind of exception it was to further diagnose
                    if (ex is ApnsNotificationException)
                    {
                        var notificationException = (ApnsNotificationException)ex;

                        // Deal with the failed notification
                        var apnsNotification = notificationException.Notification;
                        var statusCode = notificationException.ErrorStatusCode;

                        System.Diagnostics.Trace.TraceError($"Apple Notification Failed: ID={apnsNotification.Identifier}, Code={statusCode}");

                    }
                    else
                    {
                        // Inner exception might hold more useful information like an ApnsConnectionException           
                        System.Diagnostics.Trace.TraceError($"Apple Notification Failed for some unknown reason : {ex.InnerException}");
                    }

                    // Mark it as handled
                    return true;
                });
            };

            apnsBroker.OnNotificationSucceeded += (notification) => {
                Console.WriteLine("Apple Notification Sent!");
            };

            // Start the broker
            apnsBroker.Start();

            var payload = "{\n" +
"\t\"aps\": {\n" +
"\t\t\"alert\": {\n" +
"\t\t\t\"title\": \"" + message.title.Replace("\"", "\\\"") + "\",\n" +
"\t\t\t\"body\": \"" + message.content.Replace("\"", "\\\"") + "\",\n" +
"\t\t\t\"action-loc-key\": \"VIEW\"\n" +
"\t\t},\n" +
"\t\t\"badge\": 1\n";

            if (message.type == NotificationDetails.Type.YourTurn)
                payload += "\t\t,\"sound\": \"TurnNotification.wav\"\n";
            else if (message.type == NotificationDetails.Type.Invite)
                payload += "\t\t,\"sound\": \"DoorKnock.wav\"\n";

            payload += "\t},\n" +
            "\t\"user_info\": {\n" +
            "\t\t\"alertType\": \"" + (int)message.type + "\"\n" +
            "\t}\n" +
            "}";
            // Queue a notification to send
            apnsBroker.QueueNotification(new ApnsNotification
            {
                DeviceToken = user.IosNotificationToken,
                Payload = JObject.Parse(payload)
            });
            

            // Stop the broker, wait for it to finish   
            // This isn't done after every message, but after you're
            // done with the broker
            apnsBroker.Stop();
        }

        public void SendAndroid(User user, NotificationDetails message)
        {
            // Configuration
            var config = new GcmConfiguration(androidSenderId, androidToken, null);
            config.GcmUrl = "https://fcm.googleapis.com/fcm/send";

            //System.Diagnostics.Trace.TraceInformation("Sending notification to " + user.Id + " token = " + user.AndroidNotificationToken);

            // Create a new broker
            var gcmBroker = new GcmServiceBroker(config);

            // Wire up events
            gcmBroker.OnNotificationFailed += (notification, aggregateEx) => {

                aggregateEx.Handle(ex => {

                    // See what kind of exception it was to further diagnose
                    if (ex is GcmNotificationException)
                    {
                        var notificationException = (GcmNotificationException)ex;

                        // Deal with the failed notification
                        var gcmNotification = notificationException.Notification;
                        var description = notificationException.Description;

                        System.Diagnostics.Trace.TraceError($"GCM Notification Failed: ID={gcmNotification.MessageId}, Desc={description}");
                    }
                    else if (ex is GcmMulticastResultException)
                    {
                        var multicastException = (GcmMulticastResultException)ex;

                        foreach (var succeededNotification in multicastException.Succeeded)
                        {
                            System.Diagnostics.Trace.TraceError($"GCM Notification Failed: ID={succeededNotification.MessageId}");
                        }

                    }
                    else if (ex is DeviceSubscriptionExpiredException)
                    {
                        var expiredException = (DeviceSubscriptionExpiredException)ex;

                        var oldId = expiredException.OldSubscriptionId;
                        var newId = expiredException.NewSubscriptionId;

                        System.Diagnostics.Trace.TraceError($"Device RegistrationId Expired: {oldId}");

                        if (!string.IsNullOrEmpty(newId))
                        {
                            // If this value isn't null, our subscription changed and we should update our database
                            System.Diagnostics.Trace.TraceError($"Device RegistrationId Changed To: {newId}");
                        }
                    }
                    else if (ex is RetryAfterException)
                    {
                        var retryException = (RetryAfterException)ex;
                        // If you get rate limited, you should stop sending messages until after the RetryAfterUtc date
                        System.Diagnostics.Trace.TraceError($"GCM Rate Limited, don't send more until after {retryException.RetryAfterUtc}");
                    }
                    else
                    {
                        System.Diagnostics.Trace.TraceError("GCM Notification Failed for some unknown reason");
                    }

                    // Mark it as handled
                    return true;
                });
            };

            gcmBroker.OnNotificationSucceeded += (notification) => {
                //System.Diagnostics.Trace.TraceInformation("GCM Notification Sent!");
            };

            // Start the broker
            gcmBroker.Start();
            var sound = "";
            
            if (message.type == NotificationDetails.Type.Invite)
                sound = "DoorKnock.wav";
            else
            {
                sound = "TurnNotification.wav";
            }

            // Queue a notification to send
            gcmBroker.QueueNotification(new GcmNotification
            {



            RegistrationIds = new List<string> {
                        user.AndroidNotificationToken
                    },


            Data = JObject.Parse("{\"content_title\" : \"" + message.title.Replace("\"", "\\\"") + "\", \"content_text\":\"" + message.content.Replace("\"", "\\\"") + "\", \"ticker_text\" : \"" + message.content.Replace("\"", "\\\"") + "\", " +
                                     "\"tag\" : \"" + message.tag.Replace("\"", "\\\"") + "\", \"custom-sound\" : \"" + sound + "\", " +
                                     "\"user_info\": { \"alertType\"  : \"" + (int)message.type + "\" } }")
            });

            // Stop the broker, wait for it to finish   
            // This isn't done after every message, but after you're
            // done with the broker
            gcmBroker.Stop();
        }
    }
}

public struct NotificationDetails
{
    public string title;
    public string content;
    public string tag;
    public Type type;

    public enum Type
    {
        Invite, Buzz, YourTurn
    }
}