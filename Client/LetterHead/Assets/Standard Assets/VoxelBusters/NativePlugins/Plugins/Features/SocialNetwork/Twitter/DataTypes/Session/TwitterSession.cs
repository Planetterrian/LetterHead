using UnityEngine;
using System.Collections;

namespace VoxelBusters.NativePlugins
{
	/// <summary>
	/// Represents an immutable object, that encapsulates the authorization details of an Twitter session.
	/// </summary>
	public class TwitterSession 
	{
		#region Properties

		/// <summary>
		/// The authorization token. (read-only)
		/// </summary>
		public string AuthToken 
		{ 
			get; 
			protected set; 
		}

		/// <summary>
		/// The authorization token secret. (read-only)
		/// </summary>
		public string AuthTokenSecret 
		{ 
			get; 
			protected set; 
		}

		/// <summary>
		/// The username associated with the access token. (read-only)
		/// </summary>
		public string UserName 
		{ 
			get; 
			protected set; 
		}

		/// <summary>
		/// The user ID associated with the access token. (read-only)
		/// </summary>
		public string UserID 
		{ 
			get; 
			protected set; 
		}
		
		#endregion
		
		#region Constructor
		
		protected TwitterSession ()
		{
			AuthToken		= string.Empty;
			AuthTokenSecret	= string.Empty;
			UserName		= string.Empty;
			UserID			= string.Empty;
		}
		
		#endregion

		#region Overriden Methods

		public override string ToString ()
		{
			return string.Format("[TwitterSession: AuthToken={0}, AuthTokenSecret={1}, UserName={2}, UserID={3}]", 
			                     AuthToken, AuthTokenSecret, UserName, UserID);
		}
		
		#endregion
	}
}