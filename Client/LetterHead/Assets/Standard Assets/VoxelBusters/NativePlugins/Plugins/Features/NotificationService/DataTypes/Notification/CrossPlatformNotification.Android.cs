﻿using UnityEngine;
using System.Collections;

#if USES_NOTIFICATION_SERVICE 
using System.Collections.Generic;
using VoxelBusters.Utility;

namespace VoxelBusters.NativePlugins
{
	public partial class CrossPlatformNotification 
	{
		/// <summary>
		/// Represents an object that holds notification properties specific to Android platform.
		/// </summary>
		public class AndroidSpecificProperties
		{
			#region Constants
			
			private		const	string 		kContentTitleKey	= "content-title";
			private		const 	string 		kTickerTextKey		= "ticker-text";
			private		const	string 		kTagKey				= "tag";
			private		const	string 		kLargeIcon			= "large-icon";
			
			#endregion

			#region Properties

			/// <summary>
			/// The first line of text in the notification. 
			/// </summary>
			/// <description>
			/// This is the text that is displayed in the notification bar on Android as title of the notification.
			/// </description>
			public string ContentTitle
			{
				get; 
				set;
			}

			/// <summary>
			/// The ticker text. 
			/// </summary>
			/// <description>
			/// The text that will be visible in a scrolling fashion on status bar.
			/// </description>
			/// <remarks>
			/// \note All the devices using pre-Lollipop OS has this feature.
			/// </remarks>
			public string TickerText
			{
				get; 
				set;
			}

			/// <summary>
			/// The tag of the notification.
			/// </summary>
			/// <description>
			/// The tag defines this notification uniquely or can be empty which overwrites previous notification. 
			/// If the tag is set with different value than previous notification, it won't override the previous one in notification bar, otherwise it will.
			/// </description>
			public string Tag
			{
				get; 
				set;
			}

			/// <summary>
			/// The name of the sound file to play when an alert is displayed.
			/// </summary>
			/// <remarks>
			/// \note Make sure all referred sounds are in Assets/PluginResources/Android or Common folder. 
			/// </remarks>
			[System.Obsolete("This property is deprecated. Use SoundName property of CrossPlatformNotification instance instead.")]
			public string CustomSound
			{
				get; 
				set;
			}

			/// <summary>
			/// The image used as the large icon for notification.
			/// </summary>
			/// <remarks>
			/// \note This will be the icon thats displayed in the notification. 
			/// If the value is not set, then default image will be used. 
			/// </remarks>
			public string LargeIcon
			{
				get; 
				set;
			}

			#endregion

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the <see cref="CrossPlatformNotification.AndroidSpecificProperties"/> class.
			/// </summary>
			public AndroidSpecificProperties ()
			{
				ContentTitle	= null;
				TickerText		= null;
				Tag				= null;
				LargeIcon		= null;
			}

			internal AndroidSpecificProperties (IDictionary _jsonDict)
			{
				ContentTitle	= _jsonDict.GetIfAvailable<string>(kContentTitleKey);
				TickerText		= _jsonDict.GetIfAvailable<string>(kTickerTextKey);
				Tag				= _jsonDict.GetIfAvailable<string>(kTagKey);
				LargeIcon		= _jsonDict.GetIfAvailable<string>(kLargeIcon);
			}

			#endregion

			#region Methods

			internal IDictionary JSONObject ()
			{
				Dictionary<string, string> _jsonDict	= new Dictionary<string, string>();
				_jsonDict[kContentTitleKey]				= ContentTitle;
				_jsonDict[kTickerTextKey]				= TickerText;
				_jsonDict[kTagKey]						= Tag;
				_jsonDict[kLargeIcon]					= LargeIcon;

				return _jsonDict;
			}

			public override string ToString ()
			{
				return string.Format ("[AndroidSpecificProperties: ContentTitle={0}, TickerText={1}, Tag = {2}, LargeIcon = {3}]", 
				                      ContentTitle, TickerText, Tag, LargeIcon);
			}

			#endregion
		}
	}
}
#endif