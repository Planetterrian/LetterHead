using UnityEngine;
using System.Collections;

namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class GameServicesSettings
	{
		[System.Serializable]
		public class AndroidSettings 
		{
			#region Fields

			[SerializeField]
			[NotifyNPSettingsOnValueChange]
			[Tooltip ("Your application id in Google Play services.")]
			private 	string		m_playServicesApplicationID;
			[SerializeField]
			[Tooltip ("String formats used to derive completed achievement description. Note: Achievement title will be inserted in place of token \'#\'.")]
			private 	string[]	m_achievedDescriptionFormats = new string[] {
				"Awesome! Achievement # completed."
			};

			#endregion

			#region Properties

			internal string PlayServicesApplicationID
			{
				get
				{
					return m_playServicesApplicationID;
				}
			}

			internal string[] AchievedDescriptionFormats
			{
				get
				{
					return m_achievedDescriptionFormats;
				}
			}

			#endregion
		}
	}
}