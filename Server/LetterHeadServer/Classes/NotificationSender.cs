using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LetterHeadServer.Models;
using Newtonsoft.Json.Linq;
using PushSharp.Core;
using PushSharp.Google;

namespace LetterHeadServer.Classes
{
    public class NotificationSender
    {
        private static string senderId = "773989279545";
        private static string token = "AIzaSyBRL9PxVgTT9LtqKaTL_1ZvJdtJETEm4xc";

        public void Send(User user, NotificationDetails message)
        {
            // Configuration
            var config = new GcmConfiguration(senderId, token, null);

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

            // Queue a notification to send
            gcmBroker.QueueNotification(new GcmNotification
            {
                RegistrationIds = new List<string> {
                        user.AndroidNotificationToken
                    },
                Data = JObject.Parse("{\"content_title\" : \"" + message.title.Replace("\"", "\\\"") + "\", \"content_text\":\"" + message.content.Replace("\"", "\\\"") + "\", \"ticker_text\" : \"" + message.content.Replace("\"", "\\\"") + "\", " +
                                     "\"tag\" : \"" + message.tag.Replace("\"", "\\\"") + "\", " +
                                     "\"user_info\": { \"key1\"  : \"value1\", \"key2\"  : \"value2\" } }")
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