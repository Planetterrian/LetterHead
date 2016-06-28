#if UNITY_ANDROID

using Google.JarResolver;
using UnityEditor;

namespace VoxelBusters.NativePlugins
{
	/// <summary>
	/// Play-Services Dependencies for Cross Platform Native Plugins.
	/// </summary>
	[InitializeOnLoad]
	public static class NPAndroidLibraryDependencies
	{
		/// <summary>
		/// The name of your plugin.  This is used to create a settings file
		/// which contains the dependencies specific to your plugin.
		/// </summary>
		private static readonly string PluginName = "GameServices";
		
		/// <summary>
		/// Initializes static members of the <see cref="SampleDependencies"/> class.
		/// </summary>
		static NPAndroidLibraryDependencies()
		{
			PlayServicesSupport svcSupport = PlayServicesSupport.CreateInstance(
				PluginName,
				EditorPrefs.GetString("AndroidSdkRoot"),
				"ProjectSettings");
			
			svcSupport.DependOn("com.google.android.gms",
			                    "play-services-games",
			                    "LATEST");
			
			// need nearby too, even if it is not used.
			svcSupport.DependOn("com.google.android.gms",
			                    "play-services-nearby",
			                    "LATEST");
			
			// Plus is needed if Token support is enabled.
			svcSupport.DependOn("com.google.android.gms",
			                    "play-services-plus",
			                    "LATEST");
            // For Notification Service          
            svcSupport.DependOn("com.google.android.gms",
                                "play-services-gcm",
                                "LATEST");
			
			// Marshmallow permissions requires app-compat
			svcSupport.DependOn("com.android.support",
			                    "support-v4",
			                    "23.1+");
		}
	}
}
#endif