#if UNITY_EDITOR

using UnityEngine ;
using TMPro ;
using UnityEditor ;


[InitializeOnLoad]
public static class DynamicFontCleaner
{
	static DynamicFontCleaner()
	{
		EditorApplication.playModeStateChanged += OnPlayModeStateChanged ;
	}
	
	private static void OnPlayModeStateChanged( PlayModeStateChange state )
	{
		if( state == PlayModeStateChange.ExitingPlayMode )
		{
			var tmpFontAssets = Resources.FindObjectsOfTypeAll<TMPro.TMP_FontAsset>() ;
			foreach( var tmpFontAsset in tmpFontAssets )
			{
				if( tmpFontAsset != null && tmpFontAsset.atlasPopulationMode == AtlasPopulationMode.Dynamic )
				{
					tmpFontAsset.ClearFontAssetData() ;
					Debug.Log( "DynamicFontCleaner: ClearFontAssetData " + tmpFontAsset.name ) ;
				}
			}
		}
	}
}

#endif
