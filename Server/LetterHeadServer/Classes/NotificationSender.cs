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

        // possible invalid
        //private const string certb64 = "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";
        //private const string certb64 = "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";
        //private const string certb64 = "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";
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
                        System.Diagnostics.Trace.TraceError(notificationException.InnerException.Message);

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