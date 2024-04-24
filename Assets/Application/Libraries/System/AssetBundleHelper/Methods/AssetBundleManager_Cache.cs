using System ;
using System.Collections.Generic ;
using System.Linq ;

using UnityEngine ;


/// <summary>
/// アセットバンドルヘルパーパッケージ
/// </summary>
namespace AssetBundleHelper
{
	/// <summary>
	/// アセットバンドルマネージャクラス(メソッド)
	/// </summary>
	public partial class AssetBundleManager
	{
		/// <summary>
		/// キャッシュタイプ
		/// </summary>
		public enum CachingTypes
		{
			/// <summary>
			/// キャッシュコントロールはしない
			/// </summary>
			None			= 0,	// キャッシュしない

			[Obsolete( "Use ReferenceCount")]
			ResourceOnly	= 1,	// リソースのみキャッシャする

			[Obsolete( "Use ReferenceCount")]
			AssetBundleOnly	= 2,	// アセットバンドルのみキャッシュする

			[Obsolete( "Use ReferenceCount")]
			Same			= 3,	// リソース・アセットバンドルともにキャッシュする

			/// <summary>
			/// 参照カウント方式でキャッシュコントロールを行う
			/// </summary>
			ReferenceCount	= 4,	// 参照カウント方式でキャッシュする
		}

		/// <summary>
		/// メモリ展開アセットバンドルの破棄動作のタイプ
		/// </summary>
		public enum CacheReleaseTypes
		{
			/// <summary>
			/// 参照カウントでコントロールされていない且つ維持設定になっていないメモリ展開アセットバンドルを破棄する
			/// </summary>
			Limited,

			/// <summary>
			/// 維持設定になっていないメモリ展開アセットバンドルを破棄する
			/// </summary>
			Standard,

			/// <summary>
			/// 全てのメモリ展開アセットバンドルを破棄する
			/// </summary>
			Perfect,
		}

		// 注意：アセットバンドルから展開させるリソースのインスタンスについて
		//
		// パスは同じでも異なるアセットバンドルのインスタンスから展開された同じパスのリソースは、
		// 別のリソースとして扱われる(別のインスタンスとなる)
		// よってアセットバンドルの展開そのものと、
		// 展開されたアセットバンドルからのリソースの展開は、重々注意する必要がある。
		// (重複展開され、無駄にメモリを消費する事になる。その他にもバグの要因となる。)

		// None：展開されたリソースのインスタンスは別個
		// 　動作をきちんと理解していないと危険なタイプである。
		// 　同じ動作を２度行った際の展開されたリソースのインスタンスは別物になる。
		// 　そのシーンで１度しか使わないようなものに対してのみ使用すること。
		//
		// ResourceOnly：展開されたリソースのインスタンスは同一
		// 　展開されたリソースそのもののインスタンスは同一になるが、
		// 　リソース群が同じアセットバンドルに内包されている場合、
		// 　何度も無駄にアセットバンドルの展開が発生する事になる。
		//
		// AssetBundleOnly：展開されたリソースのインスタンスは同一
		// 　展開されたアセットバンドルのインスタンス内に、
		// 　展開されたリソースのインスタンス(実体はシステムリソースキャッシュにある)を
		// 　保持しているため、同じリソースであればリソースの再展開は行われず、
		// 　同一のリソースのリソースのインスタンスが返される。
		//
		// Same：展開されたリソースのインスタンスは同一
		// 　ResourceOnly とと同じく、リソースキャッシュに保存された、
		// 　展開されたリソースのインスタンスを返すので、
		// 　展開されたリソースのインスタンスは同一になる。

		//-----------------------------------------------------------

		/// <summary>
		/// アセット(リソース)キャッシュ
		/// </summary>
		[Serializable]
		public class ResourceCacheElement
		{
			/// <summary>
			/// アセット(リソース)のパス
			/// </summary>
			public	string											Path ;			// デバッグ表示用のパス

			//--------------

			/// <summary>
			/// キャッシュしているアセット(リソース)のインスタンス
			/// </summary>
			[NonSerialized]
			public	UnityEngine.Object								Resource ;


			/// <summary>
			/// アセット(リソース)自体の参照カウント
			/// </summary>
			public	int												ReferenceCount ;


			/// <summary>
			/// アセット(リソース)が属しているアセットバンドル
			/// </summary>
			[NonSerialized]
			public	ManifestInfo.AssetBundleCacheElement			AssetBundleCache ;

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="path"></param>
			/// <param name="resource"></param>
			public ResourceCacheElement( string path, UnityEngine.Object resource, ManifestInfo.AssetBundleCacheElement assetBundleCache )
			{
				Path				= path ;
				Resource			= resource ;
				ReferenceCount		= 1 ;	// 参照カウントは１
				AssetBundleCache	= assetBundleCache ;
			}

			/// <summary>
			/// キャッシュされているアセット(リソース)を取得する
			/// </summary>
			/// <returns></returns>
			public UnityEngine.Object Load()
			{
				// 参照カウントを増加させる
				ReferenceCount ++ ;

				return Resource ;
			}

			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
			public bool Free()
			{
				if( ReferenceCount <= 0 )
				{
					Debug.LogWarning( "参照カウントが異常です Path = " + Path ) ;
					return false ;
				}

				//---------------------------------

				// 参照カウントを減少させる
				ReferenceCount -- ;

				// 参照カウントが０になったらキャッシュから削除する必要がある
				return ( ReferenceCount == 0 ) ;
			}
		}

		/// <summary>
		/// アセット(リソース)のキャッシュ
		/// </summary>
		private Dictionary<string,ResourceCacheElement> m_ResourceCache ;

		/// <summary>
		/// アセット(リソース)キャッシュ
		/// </summary>
		internal protected Dictionary<string,ResourceCacheElement> ResourceCache
		{
			get
			{
				return m_ResourceCache ;
			}
		}

		private protected Dictionary<UnityEngine.Object,ResourceCacheElement> m_ResourceCacheDetector ;


		//-------------------------------------------------------------------------------------------

		// アセット(リソース)キャッシュを追加する
		private ResourceCacheElement AddResourceCache( string resourceCachePath, UnityEngine.Object asset, ManifestInfo.AssetBundleCacheElement assetBundleCache )
		{
			// 新規でアセット(リソース)キャッシュが生成された分、アセットバンドルの参照カウントを増加させる
			assetBundleCache?.IncrementCachingReferenceCount( 1 ) ;	// LocalAssets からロードしている場合はインスタンスは null である

			//----------------------------------

			var resourceCache = new ResourceCacheElement( resourceCachePath, asset, assetBundleCache ) ;

			m_ResourceCache.Add( resourceCachePath, resourceCache ) ;
			m_ResourceCacheDetector.Add( asset, resourceCache ) ;
#if UNITY_EDITOR
			m_ResourceCacheViewer.Add( resourceCache ) ;
#endif
			//----------------------------------

			return resourceCache ;
		}

		// アセット(リソース)キャッシュを削除する
		private void RemoveResourceCache( string resourceCachePath )
		{
			if( m_ResourceCache.ContainsKey( resourceCachePath ) == false )
			{
				// キャッシュには存在していない
				return ;
			}

			//----------------------------------

			var resourceCache = m_ResourceCache[ resourceCachePath ] ;

			m_ResourceCache.Remove( resourceCachePath ) ;
			m_ResourceCacheDetector.Remove( resourceCache.Resource ) ;
#if UNITY_EDITOR
			m_ResourceCacheViewer.Remove( resourceCache ) ;
#endif
			//----------------------------------

			// アセットバンドル側の参照カウントを減少させる
			resourceCache.AssetBundleCache?.DecrementCachingReferenceCount( 1, true ) ;	// LocalAssets からロードしている場合はインスタンスは null である
		}

		//-------------------------------------------------------------------

		/// <summary>
		/// リソースキャッシュをクリアする
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool ClearResourceCache( bool isPerfect, bool useUnloadUnusedAssets )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.ClearResourceCache_Private( isPerfect, useUnloadUnusedAssets ) ;
		}

		// リソースキャッシュをクリアする
		private bool ClearResourceCache_Private( bool isPerfect, bool useUnloadUnusedAssets )
		{
			// リソースキャッシュをクリア
			if( m_ResourceCache != null && m_ResourceCache.Count >  0 )
			{
#if UNITY_EDITOR
				Debug.Log( "<color=#FF80FF>[AssetBundleManager] キャッシュからクリア対象となる展開済みリソース数 = " + m_ResourceCache.Count + "</color>" ) ;
#endif
				// ResourceCache が AssetBundleCache に紐づいている(参照カウントで管理されている)場合は、
				// AssetBundleCache の参照カウントを下げて、AssetBundleCache の参照カウントが 0 になったら AssetBundleCache を破棄する
				foreach( var resourceCache in m_ResourceCache.Values )
				{
					// 注意：UseeLocalAssets が true である場合、ReferenceCount を使用しても AssetBundleCache が貯まる事は無い(AssetBundleCache が null のケースがある)
					resourceCache.AssetBundleCache?.DecrementCachingReferenceCount( 1, true ) ;
				}

				m_ResourceCache.Clear() ;
				m_ResourceCacheDetector.Clear() ;
#if UNITY_EDITOR
				m_ResourceCacheViewer.Clear() ;
#endif
			}

			//----------------------------------------------------------

            CacheReleaseTypes cacheReleaseType = ( isPerfect == false ? CacheReleaseTypes.Standard : CacheReleaseTypes.Perfect ) ;
            
			// アセットバンドルキャッシュをクリア
			if( m_ManifestInfo == null || m_ManifestInfo.Count >  0 )
			{
				foreach( var manifestInfo in m_ManifestInfo )
				{
					// 各マニフェストのアセットバンドルキャッシュもクリアする
					manifestInfo.ClearAssetBundleCache( cacheReleaseType ) ;
				}
			}

			//----------------------------------------------------------

			// ランタイムでのリソースキャッシュをクリア
			if( useUnloadUnusedAssets == true )
			{
#if UNITY_EDITOR
				Debug.Log( "<color=#FF80FF>[AssetBundleManager] *** Resources.UnloadUnusedAssets() が実行されました ***</color>" ) ;
#endif
				Resources.UnloadUnusedAssets() ;	// 非同期で実行する
				System.GC.Collect() ;
			}

			return true ;
		}

		//---------------------------------------------------------------------------------------------------------

		/// <summary>
		/// ローカルストレージ内のアセットバンドルファイルキャッシュをクリアする
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Cleanup( string manifestName = null )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.Cleanup_Private( manifestName ) ;
		}

		// ローカルストレージ内のアセットバンドルキャッシュをクリアする
		private  bool Cleanup_Private( string manifestName )
		{
			bool result = false ;

			if( string.IsNullOrEmpty( manifestName ) == true )
			{
				// 全マニフェストを対象とする
				if( m_ManifestInfo != null && m_ManifestInfo.Count >  0 )
				{
					foreach( var manifestInfo in m_ManifestInfo )
					{
						manifestInfo.RemoveAllFiles() ;
					}
				}

				result = true ;
			}
			else
			{
				// 指定したマニフェストのみを対象とする
				manifestName = manifestName.ToLower() ;

				var manifestInfo = m_ManifestInfo.FirstOrDefault( _ => _.ManifestName.ToLower() == manifestName ) ;
				if( manifestInfo != null )
				{
					manifestInfo.RemoveAllFiles() ;
					result = true ;
				}
				else
				{
					// 失敗
					result = false ;
				}
			}

			return result ;
		}

		//-------------------------------------------------------------------
	}
}
