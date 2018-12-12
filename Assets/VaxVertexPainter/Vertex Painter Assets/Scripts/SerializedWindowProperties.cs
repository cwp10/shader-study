using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class SerializedWindowProperties : ScriptableObject {


	[SerializeField]
	public bool instanceMode = true;
	[SerializeField]
	public bool assetMode;

	[SerializeField]
	public bool paintMode = true;
	[SerializeField]
	public bool eraseMode;

	[SerializeField]
	public bool brushMode = true;
	[SerializeField]
	public bool bucketMode;

	[SerializeField]
	public Color paintColor = Color.red;
	[SerializeField]
	public Color eraseColor = Color.clear;
	[SerializeField]
	public float brushSize = 1f;
	[SerializeField]
	public float brushStrenght = 1f;
	[SerializeField]
	public float brushFallOff;
	[SerializeField]
	public float angleLimit = 180f;

	[SerializeField]
	public float bucketSize;

	[SerializeField]
	public bool allObjects = true;
	[SerializeField]
	public bool perObjects;

	[SerializeField]
	public bool rChannel = true;
	[SerializeField]
	public bool gChannel = true;
	[SerializeField]
	public bool bChannel = true;
	[SerializeField]
	public bool aChannel = true;

	[SerializeField]
	public Color handleColor = Color.yellow;
	[SerializeField]
	public Color outlineHandleColor = Color.grey;
	[SerializeField]
	public bool solidHandle;
	[SerializeField]
	public bool drawHandleOutline;
	[SerializeField]
	public bool drawHandleAngle;

	//Foldouts
	[SerializeField]
	public bool objectPropertiesFoldout = true;
	[SerializeField]
	public bool colorPropertiesFoldout = true;
	[SerializeField]
	public bool toolPropertiesFoldout = true;
	[SerializeField]
	public bool paintPropertiesFoldout = true;
	[SerializeField]
	public bool gizmoPropertiesFoldout = true;
	[SerializeField]
	public bool hotkeysPropertiesFoldout = true;
	[SerializeField]
	public bool uninstallerFoldout;
	[SerializeField]
	public bool questionsFoldout = true;
	[SerializeField]
	public bool suggestionsFoldout;
	[SerializeField]
	public bool bugsFoldout;

	//Hotkeys
	[SerializeField]
	public Hotkey paintHotkey = new Hotkey(KeyCode.None,true,false,false);
	[SerializeField]
	public Hotkey eraseHotkey = new Hotkey(KeyCode.None,true,true,false);
	[SerializeField]
	public Hotkey increaseSizeHotkey = new Hotkey(KeyCode.KeypadPlus,true,false,false);
	[SerializeField]
	public Hotkey decreaseSizeHotkey = new Hotkey(KeyCode.KeypadMinus,true,false,false);
	[SerializeField]
	public Hotkey showVertexColorsHotkey = new Hotkey(KeyCode.X,true,false,true);
	[SerializeField]
	public Hotkey copyVertexColorsHotkey = new Hotkey(KeyCode.C,true,false,true);
	[SerializeField]
	public Hotkey pasteVertexColorsHotkey = new Hotkey(KeyCode.V,true,false,true);
	
	//Hotkeys States
	[SerializeField]
	public bool paintHotkeyState;
	[SerializeField]
	public bool eraseHotkeyState;
	[SerializeField]
	public bool increaseSizeHotkeyState;
	[SerializeField]
	public bool decreaseSizeHotkeyState;
	[SerializeField]
	public bool showVertexColorsHotkeyState;
	[SerializeField]
	public bool copyVertexColorsHotkeyState;
	[SerializeField]
	public bool pasteVertexColorsHotkeyState;
	[SerializeField]
	
	public class Hotkey{
		public KeyCode keycode;
		public bool control;
		public bool shift;
		public bool alt;

		public Hotkey(KeyCode Keycode, bool Control,bool Shift, bool Alt){

			keycode = Keycode;
			control = Control;
			shift = Shift;
			alt = Alt;
		}
	}
}
