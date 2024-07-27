#if UNITY_EDITOR

using System ;
using System.Reflection ;
using UnityEditor ;
using UnityEngine ;
using UnityEngine.UI ;

// 参考
// https://zenn.dev/emptybraces/articles/6840c98af1c544


namespace DSW
{
    /// <summary>
    /// RectTransform の Inspector 拡張クラス(Editor 用) Version 2024/07/27
    /// </summary>
    [CustomEditor( typeof( RectTransform ), true )]
    internal class RectTransformEditor : Editor
    {
//      private const string RECORD_NAME = nameof( ExRectTransformInspector ) ;
        private const string RECORD_NAME = "RectTransform Modified" ;

        private Editor     m_DefaultEditorInstance ;

        //-------------------------------------------------------------------------------------

        /// <summary>
        /// インスペクター拡張
        /// </summary>
        public override void OnInspectorGUI()
        {
//          Debug.Log( "<color=#FFFF00>OnInspectorGUI " + this.GetInstanceID() + "</color>" ) ;

            if( m_DefaultEditorInstance == null )
            {
                Debug.LogWarning( "[Error] Could not found editor type." ) ;
                return ;
            }

            m_DefaultEditorInstance.OnInspectorGUI() ;

            //-------------------------------------------------

            // RectTransform のインスタンスを取得する
            var rectTransform = target as RectTransform ;

            if( rectTransform == null )
            {
                // RectTRansform でなければ無視する
                return ;
            }

            string recordName = $"{RECORD_NAME} - {rectTransform.name}" ;

            //-------------------------------------------------

            var oldEnabled = GUI.enabled ;

            //---------------------------------------------------------------------------------

            if( rectTransform.hideFlags != HideFlags.NotEditable )
            {
                // １行目の拡張メニュー(編集可能な時のみ有効)
                using ( new EditorGUILayout.HorizontalScope( EditorStyles.helpBox ) )
                {
                    //---------------------
                    // Copy

                    GUI.enabled = true ;

                    if( GUILayout.Button( new GUIContent( "Copy", "RectTransform の状態をクリップボードにコピーします" ) ) == true )
                    {
                        GUIUtility.systemCopyBuffer = Serialize( rectTransform ) ;

                        EditorUtility.DisplayDialog( "コピー", "クリップボードに保存しました", "閉じる" ) ;
                    }

                    //---------------------
                    // Paste

                    GUI.enabled = VerifyClipboard() ;

                    if( GUILayout.Button( new GUIContent( "Paste", "クリップボードに保存された RectTransform の状態を反映します" ) ) == true )
                    {
                        if( Deserialize( rectTransform ) == true )
                        {
                        }
                        else
                        {
                            EditorUtility.DisplayDialog( "エラー", "反映に失敗しました", "閉じる" ) ;
                        }
                    }

                    //---------------------
                    // Stretch

                    GUI.enabled =
                        rectTransform.anchorMin != Vector2.zero ||
                        rectTransform.anchorMax != Vector2.one  ||
                        rectTransform.offsetMin != Vector2.zero ||
                        rectTransform.offsetMax != Vector2.zero ||
                        rectTransform.pivot != new Vector2( 0.5f, 0.5f ) ||
                        rectTransform.rotation != Quaternion.identity ||
                        rectTransform.localScale != Vector3.one ;

                    if( GUILayout.Button( new GUIContent( "Stretch", "Anchor を完全な Stretch 状態にします" ) ) == true )
                    {
                        Undo.RecordObject( rectTransform, recordName ) ;

                        rectTransform.anchorMin  = Vector2.zero ;
                        rectTransform.anchorMax  = Vector2.one ;
                        rectTransform.offsetMin  = Vector2.zero ;
                        rectTransform.offsetMax  = Vector2.zero ;
                        rectTransform.pivot      = new Vector2( 0.5f, 0.5f ) ;
                        rectTransform.rotation   = Quaternion.identity ;
                        rectTransform.localScale = Vector3.one ;
                    }

                    //---------------------
                    // Round

                    GUI.enabled =
                        rectTransform.anchoredPosition.HasAfterDecimalPoint() ||
                        rectTransform.sizeDelta.HasAfterDecimalPoint() ||
                        rectTransform.offsetMin.HasAfterDecimalPoint() ||
                        rectTransform.offsetMax.HasAfterDecimalPoint() ||
                        rectTransform.localScale.HasAfterDecimalPoint() ;

                    if( GUILayout.Button( new GUIContent( "Round", "位置やサイズを四捨五入して整数値にします" ) ) == true )
                    {
                        Undo.RecordObject( rectTransform, recordName ) ;

                        rectTransform.anchoredPosition  = rectTransform.anchoredPosition.Round() ;
                        rectTransform.sizeDelta         = rectTransform.sizeDelta.Round() ;
                        rectTransform.offsetMin         = rectTransform.offsetMin.Round() ;
                        rectTransform.offsetMax         = rectTransform.offsetMax.Round() ;
                        rectTransform.localScale        = rectTransform.localScale.Round() ;
                    }
                }
            }

            if( rectTransform.hideFlags != HideFlags.NotEditable )
            {
                // ２行目の拡張メニュー(編集可能な時のみ有効)
                using ( new EditorGUILayout.HorizontalScope( EditorStyles.helpBox ) )
                {
                    // 位置を初期化する
                    GUI.enabled = ( rectTransform.anchoredPosition != Vector2.zero ) && ( rectTransform.anchorMin == rectTransform.anchorMax ) ;
                    if( GUILayout.Button( new GUIContent( "Reset Position", "Position を (0,0,0) にリセットします" ) ) == true )
                    {
                        Undo.RecordObject( rectTransform, recordName ) ;
                        rectTransform.anchoredPosition = Vector2.zero ;
                    }

                    // 回転を初期化する
                    GUI.enabled = rectTransform.localRotation != Quaternion.identity ;
                    if( GUILayout.Button( new GUIContent( "Reset Rotation", "Rotation を (0,0,0) にリセットします" ) ) == true )
                    {
                        Undo.RecordObject( rectTransform, recordName ) ;
                        rectTransform.localRotation = Quaternion.identity ;
                    }

                    // 縮尺を初期化する
                    GUI.enabled = rectTransform.localScale != Vector3.one ;
                    if( GUILayout.Button( new GUIContent( "Reset Scale", "Scale を (1,1,1) にリセットします" ) ) == true )
                    {
                        Undo.RecordObject( rectTransform, recordName ) ;
                        rectTransform.localScale = Vector3.one ;
                    }
                }
            }

            //---------------------------------------------------------------------------------
#if false
            // レイアウト設定用のコンポーネントの追加
            var creator = new ComponentButtonCreator( rectTransform.gameObject ) ;

            // ３行目の拡張メニュー
            using ( new EditorGUILayout.HorizontalScope( EditorStyles.helpBox ) )
            {
                creator.Create<CanvasGroup, CanvasGroup>( "CanvasGroup Icon" ) ;
                creator.Create<HorizontalLayoutGroup, LayoutGroup>( "HorizontalLayoutGroup Icon" ) ;
                creator.Create<VerticalLayoutGroup, LayoutGroup>( "VerticalLayoutGroup Icon" ) ;
                creator.Create<GridLayoutGroup, LayoutGroup>( "GridLayoutGroup Icon" ) ;
                creator.Create<ContentSizeFitter, ContentSizeFitter>( "ContentSizeFitter Icon" ) ;
            }
#endif
            //---------------------------------------------------------------------------------

            GUI.enabled = oldEnabled ;
        }

        //-------------------------------------------------------------------------------------

        // オリジナルの Inspector のインスタンスを生成する
        private void CreateDefaultEditorInstance()
        {
            if( m_DefaultEditorInstance != null )
            {
                return ;
            }

            //-------------------------

            // オリジナルの Inspector のタイプを取得する
            Type defaultEditorType  = Assembly.GetAssembly( typeof( Editor ) ).GetType( "UnityEditor.RectTransformEditor" ) ;

            if( defaultEditorType != null )
            {
                // オリジナルの Inspector のインスタンスを生成する
                m_DefaultEditorInstance = CreateEditor( target, defaultEditorType ) ;
            }
            else
            {
                Debug.LogWarning( "[Error] Could not found default editor type." ) ;
            }
        }

        //-----------------------------------------------------
        // 呼ばれる順番
        // 生成時 : Reset → Awake → OnEnable
        // 破棄時 : OnDisable → OnDestroy

        private void Reset()
        {
//          Debug.Log( "<color=#FFFF00>Reset " + this.GetInstanceID() + "</color>" ) ;

            if( m_DefaultEditorInstance == null )
            {
                CreateDefaultEditorInstance() ;
            }
            if( m_DefaultEditorInstance == null )
            {
                Debug.LogWarning( "[Error] Default editor instance is null." ) ;
                return ;
            }

            // オリジナルの Inspector の Reset() を呼び出す
            m_DefaultEditorInstance.GetType().GetMethod( "Reset", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )?.Invoke( m_DefaultEditorInstance, null ) ;
        }

        private void Awake()
        {
//          Debug.Log( "<color=#FFFF00>Call OnAwake " + this.GetInstanceID() + "</color>" ) ;

            if( m_DefaultEditorInstance == null )
            {
                CreateDefaultEditorInstance() ;
            }
            if( m_DefaultEditorInstance == null )
            {
                Debug.LogWarning( "[Error] Default editor instance is null." ) ;
                return ;
            }

            // オリジナルの Inspector の Awake() を呼び出す
            m_DefaultEditorInstance.GetType().GetMethod( "Awake", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )?.Invoke( m_DefaultEditorInstance, null ) ;
        }

        private void OnEnable()
        {
//          Debug.Log( "<color=#FFFF00>Call OnEnable " + this.GetInstanceID() + "</color>" ) ;

            if( m_DefaultEditorInstance != null )
            {
               // オリジナルの Inspector の OnEnable() を呼び出す
                m_DefaultEditorInstance.GetType().GetMethod( "OnEnable", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic )?.Invoke( m_DefaultEditorInstance, null ) ;
            }
        }

        private void OnSceneGUI()
        {
//          Debug.Log( "<color=#FFFF00>Call OnSceneGUI " + this.GetInstanceID() + "</color>" ) ;

            if( m_DefaultEditorInstance != null )
            {
                // オリジナルの Inspector の OnSceneGUI() を呼び出す
                m_DefaultEditorInstance.GetType().GetMethod( "OnSceneGUI", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance )?.Invoke( m_DefaultEditorInstance, null ) ;
            }
        }

        private void OnValidate()
        {
//          Debug.Log( "<color=#FFFF00>Call OnValidate " + this.GetInstanceID() + "</color>" ) ;

            if( m_DefaultEditorInstance != null )
            {
                // オリジナルの Inspector の OnValidate() を呼び出す
                m_DefaultEditorInstance.GetType().GetMethod( "OnValidate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance )?.Invoke( m_DefaultEditorInstance, null ) ;
            }
        }

        private void OnDisable()
        {
//          Debug.Log( "<color=#FFFF00>Call OnDisable " + this.GetInstanceID() + "</color>" ) ;

            if( m_DefaultEditorInstance != null )
            {
                // オリジナルの Inspector の OnDisable() を呼び出す
                m_DefaultEditorInstance.GetType().GetMethod( "OnDisable", BindingFlags.NonPublic | BindingFlags.Instance )?.Invoke( m_DefaultEditorInstance, null ) ;
            }
        }

        private void OnDestroy()
        {
//          Debug.Log( "<color=#FFFF00>Call OnDestroy " + this.GetInstanceID() + "</color>" ) ;

            if( m_DefaultEditorInstance != null )
            {
                // オリジナルの Inspector の OnDestroy() を呼び出す
                m_DefaultEditorInstance.GetType().GetMethod( "OnDestroy", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )?.Invoke( m_DefaultEditorInstance, null ) ;

                DestroyImmediate( m_DefaultEditorInstance ) ;
                m_DefaultEditorInstance = null ;
            }
        }

        //-------------------------------------------------------------------------------------

        private sealed class ComponentButtonCreator
        {
            private readonly GameObject m_gameObject ;

            public ComponentButtonCreator( GameObject gameObject ) => m_gameObject = gameObject ;

            public void Create<T1, T2>( string iconName )
                where T1 : Component
                where T2 : Component
            {
                var hasComponent = m_gameObject.GetComponent<T2>() != null ;

                GUI.enabled = !hasComponent ;

                if( GUILayout.Button( EditorGUIUtility.IconContent( iconName ), GUILayout.Height( 20 ) ) )
                {
                    Undo.AddComponent<T1>( m_gameObject ) ;
                }
            }
        }

        //-------------------------------------------------------------------------------------

        // クリップボード格納用の RectTransform の設定情報
        [Serializable]
        public class RectTransformPackage
        {
            public float   PositionZ ;
            public Vector3 Rotation ;
            public Vector3 Scale ;

            public Vector2 AnchorMin ;
            public Vector2 AnchorMax ;
            public Vector2 OffsetMin ;
            public Vector2 OffsetMax ;
            public Vector2 AnchorPosition ;
            public Vector2 Pivot ;
        }

        private const string m_Signature = "RectTransform:" ;

        // RectTransform の設定をクリップボード保存用に Json テキストにシリアライズする
        private string Serialize( RectTransform rectTransform )
        {
            var package = new RectTransformPackage()
            {
                PositionZ       = rectTransform.localPosition.z,
                Rotation        = rectTransform.localRotation.eulerAngles,
                Scale           = rectTransform.localScale,

                AnchorMin       = rectTransform.anchorMin,
                AnchorMax       = rectTransform.anchorMax,
                OffsetMin       = rectTransform.offsetMin,
                OffsetMax       = rectTransform.offsetMax,
                AnchorPosition  = rectTransform.anchoredPosition,
                Pivot           = rectTransform.pivot,
            } ;

            return m_Signature + JsonUtility.ToJson( package ) ;
        }

        // クリップボードに RectTransform の設定が確認されているか確認する
        private bool VerifyClipboard()
        {
            if( string.IsNullOrEmpty( GUIUtility.systemCopyBuffer ) == true )
            {
                // クリップボードには存在しない
                return false ;
            }

            string jsonText = GUIUtility.systemCopyBuffer ;

            return ( jsonText.IndexOf( m_Signature ) == 0 ) ;
        }

        // クリップボードに格納された RectTransform の設定を反映する
        private bool Deserialize( RectTransform rectTransform )
        {
            if( string.IsNullOrEmpty( GUIUtility.systemCopyBuffer ) == true )
            {
                // クリップボードには存在しない
                return false ;
            }

            string jsonText = GUIUtility.systemCopyBuffer ;

            if( jsonText.IndexOf( m_Signature ) != 0 )
            {
                // フォーマット異常
                return false ;
            }

            //--------------------------------------------------

            // 不要なシグネチャ部分を削除する
            jsonText = jsonText.Replace( m_Signature, "" ) ;

            var package = JsonUtility.FromJson<RectTransformPackage>( jsonText ) ;
            if( package == null )
            {
                // デシリアライズ失敗
                return false ;
            }

            //-------------------------------------------------

            // 反映
            rectTransform.localRotation     = Quaternion.Euler( package.Rotation ) ;
            rectTransform.localScale        = package.Scale ;

            rectTransform.anchorMin         = package.AnchorMin ;
            rectTransform.anchorMax         = package.AnchorMax ;
            rectTransform.offsetMin         = package.OffsetMin ;
            rectTransform.offsetMax         = package.OffsetMax ;
            rectTransform.anchoredPosition  = package.AnchorPosition ;

            rectTransform.pivot             = package.Pivot ;

            rectTransform.localPosition = new Vector3( rectTransform.localPosition.x, rectTransform.localPosition.y, package.PositionZ ) ;

            return true ;

        }
    }

    //----------------------------------------------------------------------------------------

    internal static class ExtensionMethods
    {
        public static bool HasAfterDecimalPoint( this float self )
        {
            return 0.000001f < Math.Abs( self % 1 ) ;
        }

        public static bool HasAfterDecimalPoint( this Vector2 self )
        {
            return
                self.x.HasAfterDecimalPoint() ||
                self.y.HasAfterDecimalPoint() ;
        }

        public static bool HasAfterDecimalPoint( this Vector3 self )
        {
            return
                self.x.HasAfterDecimalPoint() ||
                self.y.HasAfterDecimalPoint() ||
                self.z.HasAfterDecimalPoint() ;
        }

        public static Vector2 Round( this Vector2 self )
        {
            return new Vector2
            (
                Mathf.Round( self.x ),
                Mathf.Round( self.y )
            ) ;
        }

        public static Vector3 Round( this Vector3 self )
        {
            return new Vector3
            (
                Mathf.Round( self.x ),
                Mathf.Round( self.y ),
                Mathf.Round( self.z )
            ) ;
        }
    }
}

#endif
