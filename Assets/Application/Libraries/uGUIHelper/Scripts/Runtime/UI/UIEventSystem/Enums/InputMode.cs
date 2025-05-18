namespace uGUIHelper.InputAdapter
{
	/// <summary>
	/// 現在の入力の処理タイプ
	/// </summary>
	public enum InputProcessingTypes
	{
		/// <summary>
		/// 処理しない
		/// </summary>
		None	    = 0 ,

		/// <summary>
		/// どちらか片方の入力のみ出来る
		/// </summary>
		Switching	= 1,

		/// <summary>
		/// 同時に両方の入力が出来る
		/// </summary>
		Parallel	= 2,
	}

	/// <summary>
	/// 現在の入力タイプ
	/// </summary>
	public enum InputTypes
	{
		/// <summary>
		/// 不明
		/// </summary>
		Unknown     =  0,

		/// <summary>
		/// ポインター(マウス・タッチ)　※nputProcessingType が Single
		/// </summary>
		Pointer	    =  1,

		/// <summary>
		/// マウス
		/// </summary>
		Mouse       =  2,

		/// <summary>
		/// キーボード
		/// </summary>
		Keyboard    =  3,

		/// <summary>
		/// ゲームパッド　※InputProcessingType が Single
		/// </summary>
		GamePad     =  4,

		// 注意 : Dual 入力モードの場合は最後に切り替わった(現在)のモードになる
	}

	/// <summary>
	/// 入力の分類
	/// </summary>
	public enum InputCategories
	{
		/// <summary>
		/// 基本入力操作
		/// </summary>
		Basis,

		/// <summary>
		/// 拡張入力操作
		/// </summary>
		Extra,
	}
}
