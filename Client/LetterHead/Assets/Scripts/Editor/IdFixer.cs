using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEditor;
using VoxelBusters.NativePlugins;

public class IdFixer : MonoBehaviour
{

    [MenuItem("Letter Head/Fix Ids")]
    private static void x()
    {
        NPBinding.GameServices.SetAchievementMetadataCollection();
    }

    [MenuItem("Edit/Cleanup Missing Scripts")]
    static void CleanupMissingScripts()
    {
        for (int i = 0; i < Selection.gameObjects.Length; i++)
        {
            var gameObject = Selection.gameObjects[i];

            // We must use the GetComponents array to actually detect missing components
            var components = gameObject.GetComponents<Component>();

            // Create a serialized object so that we can edit the component list
            var serializedObject = new SerializedObject(gameObject);
            // Find the component list property
            var prop = serializedObject.FindProperty("m_Component");

            // Track how many components we've removed
            int r = 0;

            // Iterate over all components
            for (int j = 0; j < components.Length; j++)
            {
                // Check if the ref is null
                if (components[j] == null)
                {
                    Debug.Log("Removed component on "  + gameObject.name);
                    // If so, remove from the serialized component array
                    prop.DeleteArrayElementAtIndex(j - r);
                    // Increment removed count
                    r++;
                }
            }

            // Apply our changes to the game object
            serializedObject.ApplyModifiedProperties();
        }
    }

    [MenuItem("Letter Head/Generate Android IAP CSV")]
    private static void IapGen()
    {
        var products = NPSettings.Billing.Products.ToList();
        var filename = Application.dataPath + "/../AndroidIAP.csv";


        using (var file = new System.IO.StreamWriter(filename))
        {
            foreach (var billingProduct in products)
            {
            }
        }

        Debug.Log("IAP products written to " + filename);
    }
}
