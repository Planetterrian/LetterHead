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
