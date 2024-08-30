using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.U2D ;

#if UNITY_EDITOR
using UnityEditor ;
using UnityEditorInternal ;
#endif


namespace SpriteHelper
{
	/// <summary>
	/// スプライト制御クラス  Version 2024/08/16
	/// </summary>
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[RequireComponent( typeof( SpriteDrawer ) )]
	public partial class SpriteImage : SpriteFrame
	{
#if UNITY_EDITOR
		/// <summary>
		/// Sprite を生成
		/// </summary>
		[MenuItem( "GameObject/SpriteHelper/SpriteImage", false, 22 )]	// メニューから
		[MenuItem( "SpriteHelper/Add a SpriteImage" )]					// ポップアップメニューから
		public static void CreateSpriteImage()
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child SpriteImage" ) ;	// アンドウバッファに登録

			var child = new GameObject( "SpriteImage" ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var component = child.AddComponent<SpriteImage>() ;
			component.SetDefault() ;	// 初期状態に設定する

			// 一番上に移動させる
			while( ComponentUtility.MoveComponentUp( component ) ){}

			if( component.TryGetComponent<SpriteDrawer>( out var spriteDrawer ) == true )
			{
				// SpriteDrawer を一番上に移動させる
				while( ComponentUtility.MoveComponentUp( spriteDrawer ) ){}
			}

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		private static bool WillLosePrefab( GameObject root )
		{
			if( root == null )
			{
				return false ;
			}

			if( root.transform != null )
			{
				PrefabAssetType type = PrefabUtility.GetPrefabAssetType( root ) ;

				if( type != PrefabAssetType.NotAPrefab )
				{
					return EditorUtility.DisplayDialog( "Losing prefab", "This action will lose the prefab connection. Are you sure you wish to continue?", "Continue", "Cancel" ) ;
				}
			}
			return true ;
		}
#endif
		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 動的生成された際にデフォルト状態を設定する
		/// </summary>
		public override void SetDefault()
		{
			base.SetDefault() ;

			//-----------------------------------------------------------

			// 予めキャッシュしておく
			_ = CSpriteDrawer ;
		}

		//-------------------------------------------------------------------------------------------

		// インスタンスキャッシュ
		[SerializeField][HideInInspector]
		protected SpriteDrawer	m_SpriteDrawer ;

		/// <summary>
		/// キャッシュされた SpriteDrawer を取得する
		/// </summary>
		public	SpriteDrawer	CSpriteDrawer
		{
			get
			{
				if( m_SpriteDrawer == null )
				{
					TryGetComponent<SpriteDrawer>( out m_SpriteDrawer ) ;
				}
				return m_SpriteDrawer ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 動作有無のショートカット
		/// </summary>
		public bool Enabled
		{
			get
			{
				return CSpriteDrawer != null && CSpriteDrawer.enabled ;
			}
			set
			{
				if( CSpriteDrawer != null )
				{
					CSpriteDrawer.enabled = value ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------
		// SpriteAtlas 限定
		
		/// <summary>
		/// アトラススプライトのインスタンス
		/// </summary>
		public  SpriteAtlas  SpriteAtlas
		{
			get
			{
				if( CSpriteDrawer == null )
				{
					return null ;
				}

				return CSpriteDrawer.SpriteAtlas ;
			}
			set
			{
				if( CSpriteDrawer == null )
				{
					return ;
				}

				CSpriteDrawer.SpriteAtlas = value ;
			}
		}

		//-------------------------------------------------------------------------------------------
		// SpriteSet 限定

		/// <summary>
		/// スプライトセットのインスタンス
		/// </summary>
		public  SpriteSet  SpriteSet
		{
			get
			{
				if( CSpriteDrawer == null )
				{
					return null ;
				}

				return CSpriteDrawer.SpriteSet ;
			}
			set
			{
				if( CSpriteDrawer == null )
				{
					return ;
				}

				CSpriteDrawer.SpriteSet = value ;
			}
		}

		/// <summary>
		/// アトラススプライトの要素となるスプライト群を設定する
		/// </summary>
		/// <param name="sprites"></param>
		/// <returns></returns>
		public bool SetSprites( Sprite[] sprites )
		{
			if( CSpriteDrawer == null )
			{
				return false ;
			}

			return CSpriteDrawer.SetSprites( sprites ) ;
		}

		//-------------------------------------------------------------------------------------------


		/// <summary>
		/// アトラススプライト内のスプライトを表示する
		/// </summary>
		/// <param name="spriteName">スプライト名</param>
		/// <param name="resize">画像のサイズに合わせてリサイズを行うかどうか</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public bool SetSpriteInAtlas( string spriteName, bool resize = false )
		{
			if( CSpriteDrawer == null )
			{
				return false ;
			}

			return CSpriteDrawer.SetSpriteInAtlas( spriteName, resize ) ;
		}

		/// <summary>
		/// アトラススプライト内のスプライトを取得する
		/// </summary>
		/// <param name="spriteName">スプライト名</param>
		/// <returns>スプライトのインスタンス</returns>
		public Sprite GetSpriteInAtlas( string spriteName )
		{
			if( CSpriteDrawer == null )
			{
				return null ;
			}

			return CSpriteDrawer.GetSpriteInAtlas( spriteName ) ;
		}

		/// <summary>
		/// 全てのスプライトを取得する
		/// </summary>
		/// <param name="spriteName">スプライト名</param>
		/// <returns>スプライトのインスタンス</returns>
		public Sprite[] GetSprites()
		{
			if( CSpriteDrawer == null )
			{
				return null ;
			}

			return CSpriteDrawer.GetSprites() ;
		}

		/// <summary>
		/// 全てのスプライトの識別名を取得する
		/// </summary>
		/// <returns></returns>
		public string[] GetSpriteNames()
		{
			if( CSpriteDrawer == null )
			{
				return null ;
			}

			return CSpriteDrawer.GetSpriteNames() ;
		}

		/// <summary>
		/// スプライトの数を取得する
		/// </summary>
		/// <returns></returns>
		public int GetSpriteCount()
		{
			if( CSpriteDrawer == null )
			{
				return 0 ;
			}

			return CSpriteDrawer.GetSpriteCount() ;
		}

		/// <summary>
		/// アトラススプライト内のスプライトの横幅を取得する
		/// </summary>
		/// <param name="spriteName">スプライト名</param>
		/// <returns>横幅</returns>
		public int GetWidthOfSpriteInAtlas( string spriteName )
		{
			if( CSpriteDrawer == null )
			{
				return 0 ;
			}

			return CSpriteDrawer.GetWidthOfSpriteInAtlas( spriteName ) ;
		}

		/// <summary>
		/// アトラススプライト内のスプライトの縦幅を取得する
		/// </summary>
		/// <param name="spriteName">スプライト名</param>
		/// <returns>縦幅</returns>
		public int GetHeightOfSpriteInAtlas( string spriteName )
		{
			if( CSpriteDrawer == null )
			{
				return 0 ;
			}

			return CSpriteDrawer.GetHeightOfSpriteInAtlas( spriteName ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// １６進数値で色を設定する
		/// </summary>
		/// <param name="color"></param>
		public void SetColor( uint color )
		{
			byte r = ( byte )( ( color >> 16 ) & 0xFF ) ;
			byte g = ( byte )( ( color >>  8 ) & 0xFF ) ;
			byte b = ( byte )( ( color       ) & 0xFF ) ;
			byte a = ( byte )( ( color >> 24 ) & 0xFF ) ;

			Color = new Color32( r, g, b, a ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// スプライト(ショートカット)
		/// </summary>
		public Sprite Sprite
		{
			get
			{
				return CSpriteDrawer != null ? CSpriteDrawer.Sprite : null ;
			}
			set
			{
				if( CSpriteDrawer != null )
				{
					CSpriteDrawer.Sprite = value ;
				}
			}
		}
	
		/// <summary>
		/// カラー
		/// </summary>
		public Color Color
		{
			get
			{
				return CSpriteDrawer != null ? CSpriteDrawer.VertexColor : Color.white ;
			}
			set
			{
				if( CSpriteDrawer != null )
				{
					CSpriteDrawer.VertexColor = value ;
				}
			}
		}

		/// <summary>
		/// アルファ
		/// </summary>
		public float Alpha
		{
			get
			{
				return CSpriteDrawer != null ? 0 : CSpriteDrawer.VertexColor.a ;
			}
			set
			{
				if( CSpriteDrawer != null )
				{
					var color = CSpriteDrawer.VertexColor ;
					color.a = value ;
					CSpriteDrawer.VertexColor = color ;
				}
			}
		}

		/// <summary>
		/// マテリアル(ショートカット)
		/// </summary>
		public Material Material
		{
			get
			{
				if( CSpriteDrawer == null )
				{
					return null ;
				}

				return CSpriteDrawer.DuplicatedMaterial ;
			}
			set
			{
				if( CSpriteDrawer == null )
				{
					return ;
				}

				CSpriteDrawer.Material = value ;
			}
		}

		/// <summary>
		/// マテリアルカラー
		/// </summary>
		public Color MaterialColor
		{
			get
			{
				if( CSpriteDrawer == null )
				{
					return Color.white ;
				}

				return CSpriteDrawer.MaterialColor ;
			}
			set
			{
				if( CSpriteDrawer == null )
				{
					return ;
				}

				CSpriteDrawer.MaterialColor = value ;
			}
		}

		/// <summary>
		/// マテリアルアルファ
		/// </summary>
		public float MaterialAlpha
		{
			get
			{
				if( CSpriteDrawer == null )
				{
					return 1 ;
				}

				return CSpriteDrawer.MaterialColor.a ;
			}
			set
			{
				if( CSpriteDrawer == null )
				{
					return ;
				}

				var color = CSpriteDrawer.MaterialColor ;
				color.a = value ;
				CSpriteDrawer.MaterialColor = color ;
			}
		}

		/// <summary>
		/// 左右反転
		/// </summary>
		public bool FlipX
		{
			get
			{
				return CSpriteDrawer != null && CSpriteDrawer.FlipX ;
			}
			set
			{
				if( CSpriteDrawer != null )
				{
					 CSpriteDrawer.FlipX = value ;
				}
			}
		}

		/// <summary>
		/// 上下反転
		/// </summary>
		public bool FlipY
		{
			get
			{
				return CSpriteDrawer != null && CSpriteDrawer.FlipY ;
			}
			set
			{
				if( CSpriteDrawer != null )
				{
					 CSpriteDrawer.FlipY = value ;
				}
			}
		}

		/// <summary>
		/// 横幅
		/// </summary>
		public float Width
		{
			get
			{
				return CSpriteDrawer != null ? CSpriteDrawer.Size.x : 0 ;
			}
			set
			{
				if( CSpriteDrawer != null )
				{
					 CSpriteDrawer.Size = new Vector2( value,  CSpriteDrawer.Size.y ) ;
				}
			}
		}

		/// <summary>
		/// 縦幅
		/// </summary>
		public float Height
		{
			get
			{
				return CSpriteDrawer != null ? CSpriteDrawer.Size.y : 0 ;
			}
			set
			{
				if( CSpriteDrawer != null )
				{
					 CSpriteDrawer.Size = new Vector2( CSpriteDrawer.Size.x, value ) ;
				}
			}
		}
#if false
		/// <summary>
		/// マスクインタラクション(スプライトの形にくり抜き制御・スプライトの内側か外側)
		/// </summary>
		public SpriteMaskInteraction MaskInteraction
		{
			get
			{
				return CSpriteRenderer != null ? CSpriteRenderer.maskInteraction : SpriteMaskInteraction.None ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					 CSpriteRenderer.maskInteraction = value ;
				}
			}
		}
#endif
#if false
		/// <summary>
		/// ソートポイント(スプライト同士が同じ SortingLayer OrderInLayer であった時に、どこを基準にソートするか : Center = Center の Y 値が小さい方が手前・Pivot = Pivot の Y 値が小さい方が手前)
		/// </summary>
		public SpriteSortPoint SortPoint
		{
			get
			{
				return CSpriteRenderer != null ? CSpriteRenderer.spriteSortPoint : SpriteSortPoint.Center ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					 CSpriteRenderer.spriteSortPoint = value ;
				}
			}
		}
#endif
		/// <summary>
		/// ソーティングレイヤー名
		/// </summary>
		public string SortingLayerName
		{
			get
			{
				if( CSpriteDrawer == null )
				{
					return null ;
				}

				return CSpriteDrawer.SortingLayerName ;
			}
			set
			{
				if( CSpriteDrawer == null )
				{
					return ;
				}

				CSpriteDrawer.SortingLayerName = value ;
			}
		}

		/// <summary>
		/// ソーティングレイヤー値
		/// </summary>
		public int SortingLayer
		{
			get
			{
				if( CSpriteDrawer == null )
				{
					return -1 ;
				}

				return CSpriteDrawer.SortingLayer ;
			}
			set
			{
				if( CSpriteDrawer == null )
				{
					return ;
				}

				CSpriteDrawer.SortingLayer = value ;
			}
		}

		/// <summary>
		/// ソーティングレイヤー(オーダーインレイヤーと同じ)
		/// </summary>
		public int SortingOrder
		{
			get
			{
				if( CSpriteDrawer == null )
				{
					return 0 ;
				}

				return CSpriteDrawer.SortingOrder ;
			}
			set
			{
				if( CSpriteDrawer == null )
				{
					return ;
				}

				CSpriteDrawer.SortingOrder = value ;
			}
		}

		/// <summary>
		/// オーダーインレイヤー
		/// </summary>
		public int OrderInLayer
		{
			get
			{
				if( CSpriteDrawer == null )
				{
					return 0 ;
				}

				return CSpriteDrawer.SortingOrder ;
			}
			set
			{
				if( CSpriteDrawer == null )
				{
					return ;
				}

				CSpriteDrawer.SortingOrder = value ;
			}
		}

		/// <summary>
		/// スプライトを設定する
		/// </summary>
		/// <param name="sprite">スプライトのインスタンス</param>
		public void SetSprite( Sprite sprite, bool resize = false )
		{
			if( resize == false )
			{
				Sprite = sprite ;
			}
			else
			{
//				SetSpriteAndResize( sprite ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		internal void Awake()
		{
			if( m_SpriteDrawer == null )
			{
				TryGetComponent<SpriteDrawer>( out m_SpriteDrawer ) ;
			}
		}

//		internal void OnEnable(){}

//		internal void OnDisable(){}

//		internal void OnDestroy(){}

		//-------------------------------------------------------------------------------------------

		// 再生中のアニメーション名
		[SerializeField]
		protected string	m_PlayingAnimationName = "Default" ;

		/// <summary>
		/// 再生中のアニメーション名
		/// </summary>
		public string PlayingAnimationName
		{
			get
			{
				return m_PlayingAnimationName ;
			}
			set
			{
				m_PlayingAnimationName = value ;
			}
		}

		// ループするかどうか
		[SerializeField]
		protected bool		m_IsAnimationLooping = true ;

		/// <summary>
		/// ループするかどうか
		/// </summary>
		public bool IsAnimationLooping
		{
			get
			{
				return m_IsAnimationLooping ;
			}
			set
			{
				m_IsAnimationLooping = value ;
			}
		}

		// 速度係数
		[SerializeField]
		protected float		m_AnimationSpeed = 1.0f ;

		/// <summary>
		/// 速度係数
		/// </summary>
		public float AnimationSpeed
		{
			get
			{
				return m_AnimationSpeed ;
			}
			set
			{
				m_AnimationSpeed = value ;
			}
		}


		// 自動でアニメーションを再生するかどうか
		[SerializeField]
		protected bool		m_AnimationPlayOnAwake = true ;

		/// <summary>
		/// 自動でアニメーションを再生するかどうか
		/// </summary>
		public bool AnimationPlayOnAwake
		{
			get
			{
				return m_AnimationPlayOnAwake ;
			}
			set
			{
				m_AnimationPlayOnAwake = value ;
			}
		}


		// 再生中かどうか
		private bool		m_IsAnimationPlaying ;

		/// <summary>
		/// 再生中かどうか
		/// </summary>
		public  bool		IsAnimationPlaying
		{
			get
			{
				return m_IsAnimationPlaying ;
			}
			set
			{
				m_IsAnimationPlaying = value ;
			}
		}


		// アニメーションの再生が終了したら呼び出されるコールバック
		private Action<string>	m_OnAimationFinished ;

		/// <summary>
		/// アニメーション終了時に呼び出すコールバックを登録する(ただしワンショット再生時のみ)
		/// </summary>
		/// <param name="OnAninationFinished"></param>
		public void SetOnAninationFinished( Action<string> OnAninationFinished )
		{
			m_OnAimationFinished = OnAninationFinished ;
		}

		//---------------

		/// <summary>
		/// アニメーション定義
		/// </summary>
		[Serializable]
		public class AnimationDescriptor
		{
			/// <summary>
			/// コンストラクタ
			/// </summary>
			public AnimationDescriptor()
			{
				Reset() ;
			}

			/// <summary>
			/// 初期化する
			/// </summary>
			public void Reset()
			{
				Name = "Default" ;
			}

			/// <summary>
			/// 識別名
			/// </summary>
			public string Name = "Default" ;

			/// <summary>
			/// アニメーションの種類ごとの速度係数
			/// </summary>
			public float  Speed = 1.0f ;

			/// <summary>
			/// フレーム情報
			/// </summary>
			[Serializable]
			public class FrameDescriptor
			{
				/// <summary>
				/// スプライト識別名
				/// </summary>
				public string SpriteName ;

				/// <summary>
				/// 表示時間
				/// </summary>
				public float Duration ;
			}

			/// <summary>
			/// フレームで表示するスプライト名
			/// </summary>
			public List<FrameDescriptor>	Frames ;

			//----------------------------------------------------------

			/// <summary>
			/// フレーム群を全て消去する
			/// </summary>
			public void ClearAllFrames()
			{
				Frames = null ;
			}

			/// <summary>
			/// フレーム群を設定する
			/// </summary>
			/// <param name="frames"></param>
			public void SetFrames( IEnumerable<( string spriteName, float duration )> frames )
			{
				if( Frames == null )
				{
					Frames = new List<FrameDescriptor>() ;
				}
				else
				{
					Frames.Clear() ;
				}

				AddFrames( frames ) ;
			}

			/// <summary>
			/// フレーム群を追加する
			/// </summary>
			/// <param name="frames"></param>
			public void AddFrames( IEnumerable<( string spriteName, float duration )> frames )
			{
				Frames ??= new List<FrameDescriptor>() ;

				foreach( var ( spriteName, duration ) in frames )
				{
					if( string.IsNullOrEmpty( spriteName ) == false && duration >  0 )
					{
						Frames.Add( new FrameDescriptor(){ SpriteName = spriteName, Duration = duration } ) ;
					}
				}
			}

			/// <summary>
			/// フレーム群を設定する
			/// </summary>
			/// <param name="frames"></param>
			public void SetFrames( IEnumerable<string> frames, float duration )
			{
				if( Frames == null )
				{
					Frames = new List<FrameDescriptor>() ;
				}
				else
				{
					Frames.Clear() ;
				}

				AddFrames( frames, duration ) ;
			}

			/// <summary>
			/// フレーム群を追加する
			/// </summary>
			/// <param name="frames"></param>
			public void AddFrames( IEnumerable<string> frames, float duration )
			{
				Frames ??= new List<FrameDescriptor>() ;

				foreach( var spriteName in frames )
				{
					if( string.IsNullOrEmpty( spriteName ) == false && duration >  0 )
					{
						Frames.Add( new FrameDescriptor(){ SpriteName = spriteName, Duration = duration } ) ;
					}
				}
			}

			/// <summary>
			/// トータル再生時間を取得する
			/// </summary>
			/// <returns></returns>
			public float GetTime()
			{
				if( Frames == null || Frames.Count == 0 )
				{
					return 0 ;
				}

				float time = 0 ;

				foreach( var frame in Frames )
				{
					time += frame.Duration ;
				}

				return time ;
			}

			/// <summary>
			/// 表示フレームのインデックス番号を取得する
			/// </summary>
			/// <param name="playingTime"></param>
			/// <param name="isLooping"></param>
			/// <returns></returns>
			public int GetFrameIndex( float playingTime, bool isLooping, out float oneShotTime )
			{
				oneShotTime = GetTime() ;
				if( oneShotTime <= 0 )
				{
					return 0 ;
				}

				//---------------------------------

				if( isLooping == true )
				{
					// ループあり
					playingTime %= oneShotTime ;
				}

				if( playingTime <  oneShotTime )
				{
					int index ;
					for( index  = 0 ; index <  Frames.Count ; index ++ )
					{
						if( playingTime <  Frames[ index ].Duration )
						{
							return index ;
						}

						playingTime -= Frames[ index ].Duration ;
					}
				}

				return Frames.Count - 1 ;
			}
		}

		// アニメーション情報群
		[SerializeField]
		protected List<AnimationDescriptor> m_Animations ;

		/// <summary>
		/// アニメーション情報群
		/// </summary>
		public List<AnimationDescriptor> Animations => m_Animations ;

		/// <summary>
		/// Json 変換用
		/// </summary>
		public class AnimationsPackage
		{
			public List<AnimationDescriptor> Animations ;
		}

		/// <summary>
		/// アニメーションを Json テキストから変換する
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		public bool ConvertAnimationsFromJson( string json )
		{
			var package = JsonUtility.FromJson<AnimationsPackage>( json ) ;
			if( package == null )
			{
				return false ;
			}

			m_Animations = package.Animations ;

			return true ;
		}

		/// <summary>
		/// アニメーションを Json テキストに変換する
		/// </summary>
		/// <returns></returns>
		public string ConvertAnimationsToJson()
		{
			var package = new AnimationsPackage()
			{
				Animations = m_Animations 
			} ;

			return JsonUtility.ToJson( package, prettyPrint:true ) ;
		}



#if UNITY_EDITOR
		[SerializeField][HideInInspector]
		private int m_AnimationCount = 0 ;

		internal void OnValidate()
		{
			if( Application.isPlaying == false )
			{
				int animationCount = 0 ;
				if( m_Animations != null && m_Animations.Count >  0 )
				{
					animationCount = m_Animations.Count ;
				}

				if( animationCount != m_AnimationCount )
				{
					if( m_AnimationCount == 0 && animationCount == 1 )
					{
						// Serializable クラスは、オブジェクトが生成された際に、ＵＩのデフォルト状態値が書き込まれてしまうため、それの回避策。(スクリプトで new を行えばデフォルト値は正しく設定される) 
						m_Animations[ 0 ].Reset() ;
					}

					m_AnimationCount = animationCount ;
				}
			}
		}
#endif

		private Dictionary<string,AnimationDescriptor> m_HashAnimations ;
		private float	m_AnimationTimer		= 0 ;
		private bool	m_AnimationAwake		= false ;	// false の場合は表示の強制更新が必要
		private int		m_AnimationIndex		= 0 ;
		private bool	m_IsAnimationPausing	= false ;

		// アニメーションの準備する
		private void PrepareAnimation()
		{
			if( m_HashAnimations == null )
			{
				m_HashAnimations = new Dictionary<string,AnimationDescriptor>() ;

				if( m_Animations != null && m_Animations.Count >  0 )
				{
					foreach( var animation in m_Animations )
					{
						if( m_HashAnimations.ContainsKey( animation.Name ) == false )
						{
							m_HashAnimations.Add( animation.Name, animation ) ;
						}
					}
				}
			}

			if( string.IsNullOrEmpty( m_PlayingAnimationName ) == true || m_HashAnimations.ContainsKey( m_PlayingAnimationName ) == false )
			{
				// 再生設定されているアニメーション名がどのアニメーション名にも該当しない場合
				// １つ以上のアニメーションが登録されていたら
				// 最初のアニメーション名に強制的に設定する
				if( m_Animations.Count >  0 )
				{
					m_PlayingAnimationName = m_Animations[ 0 ].Name ;
				}
			}
		}

		/// <summary>
		/// 全てのアニメーションを削除する
		/// </summary>
		public void ClearAllAnimations()
		{
			m_HashAnimations	= null ;
			m_Animations		= null ;
		}

		/// <summary>
		/// アニメーションを追加する
		/// </summary>
		/// <param name="animationName"></param>
		/// <param name="frames"></param>
		/// <param name=""></param>
		/// <returns></returns>
		public bool AddAnimation( string animationName, IEnumerable<( string, float )> frames )
		{
			if( string.IsNullOrEmpty( animationName ) == true || frames == null )
			{
				// 不可
				return false ;
			}

			//----------------------------------

			PrepareAnimation() ;

			// 事前に削除する
			RemoveAnimation( animationName ) ;

			//----------------------------------

			var animation = new AnimationDescriptor(){ Name = animationName } ;
			animation.SetFrames( frames ) ;

			m_Animations.Add( animation ) ;
			m_HashAnimations.Add( animationName, animation ) ;

			//----------------------------------

			return true ;
		}

		/// <summary>
		/// アニメーションを追加する
		/// </summary>
		/// <param name="animationName"></param>
		/// <param name="frames"></param>
		/// <param name=""></param>
		/// <returns></returns>
		public bool AddAnimation( string animationName, IEnumerable<string> frames, float duration )
		{
			if( string.IsNullOrEmpty( animationName ) == true || frames == null || duration <= 0 )
			{
				// 不可
				return false ;
			}

			//----------------------------------

			PrepareAnimation() ;

			// 事前に削除する
			RemoveAnimation( animationName ) ;

			//----------------------------------

			var animation = new AnimationDescriptor(){ Name = animationName } ;
			animation.SetFrames( frames, duration ) ;

			m_Animations.Add( animation ) ;
			m_HashAnimations.Add( animationName, animation ) ;

			//----------------------------------

			return true ;
		}

		/// <summary>
		/// アニメーションを削除する
		/// </summary>
		/// <param name="animationName"></param>
		/// <returns></returns>
		public bool RemoveAnimation( string animationName )
		{
			if( string.IsNullOrEmpty( animationName ) == true )
			{
				// 不可
				return false ;
			}

			//----------------------------------

			if( m_HashAnimations != null && m_HashAnimations.Count >  0 )
			{
				if( m_HashAnimations.ContainsKey( animationName ) == true )
				{
					m_HashAnimations.Remove( animationName ) ;
				}
			}

			if( m_Animations != null && m_Animations.Count >  0 )
			{
				var animations = m_Animations.Where( _ => _.Name == animationName ).ToArray() ;
				if( animations != null && animations.Length >  0 )
				{
					m_Animations.RemoveRange( animations ) ;
				}
			}

			return true ;
		}

		/// <summary>
		/// アニメーションを再生する
		/// </summary>
		/// <param name="animationName"></param>
		/// <param name="isLooping"></param>
		/// <param name="speed"></param>
		/// <param name="onFinished"></param>
		/// <returns></returns>
		public bool PlayAnimation( string animationName, Action<string> onFinished = null, bool isRestart = true )
		{
			return PlayAnimation( animationName, m_IsAnimationLooping, m_AnimationSpeed, onFinished, isRestart ) ;
		}

		/// <summary>
		/// アニメーションを再生する
		/// </summary>
		/// <param name="animationName"></param>
		/// <param name="isLooping"></param>
		/// <param name="speed"></param>
		/// <param name="onFinished"></param>
		/// <returns></returns>
		public bool PlayAnimation( string animationName, bool isLooping, Action<string> onFinished = null, bool isRestart = true )
		{
			return PlayAnimation( animationName, isLooping, m_AnimationSpeed, onFinished, isRestart ) ;
		}

		/// <summary>
		/// アニメーションを再生する
		/// </summary>
		/// <param name="animationName"></param>
		/// <param name="isLooping"></param>
		/// <param name="speed"></param>
		/// <param name="onFinished"></param>
		/// <returns></returns>
		public bool PlayAnimation( string animationName, bool isLooping, float speed, Action<string> onFinished = null, bool isRestart = true )
		{
			if( string.IsNullOrEmpty( animationName ) == true )
			{
				// 不可
				return false ;
			}

			//----------------------------------

			PrepareAnimation() ;

			if( m_HashAnimations.ContainsKey( animationName ) == false )
			{
				// 再生不可
				return false ;
			}

			//----------------------------------

			if( isRestart == false )
			{
				// 既に再生中のアニメーションと同じアニメーションの再生は無視する
				if( m_IsAnimationPlaying == true )
				{
					if( m_PlayingAnimationName == animationName )
					{
						// 既に同じアニメーションが再生中になっている
						return true ;
					}
				}
			}

			//----------------------------------

			m_PlayingAnimationName	= animationName ;
			m_IsAnimationLooping	= isLooping ;
			m_AnimationSpeed		= speed ;
			m_OnAimationFinished	= onFinished ;

			//----------------------------------

			var animation = GetAnimation( m_PlayingAnimationName ) ;
			if( animation != null )
			{
				// 再生有効
				PlayAnimation_Private() ;

				return true ;
			}

			// 再生不可
			return false ;
		}

		// アニメーションの再生を実行する
		private void PlayAnimation_Private()
		{
			m_IsAnimationPlaying	= true ;
			m_IsAnimationPausing	= false ;

			m_AnimationTimer		= 0 ;
			m_AnimationAwake		= false ;
			m_AnimationIndex		= 0 ;
		}

		/// <summary>
		/// アニメーションを停止させる
		/// </summary>
		public void StopAnimation()
		{
			m_IsAnimationPlaying = false ;
			m_IsAnimationPausing = false ;

			m_AnimationAwake	 = false ;
		}

		/// <summary>
		/// 一時停止を実行する
		/// </summary>
		public void PauseAnimation()
		{
			if( m_IsAnimationPlaying == true )
			{
				m_IsAnimationPausing = true ;
			}
		}

		/// <summary>
		/// 一時停止を終了する
		/// </summary>
		public void UnpauseAnimation()
		{
			if( m_IsAnimationPlaying == true )
			{
				m_IsAnimationPausing = false ;

				m_AnimationAwake	 = false ;
			}
		}

		/// <summary>
		/// 再生中のアニメーションの状態を取得する
		/// </summary>
		/// <param name="animationName"></param>
		/// <param name="timer"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public bool GetPlayingAnimationState( out string animationName, out float timer, out int index )
		{
			animationName	= null ;
			timer			= 0 ;
			index			= 0 ;

			if( m_IsAnimationPlaying == false )
			{
				// アニメーションは再生中ではない
				return false ;
			}

			//----------------------------------------------------------

			animationName	= m_PlayingAnimationName ;

			timer			= m_AnimationTimer ;
			index			= m_AnimationIndex ;

			return true ;
		}

		/// <summary>
		/// 再生中のアニメーションの状態を強制的に設定する
		/// </summary>
		/// <returns></returns>
		public bool SetPlayingAnimationState( string animationName, float timer, int index )
		{
			if( string.IsNullOrEmpty( animationName ) == true )
			{
				// 不可
				return false ;
			}

			//----------------------------------

			PrepareAnimation() ;

			if( m_HashAnimations.ContainsKey( animationName ) == false )
			{
				// 再生不可
				return false ;
			}

			//----------------------------------

			var animation = GetAnimation( animationName ) ;
			if( animation != null )
			{
				if( animation.Frames == null || animation.Frames.Count == 0 )
				{
					// 再生不可
					return false ;
				}

				index %= animation.Frames.Count ;

				// 再生有効
				m_PlayingAnimationName = animationName ;

				m_IsAnimationPlaying = true ;

				m_AnimationTimer = timer ;
				m_AnimationAwake = false ;
				m_AnimationIndex = index ;
				
				return true ;
			}

			// 再生不可
			return false ;
		}

		//---------------

		// 再生するアニメーションを取得する
		private AnimationDescriptor GetAnimation( string playingAnimationName )
		{
			if( m_HashAnimations != null && m_HashAnimations.Count >  0 )
			{
				if( string.IsNullOrEmpty( playingAnimationName ) == false && m_HashAnimations.ContainsKey( playingAnimationName ) == true )
				{
					var animation = m_HashAnimations[ playingAnimationName ] ;
					if( animation != null && animation.Frames != null && animation.Frames.Count >  0 )
					{
						return animation ;
					}
				}
			}

			return null ;
		}

		//-----------------------------------------------------------

		internal void Start()
		{
			if( Application.isPlaying == true )
			{
				PrepareAnimation() ;

				var animation = GetAnimation( m_PlayingAnimationName ) ;
				if( animation != null )
				{
					if( m_AnimationPlayOnAwake == true )
					{
						// 自動再生有効
						PlayAnimation_Private() ;
					}
				}
			}
		}

		private bool m_FlipX = false ;
		private bool m_FlipY = false ;

		// 毎フレーム呼び出される
		internal override void Update()
		{
			// コライダーの更新
			if( CSpriteDrawer != null && m_ColliderAdjustment == true )
			{
				if( CSpriteDrawer.FlipX != m_FlipX || CSpriteDrawer.FlipY != m_FlipY || m_ForceRefresh == true )
				{
					m_IsColliderDirty = true ;

					m_FlipX = CSpriteDrawer.FlipX ;
					m_FlipY = CSpriteDrawer.FlipY ;
				}
			}

			//----------------------------------------------------------

			base.Update() ;

			//----------------------------------------------------------

			if( Application.isPlaying == true )
			{
				var animation = GetAnimation( m_PlayingAnimationName ) ;
				if( animation != null )
				{
					if( m_IsAnimationPlaying == true )
					{
						if( m_IsAnimationPausing == false )
						{
							// ポーズ中でなければ時間を進める
							m_AnimationTimer += ( Time.deltaTime * m_AnimationSpeed * animation.Speed ) ;
						}

						int animationIndex = animation.GetFrameIndex( m_AnimationTimer, m_IsAnimationLooping, out var oneShotTime ) ;
						if( m_AnimationAwake == false || m_AnimationIndex != animationIndex || m_AnimationTimer >= oneShotTime )
						{
							m_AnimationAwake  = true ;
							m_AnimationIndex  = animationIndex ;

							var spriteName = animation.Frames[ m_AnimationIndex ].SpriteName ;

							// スプライト画像を設定する
							SetSpriteInAtlas( spriteName ) ;

							if( m_IsAnimationLooping == false )
							{
								// ループなし
								if( m_AnimationTimer >= oneShotTime )
								{
									// 再生終了
									m_OnAimationFinished?.Invoke( m_PlayingAnimationName ) ;
									m_IsAnimationPlaying = false ;
								}
							}
							else
							{
								// ループあり
								if( oneShotTime >  0 )
								{
									m_AnimationTimer %= oneShotTime ;
								}
							}
						}
					}
				}
			}
		}

		//-------------------------------------------------------------------------------------------
		// Interpolation 関係

		/// <summary>
		/// 線形補間値
		/// </summary>
		public float InterpolationValue
		{
			get
			{
				if( CSpriteDrawer == null )
				{
					return 0 ;
				}

				Material material = CSpriteDrawer.DuplicatedMaterial ;
				if( material == null )
				{
					return 0 ;
				}

				//---------------------------------

				string key = "_InterpolationValue" ;

				if( material.HasFloat( key ) == false )
				{
					return 0 ;
				}

				return material.GetFloat( key ) ;
			}
			set
			{
				if( CSpriteDrawer == null )
				{
					return ;
				}

				Material material = CSpriteDrawer.DuplicatedMaterial ;
				if( material == null )
				{
					return ;
				}
				
				//---------------------------------

				string key = "_InterpolationValue" ;

				if( material.HasFloat( key ) == false )
				{
					return ;
				}

				material.SetFloat( key, value ) ;
			}
		}

		/// <summary>
		/// 線形補間色
		/// </summary>
		public Color InterpolationColor
		{
			get
			{
				if( CSpriteDrawer == null )
				{
					return Color.white ;
				}

				Material material = CSpriteDrawer.DuplicatedMaterial ;
				if( material == null )
				{
					return Color.white ;
				}

				//---------------------------------

				string key = "_InterpolationColor" ;

				if( material.HasColor( key ) == false )
				{
					return Color.white ;
				}

				return material.GetColor( key ) ;
			}
			set
			{
				if( CSpriteDrawer == null )
				{
					return ;
				}

				Material material = CSpriteDrawer.DuplicatedMaterial ;
				if( material == null )
				{
					return ;
				}

				//---------------------------------

				string key = "_InterpolationColor" ;

				if( material.HasColor( key ) == false )
				{
					return ;
				}

				material.SetColor( key, value ) ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// コライダーの位置と大きさをメッシュと同じに合わせる(Flip に対応)
		/// </summary>
		public override void AdjustCollider()
		{
			if( CST == null )
			{
				return ;
			}

			//----------------------------------------------------------

			var offset = CST.Offset ;
			var size   = CST.DeltaSize ;

			if( m_Collider is BoxCollider2D )
			{
				var collider2D = m_Collider as BoxCollider2D ;

				float mx, my ;
				float ml, mr, mt, mb ;

				ml = m_ColliderMarginL ;
				mr = m_ColliderMarginR ;

				if( CSpriteDrawer.FlipX == true )
				{
					( ml, mr ) = ( mr, ml ) ;
				}

				mt = m_ColliderMarginT ;
				mb = m_ColliderMarginB ;

				if( CSpriteDrawer.FlipY == true )
				{
					( mt, mb ) = ( mb, mt ) ;
				}

				mx = ( ml - mr ) * 0.5f ;
				my = ( mb - mt ) * 0.5f ;

				collider2D.offset	= new ( offset.x + mx, offset.y + my ) ;
				collider2D.size		= new ( size.x - ml - mr, size.y - mb - mt ) ;
			}
			else
			if( m_Collider is CircleCollider2D )
			{
				var collider2D = m_Collider as CircleCollider2D ;

				float mx, my ;
				float ml, mr, mt, mb ;

				ml = m_ColliderMarginL ;
				mr = m_ColliderMarginR ;

				if( CSpriteDrawer.FlipX == true )
				{
					( ml, mr ) = ( mr, ml ) ;
				}

				mt = m_ColliderMarginT ;
				mb = m_ColliderMarginB ;

				if( CSpriteDrawer.FlipY == true )
				{
					( mt, mb ) = ( mb, mt ) ;
				}

				mx = ( ml - mr ) * 0.5f ;
				my = ( mb - mt ) * 0.5f ;

				collider2D.offset	= new ( offset.x + mx, offset.y + my ) ;
				collider2D.radius	= Mathf.Min( size.x - ml - mr, size.y - mb - mt ) * 0.5f ;
			}
			else
			if( m_Collider is CapsuleCollider2D )
			{
				var collider2D = m_Collider as CapsuleCollider2D ;

				float mx, my ;
				float ml, mr, mt, mb ;

				ml = m_ColliderMarginL ;
				mr = m_ColliderMarginR ;

				if( CSpriteDrawer.FlipX == true )
				{
					( ml, mr ) = ( mr, ml ) ;
				}

				mt = m_ColliderMarginT ;
				mb = m_ColliderMarginB ;

				if( CSpriteDrawer.FlipY == true )
				{
					( mt, mb ) = ( mb, mt ) ;
				}

				mx = ( ml - mr ) * 0.5f ;
				my = ( mb - mt ) * 0.5f ;

				collider2D.offset	= new ( offset.x + mx, offset.y + my ) ;
				collider2D.size		= new ( size.x - ml - mr, size.y - mb - mt ) ;
			}
			else
			if( m_Collider is EdgeCollider2D )
			{
				var collider2D = m_Collider as EdgeCollider2D ;

				float mx, my ;
				float ml, mr, mt, mb ;

				ml = m_ColliderMarginL ;
				mr = m_ColliderMarginR ;

				if( CSpriteDrawer.FlipX == true )
				{
					( ml, mr ) = ( mr, ml ) ;
				}

				mt = m_ColliderMarginT ;
				mb = m_ColliderMarginB ;

				if( CSpriteDrawer.FlipY == true )
				{
					( mt, mb ) = ( mb, mt ) ;
				}

				mx = ( ml - mr ) * 0.5f ;
				my = ( mb - mt ) * 0.5f ;

				float ox = offset.x + mx,    oy = offset.y + my ;
				float sx = size.x - ml - mr, sy = size.y - mb - mt ; 

				float hx = sx * 0.5f ;
				float hy = sy * 0.5f ;

				float x0 = ox - hx ;
				float x1 = ox + hx ;
				float y0 = oy - hy ;
				float y1 = oy + hy ;

				float ew = m_ColliderEdgeWidth ;
				if( ew <= 0 )
				{
					ew  = 0.1f ;
				}

				collider2D.points = new Vector2[]
				{
					new ( x0 + ew, y0 ),
					new ( x0, y0 + ew ),

					new ( x0, y1 - ew ),
					new ( x0 + ew, y1 ),

					new ( x1 - ew, y1 ),
					new ( x1, y1 - ew ),

					new ( x1, y0 + ew ),
					new ( x1 - ew, y0 ),

					new ( x0 + ew, y0 ),	// 最初に戻る
				} ;
			}

			m_IsColliderDirty = false ;
		}
	}

	//--------------------------------------------------------------------------------------------

	/// <summary>
	/// リストの拡張メソッド
	/// </summary>
	public static class ExList
	{
		/// <summary>
		/// 要素をまとめて破棄する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="collection"></param>
		public static void RemoveRange<T>( this List<T> list, IEnumerable<T> collection )
		{
			if( collection == null )
			{
				return ;
			}

			foreach( T element in collection )
			{
				list.Remove( element ) ;
			}
		}
	}
}
