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

        private const string certb64 = "MIIM9wIBAzCCDL4GCSqGSIb3DQEHAaCCDK8EggyrMIIMpzCCBzcGCSqGSIb3DQEHBqCCBygwggckAgEAMIIHHQYJKoZIhvcNAQcBMBwGCiqGSIb3DQEMAQYwDgQIaNPeIw3gZw8CAggAgIIG8AZBRXiMQ/+WVIfp+iIyb+QA+/T1QKSyzWlXALieIWN13kafiCeilXaOeLiezwSRgil8ReaED/4oRdd9kcoqpRG8Q0AWWOZcGQg46nwOMUtcY+RpjUWBI8Z/FaB57BIHpRomWFt5HWIa2niLDh9cw5WIYwPQDXoUPc2onL/w34MektMoSrlnWvtei/CXKPPUYGCc1iU7P9K+uSkXY2XYdF7nVOD8qyJApGkDeKpAMVkRNxMXNfQmnO45Dejnl7KAwL/aBswrSMfWQjcOWYhVDE+Rgpl5Zm46x/hHH+oNl417+MvxxVLWPxTg18C3juWHKeZoZJqz+HDpAqnQTIe7NiVJ3BOQf4poXwE2ajefw2hPryaDpSMXy0xUF9/E6scwZWDOc1oOR0+qNsfQNHfSE1+7m3IbkMxgGrF/JtZyEDZpG1nBXkZExlmCQnv4BXoEcWbARwIDz1EQZATqfS7/3dWMJrZtasj3gNWOVq8sizE+nO2bx/8gDzH+PNDeiZuUQ9H/IbO599U0LfGnOWKX8wh1e7IjtCYxbzfLZEG+oERcA6nmeV5DvgcnJjXFrmZCztNacv2x+tvJPxq/bt4B3z5aATE1XcqB6gyUuamS2O6dVNub3pSIManjANape4rcecNmVNLqg7GeCK7rerQREsbWd6/r5OtyXTFNIrSHhzT+NiiCg3/d7IeuhnfdY4uwxjEOVlPCWg9TC7x3BIKuJF10bkmIg9j0y7WfmRShcZGPpmN5f6h/TsV7yybQgKbAfjCEHZDvvxmWbIdzdu69I6kOykMrCN1V1nN3rlhorvWKIQZHaqTyL4ybu3epNKnQ1LlJ/01Y8slxQgcIefplTAZEaKkUoGjtYYNu06sHk5PPkyoGmNAMRfRhn5Azw69YSlKo2niI8kXFkCpdPJaPe+5QJJLa2eeZxLxeabwdDfil5SKLqEu/y6cy55VS/sCT3eMe7GTqBcIHwFavMjNMuI+GsEmLA1UIKrxN2hGxtIg1Y7oHl79gqNqlvLF3NdH/3ej2NyyrHStFlig8A6vzxQd8PER8z0i+Z1Mf31vRT4CIvzZ+5Cztyw4WzmZe+QI+SLGbklfQxuKLSjKQfjeSMGXGgyoN40L3f9qIkDt+4ef4R0204AaMtwFhvBrZUx7b59A/GwkSLZkSqB+Wt2zyPtKZvyh/ixIzUroHQ2N6ojvwb6eQMw+4/T9U3QSm0C0BdxeYIg7TFk6QcYlYol47MZ/CTDkL8VWPH0P7sFEq4zDZLpw4/M+gUPCLPvYFUDAJW6TtgUXPFhYKkOuo4CC64cMsGYR9PShAu39DJdVmTwkuHSYsbSwE1aIod7e19e/gDPr+SJpz+59k9nFf/0oD2ri/QKXXMBnwWd/SUx9UivKHQ/aimcFqDZR7yvyEqzKcpPszK9O6jsYqt5GL5lROcIDIIznLCfs5kBtaou0cO3OYlTlot/Mzzy6a7a1aS1AfrAiQd9sqW0HmaEsSefslWi+vQLPPBq4BnHQ+EeF9CiLBe68mvCieLnooEhj63Y+e8pTSlaS8yrYOV2pUFKsJ612y8At0fXDA+Koks6PreKIa0HXFF/edFppXLr3bvSviAOw7LcGoEB0ztSWvL70mNASgXrOu8QMCRwRY4q2bNjFViYp4bHocPe6Ql1J3efenhBK2NUkUwJvzgMtcV2zm1yTDlM4pTDgKudgsJEQvjkZX+6Zt7aqXiRn7EZVBbYKXhL0Ctk8h23K2BanVzuQhH44Kl0/cxcZc6XP2dxSiZFJj4bXYmSeT9ZonK4nwLgiIqhJBNIMzV5HA0XWqOtLQavsVXatZqQ/LupcB82EH+oX0N4flKPGYbKb8bNPqiVplFL7ULGfRRFkRYpHJXCe8dJxcZ4e3XpZNsfLHCwjN4xAtvn2EAtS/NHfH1ALHwqc3TXxga4zZPx4IZllPoZDx1aXuKSKe19pAbkHycjdN91vCjOvBR9gMI1QBHD31XdTAGfXzaDqlEYICTb1Aw2SVxI5cgIzmg5TKUSd9caDDY82zOt3VL/wu4AZobsG0a7kcm0a4u/szldUq3xn34jdul1e8U4ohFF0tzxOCWNpWSyUflSao3RaEePPRIifASzO8z8mnvGEXk41OUBSqB2GJiEt/C0qJKfc/6fXA0wi9ACvDXO+FOxGPRBjG/eL7sMZCsQa7c+fQYLpidbGhWQ6OAKch4GMAc1sgGq2DvHy36HYDTVJAwyzh1SMCZhCERo+YVFof/Qajs7WC6aGZu3e+ML6Rm0rpJzdYFaaG70DtCfyBd3VL1Wv/FlPUgTVZSSGpyOrC3CJg9BDkJUbaP6ETKkCaYi8jxNTIBIjjnrIwTBWBew51gnlTgrVX0JGh01yi8zCCBWgGCSqGSIb3DQEHAaCCBVkEggVVMIIFUTCCBU0GCyqGSIb3DQEMCgECoIIE7jCCBOowHAYKKoZIhvcNAQwBAzAOBAjX06PQLsOknwICCAAEggTInkEG1sqMIaIO/q+biTkNWllZtAmq9gwzol//Pks0Mq3QFh9C7mHMrW+K0x6W2u/qkL6AA2Ag3dWzFjP8QQNlcyI2t/BD6RjK1DZEsRl9JbaAR2ZPV6b5XzniX8Q2UhRLJjnctQduXJvqVtrvjlCrVufrcFa/mubj6ijh7a2QcCiOXGu09JW7iw6e77aphWu/AEeLtNnr8HWyTjt3U6lTOA20L4GzFBRE3CEjfo6KHTZAy0NUDW3HRkQboaH3H5X6X1LJaHJx93QNyiF0K9+6G/+on76XkkAYWJc/1itN0jxlKIjgAykkJFcDE9MYAM+oU1E5kTYIKKX87U8m+X5t0n09SHoNrBxrmLLGy11oOM2tw2380ZZNueAOKRHkYwJUlX7RMNI9qhAdDUpVKOOKqKRa1mSGK6Zw5r6/st5s+ID3SeYlLe6lLRg/4f655D0kGM5xAjpQAHSvmOPl9T1Plcd06viEIDb6Rn2guq7l1+stqSHUGfc9/s+uPCWlN7pp287eIAja7PAdD1jup5lcsAllIYyt817k9fKTeqccWskeyrSlrKuZHWXnBrrOUX+/ukE6iKsXbv2pEMYu3gpwz6jJS6nvO8EJMK3KTefROLe9tJNCawWwTL+mkBnLJLSQ9gq4HJYZAxl0qTbrATkAWz+5YF1S8ejWRUDrKGiDMKFDDgPFjE6sa6S03f2yjrDXWNsCtiQhmoTgYXnbeDcrpA6RDOmJoaOQX5xXzzBrZpkK/a2bnwGA6NwBV/zZXZPQeoIhA5IT/M6vmKRKxdHtYZZQb6IxVwO/iu6je57t7Jdv3PMPyPr5Dp02MmghP1KhCnCvJz1aFd0ySXDeXSVPvE8JgMVetSW7SFOsF9HZHKv9rKmsyfC2JpLiCrOsUBYfTkBigxxvXohcFcFznIJJzr74tkmRy8ZU+KWptnBjQ+uXVKwZ1R0KXatUPtZ0wZM8jtKpCSlzW5D6TYX1uyDF1E3Cos4J/FJKRb6jceTqAQOPV6HLCq8KTG7OktHonwtGyQda78hb0uoH9zi7VzTeCQ9yi8fovGOJZP9mnx3rjKpjEVdP4tbCTXCvynjpNnE8I1tYbbpxXKkXSeYl3tfXzfphJ2Uyr7PS+CsDGHgR3t1KGMnkSs2PYMka1y0XIymNtJejpRaFosrTRmRH8WrGzGlXUT5afzOoCIShI8e58uKCmP5EYMakgU9VgNlA3OOD8ewMtKCIYtTtvRTl8KlHaaZgUYihY0uH2ZFBtvvO+JHMpNHcaVkcwLPj6OWVOH0LjSY4tasKA5+ozhYdjbtABj12Fhg9AA7V8SfUM/2VaK+VtqgnFSNlom3zLuON/SQjZ+zc7iUgviyGh4iAldUd4xQD+3A854yHNfNWHjxJuZLZVEK/rZQ3OX+yyG5It+nLm8dDb2weBLUbrUSnX2yg+FIu305bYEH4pvSVxzmFAUZhArCPj+hMecyLkdmMYQTOmFO3y9JZ+fMO/y75Ch6YHXq5yzKol4fmOqe8LIotgM/9SQN4z0aThPXElT81r1ubrmd6mfTFM9rctJy/uGh+vlZDEpHZKA5mXXw4pTjFRrYuzWtge2wisE2ptCoAXHF203wr7lYVcVGcNSc1yodi7rsfCLoomlPRMUwwJQYJKoZIhvcNAQkUMRgeFgBQAGUAdABlACAAUwBpAG0AYQByAGQwIwYJKoZIhvcNAQkVMRYEFN/12lp5+wMwv6MEW42ENnwO3+eDMDAwITAJBgUrDgMCGgUABBS2ma3jLxPTMDWdqVPVV2iIx39FLAQImC4+okzdkM8CAQE=";
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