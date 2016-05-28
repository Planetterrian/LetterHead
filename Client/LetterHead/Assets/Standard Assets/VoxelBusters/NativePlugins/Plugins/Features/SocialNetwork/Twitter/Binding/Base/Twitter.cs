using UnityEngine;
using System.Collections;

#if USES_TWITTER
using VoxelBusters.Utility;
using VoxelBusters.DebugPRO;

namespace VoxelBusters.NativePlugins
{
	using Internal;

	/// <summary>
	/// Provides cross-platform interface to Twitter login and sending requests on the behalf of the user. Additionally, it includes methods to compose and send Tweet messages.
	/// </summary>
	/// <description>
	///	You will need a consumer key and a consumer secret to use Twitter SDK within your application.
	/// And for this, please follow the instructions provided by <a href="https://docs.fabric.io/ios/twitter/configure-twitter-app.html">Twitter</a> and set it in NPSettings.
	/// </description>
	public partial class Twitter : MonoBehaviour 
	{
		#region Fields

		private		bool		m_lastTweetWasTextOnly;
		private		bool		m_isInitialised;

		#endregion

		#region Init API

		/// <summary>
		/// Initialises the SDK with the credentials set in NPSettings.
		/// </summary>
		/// <returns><c>true</c> if SDK got initialised; otherwise, <c>false</c>.</returns>
		///	<remarks> 
		/// \note You need to call this method, before using any features. 
		/// This method requires that you have set up your consumerKey and consumerSecret in NPSettings. 
		/// </remarks>
		public virtual bool Initialise ()
		{
			TwitterSettings _twitterSettings	= NPSettings.SocialNetworkSettings.TwitterSettings;
			
			if (string.IsNullOrEmpty(_twitterSettings.ConsumerKey) || string.IsNullOrEmpty(_twitterSettings.ConsumerSecret))
			{
				Console.LogError(Constants.kDebugTag, "[Twitter] Twitter initialize failed. Please configure Consumer Key and Consumer Secret in NPSettings.");
				m_isInitialised = false;
			}
			else
			{
				m_isInitialised = true;
			}

			return m_isInitialised;
		}

		protected bool IsInitialised()
		{
			return m_isInitialised;
		}

		#endregion
		
		#region Account API's

		/// <summary>
		/// Authenticates the app user with Twitter.
		/// </summary>
		/// <description>
		/// This method falls back to presenting an OAuth flow, if it fails to find saved login credentials.
		/// </description>
		/// <param name="_onCompletion">Callback that will be called after operation is completed.</param>
		/// <remarks>
		/// \note User authentication is required for API requests that require a user context, for example: Tweeting or following other users.
		/// </remarks>
		/// <example>
		/// The following code shows how to use login method.
		/// <code>
		/// using UnityEngine;
		/// using System.Collections;
		/// using VoxelBusters.NativePlugins;
		/// 
		/// public class ExampleClass : MonoBehaviour 
		/// {
		/// 	public void Login ()
		/// 	{
		/// 		NPBinding.Twitter.Login(LoginFinished);
		/// 	}
		/// 
		/// 	private void OnLoginFinished (TwitterSession _session, string _error)
		/// 	{
		/// 		if (_error == null)
		/// 		{
		/// 			// Logged in successfully
		/// 		}
		/// 		else
		/// 		{
		/// 			// Login failed
		/// 		}
		/// 	}
		/// }
		/// </code>
		/// </example>
		public virtual void Login (TWTRLoginCompletion _onCompletion)
		{
			// Pause unity player
			this.PauseUnity();

			// Cache callback
			OnLoginFinished	= _onCompletion;

#if USES_SOOMLA_GROW
			NPBinding.SoomlaGrowService.ReportOnSocialLoginStarted(eSocialProvider.TWITTER);
#endif
		}

		/// <summary>
		/// Deletes the Twitter user session from this app.
		/// </summary>
		public virtual void Logout ()
		{
#if USES_SOOMLA_GROW
			NPBinding.SoomlaGrowService.ReportOnSocialLogoutStarted(eSocialProvider.TWITTER);
			NPBinding.SoomlaGrowService.ReportOnSocialLogoutFinished(eSocialProvider.TWITTER);
#endif
		}

		/// <summary>
		/// Determines whether user is currently logged in to Twitter.
		/// </summary>
		/// <returns><c>true</c> if user is currently logged into Twitter; otherwise, <c>false</c>.</returns>
		public virtual bool IsLoggedIn ()
		{
			bool _isLoggedIn	= false;
			Console.Log(Constants.kDebugTag, "[Twitter] IsLoggedIn=" + _isLoggedIn);
			
			return _isLoggedIn;
		}

		/// <summary>
		/// Returns the authorization token of the current user session.
		/// </summary>
		/// <returns>The authorization token.</returns>
		/// <remarks>
		/// \note Returns <c>null</c> if there is no logged in user.
		/// </remarks>
		public virtual string GetAuthToken ()
		{
			string _authToken	= null;
			Console.Log(Constants.kDebugTag, "[Twitter] AuthToken=" + _authToken);

			return _authToken;
		}

		/// <summary>
		/// Returns the authorization token secret of the current user session.
		/// </summary>
		/// <returns>The authorization token secret.</returns>
		/// <remarks>
		/// \note Returns <c>null</c> if there is no logged in user.
		/// </remarks>
		public virtual string GetAuthTokenSecret ()
		{
			string _authTokenSecret	= null;
			Console.Log(Constants.kDebugTag, "[Twitter] AuthTokenSecret=" + _authTokenSecret);
			
			return _authTokenSecret;
		}

		/// <summary>
		/// Returns the user ID associated with the access token.
		/// </summary>
		/// <returns>The user ID associated with the access token.</returns>
		/// <remarks>
		/// \note Returns <c>null</c> if there is no logged in user.
		/// </remarks>
		public virtual string GetUserID ()
		{
			string _userID	= null;
			Console.Log(Constants.kDebugTag, "[Twitter] UserID=" + _userID);
			
			return _userID;
		}

		/// <summary>
		/// Returns the username associated with the access token.
		/// </summary>
		/// <returns>The username associated with the access token.</returns>
		/// <remarks>
		/// \note Returns <c>null</c> if there is no logged in user.
		/// </remarks>
		public virtual string GetUserName ()
		{
			string _userName	= null;
			Console.Log(Constants.kDebugTag, "[Twitter] UserName=" + _userName);
			
			return _userName;
		}
		
		#endregion
		
		#region Tweet API's

		/// <summary>
		/// Shows a view to compose and send Tweet message.
		/// </summary>
		/// <param name="_message">The initial text for the Tweet.</param>
		/// <param name="_onCompletion">Callback that will be called after operation is completed.</param>
		public void ShowTweetComposerWithMessage (string _message, TWTRTweetCompletion _onCompletion)
		{
			ShowTweetComposer(_message, null, null, _onCompletion);
		}
		
		/// <summary>
		/// Shows a view to compose and send Tweet message.
		/// </summary>
		/// <param name="_message">The initial text for the Tweet.</param>
		/// <param name="_onCompletion">Callback that will be called after operation is completed.</param>
		public void ShowTweetComposerWithScreenshot (string _message, TWTRTweetCompletion _onCompletion)
		{
			// First capture screenshot, once its done tweet about it
			StartCoroutine(TextureExtensions.TakeScreenshot((_texture)=>{
				
				ShowTweetComposerWithImage(_message, _texture, _onCompletion);
			}));
		}

		/// <summary>
		/// Shows a view to compose and send Tweet message.
		/// </summary>
		/// <param name="_message">The initial text for the Tweet.</param>
		/// <param name="_texture">Unity texture object that has to be shared in the Tweet.</param>
		/// <param name="_onCompletion">Callback that will be called after operation is completed.</param>
		public void ShowTweetComposerWithImage (string _message, Texture2D _texture, TWTRTweetCompletion _onCompletion)
		{
			byte[] _imgByteArray	= null;

			// Convert texture into byte array
			if (_texture != null)
			{
				_imgByteArray = _texture.EncodeToPNG();
			}
			else
			{
				Console.LogWarning(Constants.kDebugTag, "[Twitter] Showing tweet composer with message only, texure is null");
			}

			// Show tweet composer
			ShowTweetComposer(_message, null, _imgByteArray, _onCompletion);
		}

		/// <summary>
		/// Shows a view to compose and send Tweet message.
		/// </summary>
		/// <param name="_message">The initial text for the Tweet.</param>
		/// <param name="_URL">URL that has to be shared in the Tweet.</param>
		/// <param name="_onCompletion">Callback that will be called after operation is completed.</param>
		public void ShowTweetComposerWithLink (string _message, string _URL, TWTRTweetCompletion _onCompletion)
		{
			if (string.IsNullOrEmpty(_URL))
			{
				Console.LogWarning(Constants.kDebugTag, "[Twitter] Showing tweet composer with message only, URL is null/empty");
			}

			// Show tweet composer
			ShowTweetComposer(_message, _URL, null, _onCompletion);
		}
		
		/// <summary>
		/// Shows a view to compose and send Tweet message.
		/// </summary>
		/// <param name="_message">The initial text for the Tweet.</param>
		/// <param name="_URL">URL that has to be shared in the Tweet.</param>
		/// <param name="_imgByteArray">Raw image data that has to be shared in the Tweet.</param>
		/// <param name="_onCompletion">Callback that will be called after operation is completed.</param>
		public virtual void ShowTweetComposer (string _message, string _URL, byte[] _imgByteArray, TWTRTweetCompletion _onCompletion)
		{
			// Pause unity player
			this.PauseUnity();

			// Cache callback
			OnTweetComposerClosed		=	_onCompletion;

#if USES_SOOMLA_GROW
			bool	_tweetingMessage	= (_URL == null && _imgByteArray == null);
			eSocialActionType	_actionType	= _tweetingMessage ? eSocialActionType.UPDATE_STATUS : eSocialActionType.UPDATE_STORY;

			// Cache information
			m_lastTweetWasTextOnly		= _tweetingMessage;

			// Report this event
			NPBinding.SoomlaGrowService.ReportOnSocialActionStarted(_actionType, eSocialProvider.TWITTER);
#endif
		}

		#endregion

		#region Request API's

		/// <summary>
		/// Requests access to the current Twitter user account details.
		/// </summary>
		/// <param name="_onCompletion">Callback that will be called after operation is completed.</param>
		/// <example>
		/// The following code snippet shows how to fetch account details of current user.
		/// <code>
		/// using UnityEngine;
		/// using System.Collections;
		/// using VoxelBusters.NativePlugins;
		/// 
		/// public class ExampleClass : MonoBehaviour 
		/// {
		/// 	public void FetchAccountDetails ()
		/// 	{
		/// 		NPBinding.Twitter.RequestAccountDetails(OnRequestFinished);
		/// 	}
		/// 
		/// 	private void OnRequestFinished (TwitterUser _user, string _error)
		/// 	{
		/// 		if (_error == null)
		/// 		{
		/// 			Debug.Log("Logged in user name is: " + _user.Name);
		/// 		}
		/// 		else
		/// 		{
		/// 			// Something went wrong
		/// 		}
		/// 	}
		/// }
		/// </code>
		/// </example>
		public virtual void RequestAccountDetails (TWTRAccountDetailsCompletion _onCompletion)
		{
			// Cache callback
			OnRequestAccountDetailsFinished	= _onCompletion;
		}

		/// <summary>
		/// Requests access to the email address associated with current Twitter user.
		/// </summary>
		/// <param name="_onCompletion">Callback that will be called after operation is completed.</param>
		/// <example>
		/// The following code snippet shows how to fetch email address of current user.
		/// <code>
		/// using UnityEngine;
		/// using System.Collections;
		/// using VoxelBusters.NativePlugins;
		/// 
		/// public class ExampleClass : MonoBehaviour 
		/// {
		/// 	public void FetchEmailAddress ()
		/// 	{
		/// 		NPBinding.Twitter.RequestEmailAccess(OnRequestFinished);
		/// 	}
		/// 
		/// 	private void OnRequestFinished (string _email, string _error)
		/// 	{
		/// 		if (_error == null)
		/// 		{
		/// 			Debug.Log("Email id: " + _email);
		/// 		}
		/// 		else
		/// 		{
		/// 			// Something went wrong
		/// 		}
		/// 	}
		/// }
		/// </code>
		/// </example>
		public virtual void RequestEmailAccess (TWTREmailAccessCompletion _onCompletion)
		{
			// Pause unity player
			this.PauseUnity();

			// Cache callback
			OnRequestEmailAccessFinished	= _onCompletion;
		}

		/// <summary>
		/// Sends a signed Twitter Get request.
		/// </summary>
		/// <param name="_URL">Request URL. This is the full Twitter API URL. E.g. https://api.twitter.com/1.1/statuses/user_timeline.json.</param>
		/// <param name="_parameters">Request parameters.</param>
		/// <param name="_onCompletion">Callback that will be called after operation is completed.</param>
		/// <example>
		/// The following code shows how to send request to Twitter.
		/// <code>
		/// using UnityEngine;
		/// using System.Collections;
		/// using System.Collections.Generic;
		/// using VoxelBusters.NativePlugins;
		/// 
		/// public class ExampleClass : MonoBehaviour 
		/// {
		/// 	public void SendRequest ()
		/// 	{
		/// 		string 		_URL 	= "https://api.twitter.com/1.1/statuses/show.json";
		/// 		IDictionary _params = new Dictionary<string, string>(){
		/// 			{"id", "20"}
		/// 		};
		/// 		
		/// 		NPBinding.Twitter.GetURLRequest(_URL, _params, OnRequestFinished);
		/// 	}
		/// 
		/// 	private void OnRequestFinished (object _responseData, string _error)
		/// 	{
		/// 		if (_error == null)
		/// 		{
		/// 			IDictionary	_JSONDict	= (IDictionary)_responseData;
		/// 
		/// 			// Extract values from JSON object
		/// 		}
		/// 		else
		/// 		{
		/// 			// Something went wrong
		/// 		}
		/// 	}
		/// }
		/// </code>
		/// </example>
		public void GetURLRequest (string _URL,	IDictionary _parameters, TWTRResonse _onCompletion)
		{
			URLRequest("GET", _URL, _parameters, _onCompletion);
		}

		/// <summary>
		/// Sends a signed Twitter Post request.
		/// </summary>
		/// <param name="_URL">Request URL. This is the full Twitter API URL. E.g. https://api.twitter.com/1.1/statuses/user_timeline.json.</param>
		/// <param name="_parameters">Request parameters.</param>
		/// <param name="_onCompletion">Callback that will be called after operation is completed.</param>
		public void PostURLRequest (string _URL,	IDictionary _parameters, TWTRResonse _onCompletion)
		{
			URLRequest("POST", _URL, _parameters, _onCompletion);
		}

		/// <summary>
		/// Sends a signed Twitter Put request.
		/// </summary>
		/// <param name="_URL">Request URL. This is the full Twitter API URL. E.g. https://api.twitter.com/1.1/statuses/user_timeline.json.</param>
		/// <param name="_parameters">Request parameters.</param>
		/// <param name="_onCompletion">Callback that will be called after operation is completed.</param>
		public void PutURLRequest (string _URL,	IDictionary _parameters, TWTRResonse _onCompletion)
		{
			URLRequest("PUT", _URL, _parameters, _onCompletion);
		}

		/// <summary>
		/// Sends a signed Twitter Delete request.
		/// </summary>
		/// <param name="_URL">Request URL. This is the full Twitter API URL. E.g. https://api.twitter.com/1.1/statuses/user_timeline.json.</param>
		/// <param name="_parameters">Request parameters.</param>
		/// <param name="_onCompletion">Callback that will be called after operation is completed.</param>
		public void DeleteURLRequest (string _URL,	IDictionary _parameters, TWTRResonse _onCompletion)
		{
			URLRequest("DELETE", _URL, _parameters, _onCompletion);
		}
		
		protected virtual void URLRequest (string _methodType, string _URL,	IDictionary _parameters, TWTRResonse _onCompletion)
		{
			// Cache callback
			OnTwitterURLRequestFinished	= _onCompletion;
		}

		#endregion
	}
}
#endif