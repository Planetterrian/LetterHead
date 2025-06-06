﻿using UnityEngine;
using System.Collections;

#if USES_TWITTER && UNITY_EDITOR
using VoxelBusters.DebugPRO;

namespace VoxelBusters.NativePlugins
{
	using Internal;

	public class TwitterEditor : Twitter 
	{
		#region Init API's

		public override bool Initialise ()
		{
			// Just to show warning
			base.Initialise();

			return false;
		}

		#endregion

		#region Account API's
		
		public override void Login (TWTRLoginCompletion _onCompletion)
		{
			base.Login(_onCompletion);

			// Associated error event is raised
			TwitterLoginFailed(Constants.kNotSupportedInEditor);
		}
		
		public override void Logout ()
		{
			base.Logout();
			Console.LogError(Constants.kDebugTag, Constants.kNotSupportedInEditor);
		}
		
		public override bool IsLoggedIn ()
		{
			Console.LogError(Constants.kDebugTag, Constants.kNotSupportedInEditor);
			return base.IsLoggedIn();
		}
		
		public override string GetAuthToken ()
		{
			Console.LogError(Constants.kDebugTag, Constants.kNotSupportedInEditor);
			return base.GetAuthToken();
		}
		
		public override string GetAuthTokenSecret ()
		{
			Console.LogError(Constants.kDebugTag, Constants.kNotSupportedInEditor);
			return base.GetAuthTokenSecret();
		}
		
		public override string GetUserID ()
		{
			Console.LogError(Constants.kDebugTag, Constants.kNotSupportedInEditor);
			return base.GetUserID();
		}
		
		public override string GetUserName ()
		{
			Console.LogError(Constants.kDebugTag, Constants.kNotSupportedInEditor);
			return base.GetUserName();
		}

		#endregion

		#region Tweet API's
		
		public override void ShowTweetComposer (string _message, string _URL, byte[] _imgByteArray, TWTRTweetCompletion _onCompletion)
		{
			base.ShowTweetComposer(_message, _URL, _imgByteArray, _onCompletion);

			// Feature isnt supported
			Console.LogError(Constants.kDebugTag, Constants.kNotSupportedInEditor);

			// Associated error event is raised
			TweetComposerDismissed(((int)eTwitterComposerResult.CANCELLED).ToString());
		}
		
		#endregion
		
		#region Request API's
		
		public override void RequestAccountDetails (TWTRAccountDetailsCompletion _onCompletion)
		{			
			base.RequestAccountDetails(_onCompletion);

			// Associated error event is raised
			RequestAccountDetailsFailed(Constants.kNotSupportedInEditor);
		}
		
		public override void RequestEmailAccess (TWTREmailAccessCompletion _onCompletion)
		{
			base.RequestEmailAccess(_onCompletion);

			// Associated error event is raised
			RequestEmailAccessFailed(Constants.kNotSupportedInEditor);
		}
		
		protected override void URLRequest (string _methodType, string _URL, IDictionary _parameters, TWTRResonse _onCompletion)
		{			
			base.URLRequest(_methodType, _URL, _parameters, _onCompletion);

			// Associated error event is raised
			TwitterURLRequestFailed(Constants.kNotSupportedInEditor);
		}
		
		#endregion
	}
}
#endif