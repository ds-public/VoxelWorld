namespace uGUIHelper.InputAdapter
{
	// https://docs.unity3d.com/ja/2021.2/ScriptReference/KeyCode.html

	/// <summary>
	/// キーボードのキー識別名
	/// </summary>
	public enum KeyCodes
	{
		/// <summary>
		/// 何もなし
		/// </summary>
		None = 0,

		Backspace,  Delete, Tab, Clear, Return, Pause, Escape, Space,
		Keypad0, Keypad1, Keypad2, Keypad3, Keypad4, Keypad5, Keypad6, Keypad7, Keypad8, Keypad9,
		KeypadPeriod, KeypadDivide, KeypadMultiply, KeypadMinus, KeypadPlus, KeypadEnter, KeypadEquals,
		UpArrow, DownArrow, RightArrow, LeftArrow,
		Insert, Home, End, PageUp, PageDown,
		F1,  F2,  F3,  F4,  F5,  F6,  F7,  F8,  F9, F10, F11, F12, F13, F14, F15,
		Alpha0, Alpha1, Alpha2, Alpha3, Alpha4, Alpha5, Alpha6, Alpha7, Alpha8, Alpha9,

		/// <summary>
		/// !
		/// </summary>
		Exclaim,

		/// <summary>
		/// "
		/// </summary>
		DoubleQuote,

		/// <summary>
		/// #
		/// </summary>
		Hash,

		/// <summary>
		/// $
		/// </summary>
		Dollar,

		/// <summary>
		/// %
		/// </summary>
		Percent,

		/// <summary>
		/// &
		/// </summary>
		Ampersand,

		/// <summary>
		/// '
		/// </summary>
		Quote,

		/// <summary>
		/// (
		/// </summary>
		LeftParen,

		/// <summary>
		/// )
		/// </summary>
		RightParen,

		/// <summary>
		/// *
		/// </summary>
		Asterisk,

		/// <summary>
		/// +
		/// </summary>
		Plus,

		/// <summary>
		/// ,
		/// </summary>
		Comma,

		/// <summary>
		/// -
		/// </summary>
		Minus,

		/// <summary>
		/// .
		/// </summary>
		Period,

		/// <summary>
		/// /
		/// </summary>
		Slash,

		/// <summary>
		/// :
		/// </summary>
		Colon,

		/// <summary>
		/// ;
		/// </summary>
		Semicolon,

		/// <summary>
		/// <
		/// </summary>
		Less,

		/// <summary>
		/// =
		/// </summary>
		Equals,

		/// <summary>
		/// >
		/// </summary>
		Greater,

		/// <summary>
		/// ?
		/// </summary>
		Question,

		/// <summary>
		/// @
		/// </summary>
		At,
		/// <summary>
		/// [
		/// </summary>
		LeftBracket,

		/// <summary>
		/// /
		/// </summary>
		Backslash,

		/// <summary>
		/// ]
		/// </summary>
		RightBracket,

		/// <summary>
		/// ^
		/// </summary>
		Caret,

		/// <summary>
		/// _
		/// </summary>
		Underscore,

		/// <summary>
		/// `
		/// </summary>
		BackQuote,

		A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,

		/// <summary>
		/// {
		/// </summary>
		LeftCurlyBracket,

		/// <summary>
		/// |
		/// </summary>
		Pipe,

		/// <summary>
		/// }
		/// </summary>
		RightCurlyBracket,

		/// <summary>
		/// ~
		/// </summary>
		Tilde,

		Numlock, CapsLock, ScrollLock,
		RightShift, LeftShift, RightControl, LeftControl, RightAlt, LeftAlt,
		LeftMeta, LeftCommand, LeftApple, LeftWindows, RightMeta, RightCommand, RightApple, RightWindows,
		AltGr, Help, Print, SysReq, Break, Menu,
	}
}
