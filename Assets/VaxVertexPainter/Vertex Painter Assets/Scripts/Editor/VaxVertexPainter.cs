using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vax.VertexPainter;

namespace Vax.VertexPainter{
public class VaxVertexPainter : EditorWindow
{

	string folderPath;
	public bool instanceMode = true;
	public bool assetMode;
	public Color paintColor = Color.white;
	public Color eraseColor = Color.black;
	public bool paintMode = true;
	public bool eraseMode;
	public bool brushMode = true;
	public bool bucketMode;
	public float brushSize;
	public float brushStrenght;
	public float brushFallOff;
	public float angleLimit = 180f;
	public float bucketSize = 1f;
	public bool allObjects = true;
	public bool perObjects;
	public bool rChannel = true;
	public bool gChannel = true;
	public bool bChannel = true;
	public bool aChannel = true;
	public Color handleColor = Color.yellow;
	public Color outlineHandleColor = Color.gray;
	public bool solidHandle;
	public bool drawHandleOutline;
	public bool drawHandleAngle;
	public Event paintHotkey = new Event();
	public Event eraseHotkey = new Event();
	public Event increaseSizeHotkey = new Event();
	public Event decreaseSizeHotkey = new Event();
	public Event showVertexColorsHotkey = new Event();
	public Event copyVertexColorsHotkey = new Event();
	public Event pasteVertexColorsHotkey = new Event();
	public bool paintHotkeyState;
	public bool eraseHotkeyState;
	public bool increaseSizeHotkeyState;
	public bool decreaseSizeHotkeyState;
	public bool showVertexColorsHotkeyState;
	public bool copyVertexColorsHotkeyState;
	public bool pasteVertexColorsHotkeyState;
	
	bool toolChange;
	Tool toolSelected;
	bool guiChanged;	
	bool paintButton;
	Mesh assetMesh;

	bool waitingForInput = false;
	string nameOfInput = "";
	Vector2 scrollMainPage;
	Vector2 scrollEditorPage;
	Vector2 scrollHelpPage;
	Vector2 scrollSelectedObjects;
	string searchText = "";
	List<Color> colorClipboard;
	float pixelWidth;
	float pixelHeight;
	float resizeOffset = 9;
	bool resizingWindow;

	bool objectPropertiesFoldout = true;
	bool selectedObjectsPropertiesFoldout = false;
	bool colorPropertiesFoldout = true;
	bool toolPropertiesFoldout = true;
	bool brushPropertiesFoldout = true;
	bool bucketPropertiesFoldout = true;
	bool paintPropertiesFoldout = true;
	bool gizmoPropertiesFoldout = true;
	bool hotkeysPropertiesFoldout = false;
	//bool editorPropertiesFoldout = false;
	bool uninstallFoldout = true;
	bool questionsFoldout = false;
	bool suggestionsFoldout = false;
	bool bugsFoldout = false;
	bool tryingToClear = false;
	bool tryingToClear2 = false;
	bool tryingToClear3 = false;
	bool finalClear = false;
	bool tryingTouninstall = false;
	bool tryingTouninstall2 = false;
	bool tryingTouninstall3 = false;
	bool finaluninstallation = false;	

	string orderNumber = "";
	string questionString = "";
	string suggestionString = "";
	string bugString = "";
	string[] menuOptions = new string[] { "Main Menu", "Editor Menu", "Help Menu" };
	int menuOptionsIndex = 0;
	Color headerColor = new Color(0.65f, 0.65f, 0.65f, 1);
	Color backgroundColor = new Color(0.75f, 0.75f, 0.75f);
	bool painting;
	bool hitting;
	Mesh objectMesh;
	List<GameObject> editedGameObjects = new List<GameObject>();
	List<GameObject> selectedTransforms = new List<GameObject>();
	string colorString;
	SerializedWindowProperties serializedScript;
	ScriptableObject configFile;
	Ray ray;
	RaycastHit hit;
	
	//Paint Variables
	float posMagnitude;
	Vector3 pos;
	Color vColor = Color.white;
	Vector3[] meshVertex;
	Color[] meshColors;

	//Vector Colors
	Vector4 redVector = new Vector4(1, 0, 0, 1);
	Vector4 greenVector = new Vector4(0, 1, 0, 1);
	Vector4 blueVector = new Vector4(0, 0, 1, 1);

	[MenuItem("Window/Vax Vertex Painter")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(VaxVertexPainter));
	}

	void OnEnable()
	{
		if (SceneView.onSceneGUIDelegate != this.OnSceneGUI) SceneView.onSceneGUIDelegate += this.OnSceneGUI;
        if (EditorApplication.playmodeStateChanged != HandleOnPlayModeChanged) { EditorApplication.playmodeStateChanged += HandleOnPlayModeChanged; }
        editedGameObjects.Clear();
		selectedTransforms.Clear();
		name = "Vax Vertex Painter";
		//Select the color of header and background based on the skin of Unity.
		if (!EditorGUIUtility.isProSkin)
		{
			headerColor = new Color(165 / 255f, 165 / 255f, 165 / 255f, 1);
			backgroundColor = new Color(193 / 255f, 193 / 255f, 193 / 255f, 1);
		}
		else
		{
			headerColor = new Color(41 / 255f, 41 / 255f, 41 / 255f, 1);
			backgroundColor = new Color(56 / 255f, 56 / 255f, 56 / 255f, 1);
		}
		
		//Get the path of the main folder.
		folderPath = GetScriptPath().Remove(GetScriptPath().Length - 34);
		//If we don't have the config settings created, create one.
		if (!AssetDatabase.LoadAssetAtPath(folderPath + "Scripts/Editor/EditorData.asset", typeof(SerializedWindowProperties)))
		{
			AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<SerializedWindowProperties>(), folderPath + "Scripts/Editor/EditorData.asset");
			serializedScript = AssetDatabase.LoadAssetAtPath(folderPath + "Scripts/Editor/EditorData.asset", typeof(SerializedWindowProperties)) as SerializedWindowProperties;
		}
		//If we have the config load it.
		else 
			serializedScript = AssetDatabase.LoadAssetAtPath(folderPath + "Scripts/Editor/EditorData.asset", typeof(SerializedWindowProperties)) as SerializedWindowProperties;
			LoadVariables();
	}
	
	void OnGUI()
	{

		//Set the minimum size of the screen.
		this.minSize = new Vector2(350, 650);
		
		//Get how many pixels we expanded on the window based on the minimum size width.
		pixelWidth = position.width - 350;
		
		GUILayout.Space(124);
		
		//Draw Logo
		GUI.DrawTexture(new Rect(GUILayoutUtility.GetLastRect().x + 1, GUILayoutUtility.GetLastRect().y - 30, 350 + pixelWidth, 210 + pixelWidth), AssetDatabase.LoadAssetAtPath(folderPath + "Interface/vertexPainterBackground.png", typeof(Texture2D)) as Texture2D);
		GUI.DrawTexture(new Rect(GUILayoutUtility.GetLastRect().x - 25 + pixelWidth / 2, GUILayoutUtility.GetLastRect().y + 15, 390, 90), AssetDatabase.LoadAssetAtPath(folderPath + "Interface/vertexPainterTitle.png", typeof(Texture2D)) as Texture2D);
		
		//Menu for select the pages of the editor.
		Rect menuRect = new Rect(GUILayoutUtility.GetLastRect().x + 10 + pixelWidth / 2, GUILayoutUtility.GetLastRect().y + 105, 325, 20);
		
		//If we are hovering the menu, change the color.
		if (menuRect.Contains(Event.current.mousePosition))
			GUI.color = new Color(0.9f, 0.9f, 0.9f, 1f);
		else
			GUI.color = new Color(0.9f, 0.9f, 0.9f, 0.2f);
		
		//Draw the menu.
		menuOptionsIndex = EditorGUI.Popup(menuRect, menuOptionsIndex, menuOptions, "ExposablePopupMenu");
		GUI.color = Color.white;
		
		GUILayout.Space(1);
		
		//Draw background color based on the skin of Unity
		if (!EditorGUIUtility.isProSkin)
			EditorGUI.DrawRect(new Rect(0, GUILayoutUtility.GetLastRect().y, position.width, position.height), backgroundColor);
		else
			EditorGUI.DrawRect(new Rect(0, GUILayoutUtility.GetLastRect().y, position.width, position.height), backgroundColor);
		
		//Main Menu Page
		#region Main Menu
		if (menuOptionsIndex == 0)
		{
			
			scrollMainPage = GUILayout.BeginScrollView(scrollMainPage);
			GUILayout.Space(-5);
			GUILayout.Label("");
			GUILayout.Space(-16);

			#region Main Menu - Object Properties
			objectPropertiesFoldout = DrawHeaderTitle("Object Properties", objectPropertiesFoldout, headerColor);
			if (objectPropertiesFoldout)
			{
				GUILayout.Space(10);
				GUILayout.BeginHorizontal();
				GUILayout.Label("Save colors on: ", GUILayout.ExpandWidth(false));
				
				if (GUILayout.Button("Instance", EditorStyles.toggle, GUILayout.ExpandWidth(false)))
				{
					Undo.RecordObject(this, "Instance Mode");
					assetMode = false;
					instanceMode = true;
				}
				if (instanceMode)
					GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x+0.5f, GUILayoutUtility.GetLastRect().y, 20, 20), "X", EditorStyles.boldLabel);
				
				if (GUILayout.Button("Asset", EditorStyles.toggle, GUILayout.ExpandWidth(false)))
				{
					Undo.RecordObject(this, "Asset Mode");
					assetMode = true;
					instanceMode = false;
				}
				if (assetMode)
					GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x+0.5f, GUILayoutUtility.GetLastRect().y, 20, 20), "X", EditorStyles.boldLabel);
				
				GUILayout.EndHorizontal();
			}
			#endregion

			#region Main Menu - Selected Objects Properties
			if (Selection.activeTransform != null)
			{
					selectedObjectsPropertiesFoldout = DrawHeaderTitle("Selected Objects Properties", selectedObjectsPropertiesFoldout, headerColor);
					if (selectedObjectsPropertiesFoldout)
					{
						int selectedObjects = 0;
						int offsetButton = 35;
						for (int i = 0; i < Selection.gameObjects.Length; i++) {
							selectedObjects++;
						}
						if (selectedObjects > 1)
							offsetButton = 50;
						else
							offsetButton = 35;
						
						GUILayout.Space(1);
						GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
						//GUILayout.FlexibleSpace();
						searchText = GUILayout.TextField(searchText, "ToolbarSeachTextField");
						if (GUILayout.Button("","ToolbarSeachCancelButton"))
						{
							// Remove focus if cleared
							searchText = "";
							GUI.FocusControl(null);
						}
						GUILayout.EndHorizontal();		

						GUILayout.Space(4);
						scrollSelectedObjects = GUILayout.BeginScrollView(scrollSelectedObjects, GUILayout.Height(Mathf.Clamp(resizeOffset,115,235)),GUILayout.MaxHeight(Mathf.Clamp(resizeOffset,115,235)));
						GUILayout.Space(-0.5f);

						if(Selection.activeGameObject.GetComponent<MeshFilter>() == null && Selection.transforms.Length == 1)
						{
							GUILayout.Label("'"+Selection.activeGameObject.name+"'"+" is not a valid object\nThe object must have MeshFilter component");	
						}

						if(Selection.activeGameObject.GetComponent<Collider>() == null && Selection.transforms.Length == 1)
						{
							GUILayout.Label("'"+Selection.activeGameObject.name+"'"+" is not a valid object\nThe object must have MeshCollider component for paint");
						}

						foreach (var selectedTransform in Selection.gameObjects) {
							
							if(selectedTransform.GetComponent<MeshFilter>() != null && selectedTransform.GetComponent<Collider>() != null)
							{
								if (selectedTransform.name.ToLower().Contains(searchText.ToLower()) || string.IsNullOrEmpty(searchText))
								{
									
									//If the selected object don't have object properties script attached.
									if (selectedTransform.GetComponent<ObjectProperties>() == null)
									{
										if (GUILayout.Button(selectedTransform.name, EditorStyles.foldout))
										{
											ObjectProperties objectProperties = selectedTransform.gameObject.AddComponent<ObjectProperties>();
											objectProperties.foldout = true;
											
										}
									}
									//If the selected object have object properties script attached.
									if (selectedTransform.GetComponent<ObjectProperties>() != null)
									{
										ObjectProperties objectProperties = selectedTransform.gameObject.GetComponent<ObjectProperties>();
										objectProperties.foldout = EditorGUILayout.Foldout(objectProperties.foldout, objectProperties.gameObject.name);
										if (objectProperties.foldout)
										{
											
											GUILayout.Box("", GUILayout.MaxWidth(position.width - 20), GUILayout.Height(90));
											//If the object is not showing vertex material
											GUILayout.Space(-88.5f);
											GUILayout.BeginHorizontal();
											GUILayout.Space(10);
											GUILayout.BeginVertical();
											
											if (!selectedTransform.GetComponent<ObjectProperties>().showingVertex)
											{
												if (GUILayout.Button("Show Vertex Colors", GUILayout.MaxWidth(position.width - offsetButton)))
												{
													objectProperties.originalMaterial = selectedTransform.GetComponent<Renderer>().sharedMaterial;
													Undo.RecordObjects(new Object[] {
														selectedTransform.GetComponent<Renderer>(),
														objectProperties
													}, "Show Vertex Color");
													selectedTransform.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Vax Vertex Painter/Debug/Vertex Colors"));
													EditorUtility.SetDirty(selectedTransform.GetComponent<Renderer>());
													objectProperties.showingVertex = true;
												}
											}
											
											//If the object is showing vertex material
											if (selectedTransform.GetComponent<ObjectProperties>().showingVertex)
											{
												GUILayout.BeginHorizontal();
												GUILayout.Label("Show Vertex Colors: ", GUILayout.ExpandWidth(true));
												//Red Color Button
												if (GUILayout.Button("", EditorStyles.textField, GUILayout.Width(20), GUILayout.Height(20)))
												{
													if(!objectProperties.showingRedColor) {
														Undo.RecordObject(this, "Show Red Color");
														objectProperties.showingRedColor = !objectProperties.showingRedColor;
														selectedTransform.GetComponent<Renderer>().sharedMaterial.SetFloat("_RedChannel", 1);
														objectProperties.vertexColors[0] = Color.red;
													}
													else {
														Undo.RecordObject(this, "Hide Red Color");
														objectProperties.showingRedColor = !objectProperties.showingRedColor;
														selectedTransform.GetComponent<Renderer>().sharedMaterial.SetFloat("_RedChannel", 0);
														objectProperties.vertexColors[0] = Color.clear;
													}
												}
												EditorGUI.DrawRect(new Rect(GUILayoutUtility.GetLastRect().x + 1, GUILayoutUtility.GetLastRect().y + 1, 18, 18), objectProperties.vertexColors[0]);
												GUI.color = new Color(0.3f, 0, 0);
												GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + 3, GUILayoutUtility.GetLastRect().y + 2, GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().height), "R", EditorStyles.whiteLabel);
												GUI.color = Color.white;
												
												//Green Color Button
												if (GUILayout.Button("", EditorStyles.textField, GUILayout.Width(20), GUILayout.Height(20)))
												{
													if(!objectProperties.showingGreenColor) {
														Undo.RecordObject(this, "Show Green Color");
														objectProperties.showingGreenColor = !objectProperties.showingGreenColor;
														selectedTransform.GetComponent<Renderer>().sharedMaterial.SetFloat("_GreenChannel", 1);
														objectProperties.vertexColors[1] = Color.green;
													}
													else {
														Undo.RecordObject(this, "Hide Green Color");
														objectProperties.showingGreenColor = !objectProperties.showingGreenColor;
														selectedTransform.GetComponent<Renderer>().sharedMaterial.SetFloat("_GreenChannel", 0);
														objectProperties.vertexColors[1] = Color.clear;
													}
												}
												EditorGUI.DrawRect(new Rect(GUILayoutUtility.GetLastRect().x + 1, GUILayoutUtility.GetLastRect().y + 1, 18, 18), objectProperties.vertexColors[1]);
												GUI.color = new Color(0, 0.3f, 0);
												GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + 3, GUILayoutUtility.GetLastRect().y + 2, GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().height), "G", EditorStyles.whiteLabel);
												GUI.color = Color.white;
												
												//Blue Color Button
												if (GUILayout.Button("", EditorStyles.textField, GUILayout.Width(20), GUILayout.Height(20)))
												{
													if(!objectProperties.showingBlueColor) {
														Undo.RecordObject(this, "Show Blue Color");
														objectProperties.showingBlueColor = !objectProperties.showingBlueColor;
														selectedTransform.GetComponent<Renderer>().sharedMaterial.SetFloat("_BlueChannel", 1);
														objectProperties.vertexColors[2] = Color.blue;
													}
													else {
														Undo.RecordObject(this, "Hide Blue Color");
														objectProperties.showingBlueColor = !objectProperties.showingBlueColor;
														selectedTransform.GetComponent<Renderer>().sharedMaterial.SetFloat("_BlueChannel", 0);
														objectProperties.vertexColors[2] = Color.clear;
													}
												}
												EditorGUI.DrawRect(new Rect(GUILayoutUtility.GetLastRect().x + 1, GUILayoutUtility.GetLastRect().y + 1, 18, 18), objectProperties.vertexColors[2]);
												GUI.color = new Color(0, 0, 0.3f);
												GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + 3, GUILayoutUtility.GetLastRect().y + 2, GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().height), "B", EditorStyles.whiteLabel);
												GUI.color = Color.white;
												
												//Alpha Color Button
												GUI.DrawTexture(new Rect(GUILayoutUtility.GetLastRect().x + 24, GUILayoutUtility.GetLastRect().y, 20, 20), AssetDatabase.LoadAssetAtPath(folderPath + "Interface/alphaBackground.png", typeof(Texture2D)) as Texture2D);
												GUI.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
												if (GUILayout.Button("", EditorStyles.textField, GUILayout.Width(20), GUILayout.Height(20)))
												{
													if(!objectProperties.showingAlphaColor) {
														Undo.RecordObject(this, "Show Alpha Color");
														objectProperties.showingAlphaColor = !objectProperties.showingAlphaColor;
														selectedTransform.GetComponent<Renderer>().sharedMaterial.SetFloat("_AlphaChannel", 1);
														objectProperties.vertexColors[3] = new Color(0.85f, 0.85f, 0.85f, 1f);
													}
													else {
														Undo.RecordObject(this, "Hide Alpha Color");
														objectProperties.showingAlphaColor = !objectProperties.showingAlphaColor;
														selectedTransform.GetComponent<Renderer>().sharedMaterial.SetFloat("_AlphaChannel", 0);
														objectProperties.vertexColors[3] = Color.clear;
													}
												}
												GUI.color = new Color(0, 0, 0.3f);
												GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + 3, GUILayoutUtility.GetLastRect().y + 2, GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().height), "A", EditorStyles.label);
												GUI.color = Color.white;
												GUILayout.EndHorizontal();

												if (GUILayout.Button("Show Normal", GUILayout.MaxWidth(position.width - offsetButton)))
												{
													objectProperties = selectedTransform.gameObject.GetComponent<ObjectProperties>();
													Undo.RecordObjects(new Object[] {
														selectedTransform.GetComponent<Renderer>(),
														objectProperties
													}, "Show Normal Color");
													selectedTransform.GetComponent<Renderer>().sharedMaterial = objectProperties.originalMaterial;
													EditorUtility.SetDirty(selectedTransform.GetComponent<Renderer>());
													objectProperties.showingVertex = false;
													Resources.UnloadUnusedAssets();
												}

											}
											
											if (selectedTransform.GetComponent<ObjectProperties>().showingWireframe)
											{
												if (GUILayout.Button("Enable Wireframe", GUILayout.MaxWidth(position.width - offsetButton)))
												{
													Undo.RecordObjects(new Object[] {
														selectedTransform.GetComponent<Renderer>(),
														objectProperties
													}, "Show Wireframe");

													EditorUtility.SetSelectedWireframeHidden(selectedTransform.GetComponent<Renderer>(), false);
													EditorUtility.SetDirty(selectedTransform.GetComponent<Renderer>());
													objectProperties.showingWireframe = false;
												}
											}

											if (!selectedTransform.GetComponent<ObjectProperties>().showingWireframe)
											{
												if (GUILayout.Button("Disable Wireframe", GUILayout.MaxWidth(position.width - offsetButton)))
												{
													Undo.RecordObjects(new Object[] {
														selectedTransform.GetComponent<Renderer>(),
														objectProperties
													}, "Show Wireframe");

													EditorUtility.SetSelectedWireframeHidden(selectedTransform.GetComponent<Renderer>(), true);
													EditorUtility.SetDirty(selectedTransform.GetComponent<Renderer>());
													objectProperties.showingWireframe = true;
												}
											}
	
											if (GUILayout.Button("Copy Vertex Color", GUILayout.MaxWidth(position.width - offsetButton)))
											{
												
												colorClipboard = selectedTransform.GetComponent<MeshFilter>().sharedMesh.colors.ToList();
												
											}
											
											if (colorClipboard != null)
											{
												if (GUILayout.Button("Paste Vertex Color", GUILayout.MaxWidth(position.width - offsetButton)))
												{
													MeshFilter meshFilter = selectedTransform.GetComponent<MeshFilter>();
													Mesh meshClone = Mesh.Instantiate(meshFilter.sharedMesh) as Mesh;
													meshClone.name = meshFilter.sharedMesh.name;
													objectMesh = meshFilter.mesh = meshClone;
													objectProperties.instanceID = objectProperties.GetInstanceID();
													objectProperties.instance = true;
													
													if (colorClipboard.Count > meshFilter.sharedMesh.vertexCount)
													{
														int exceededVertex = colorClipboard.Count - meshFilter.sharedMesh.vertexCount;
														int indexList = colorClipboard.Count - exceededVertex;
														List<Color> tempList = colorClipboard;
														tempList.RemoveRange(indexList, exceededVertex);
														meshFilter.sharedMesh.colors = tempList.ToArray();
													}
													else
														meshFilter.sharedMesh.colors = colorClipboard.ToArray();
												}
											}
											GUILayout.EndHorizontal();
											GUILayout.EndVertical();
											GUILayout.Space(10);
											
										}
									}
								}
							}
						}
						
						GUILayout.Space(-2f);
						GUILayout.EndScrollView();
						GUILayout.Space(-4f);

						EditorGUIUtility.AddCursorRect (new Rect(GUILayoutUtility.GetLastRect().x-1,GUILayoutUtility.GetLastRect().y-5,position.width,5), MouseCursor.ResizeVertical);
						if (new Rect(GUILayoutUtility.GetLastRect().x-1,GUILayoutUtility.GetLastRect().y-5,position.width,5).Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
						{
								GUI.FocusControl("");
								resizingWindow = true;
						}
						
						if(resizingWindow){

							resizeOffset = Event.current.mousePosition.y-108;
							if (Event.current.type == EventType.MouseUp){
							resizingWindow = false;
							}
						}
						
				}
			}
			#endregion

			#region Main Menu - Color Properties
			colorPropertiesFoldout = DrawHeaderTitle("Color Properties", colorPropertiesFoldout, headerColor);
			if (colorPropertiesFoldout)
			{
				
				GUILayout.Space(10);
				
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Paint Color: ", EditorStyles.toggle, GUILayout.ExpandWidth(false)))
				{
					Undo.RecordObject(this, "Paint Color Value");
					paintMode = true;
					eraseMode = false;
				}
				if (paintMode)
					GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x+0.5f, GUILayoutUtility.GetLastRect().y, 20, 20), "X", EditorStyles.boldLabel);
				
				
				
				//Box containing the color of paintMode
				GUI.Box(new Rect(GUILayoutUtility.GetLastRect().x + GUILayoutUtility.GetLastRect().width - 1, GUILayoutUtility.GetLastRect().y - 1, 21, 21), "");
				//Alpha background label of paintMode color
				GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + GUILayoutUtility.GetLastRect().width - 2, GUILayoutUtility.GetLastRect().y - 1, 23, 23),
				          AssetDatabase.LoadAssetAtPath(folderPath + "Interface/alphaBackground.png", typeof(Texture2D)) as Texture2D);
				EditorGUI.DrawRect(new Rect(GUILayoutUtility.GetLastRect().x + GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().y, 19, 19), paintColor);
				
				//A dynamic space based on the window width
				GUILayout.Space(position.width - 222);
				
				//Swap button
				if (GUI.Button(new Rect(GUILayoutUtility.GetLastRect().x + 25, GUILayoutUtility.GetLastRect().y + 2, position.width - 250, 20), "Swap"))
				{
					Undo.RecordObject(this, "Swap Colors");
					//Here we save temporaly the paintColor for the change.
					Color tempColor = paintColor;
					paintColor = eraseColor;
					eraseColor = tempColor;
				}
				
				
				//Gui Toggle eraseMode
				if (GUILayout.Button("Erase Color: ", EditorStyles.toggle, GUILayout.ExpandWidth(false)))
				{
					Undo.RecordObject(this, "Erase Color Value");
					paintMode = false;
					eraseMode = true;
				}
				if (eraseMode)
					GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x+0.5f, GUILayoutUtility.GetLastRect().y, 20, 20), "X", EditorStyles.boldLabel);
				
				
				
				
				//Box containing the color of paintMode
				GUI.Box(new Rect(GUILayoutUtility.GetLastRect().x + GUILayoutUtility.GetLastRect().width - 1, GUILayoutUtility.GetLastRect().y - 1, 21, 21), "");
				//Alpha background label of paintMode color
				GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + GUILayoutUtility.GetLastRect().width - 2, GUILayoutUtility.GetLastRect().y - 1, 23, 23),
				          AssetDatabase.LoadAssetAtPath(folderPath + "Interface/alphaBackground.png", typeof(Texture2D)) as Texture2D);
				EditorGUI.DrawRect(new Rect(GUILayoutUtility.GetLastRect().x + GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().y, 19, 19), eraseColor);
				
				
				//Here we end the group of the colors.
				GUILayout.EndHorizontal();
				
				//A space of 5 pixels.
				GUILayout.Space(6);
				
				GUILayout.BeginHorizontal();
				float colorValue = 0;
				if (EditorGUIUtility.isProSkin)
					colorValue = 0.64f;
				else
					colorValue = 0.84f;

				//Red Color Button
				if (GUILayout.Button("", EditorStyles.textField, GUILayout.Width(20), GUILayout.Height(20)))
				{
					Undo.RecordObject(this, "Red Color Value");
					if (paintMode)
						paintColor = Color.red;
					if (eraseMode)
						eraseColor = Color.red;
				}
				EditorGUI.DrawRect(new Rect(GUILayoutUtility.GetLastRect().x + 1, GUILayoutUtility.GetLastRect().y + 1, 18, 18), new Color(colorValue, 0, 0));
				GUI.color = new Color(0.3f, 0, 0);
				GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + 3, GUILayoutUtility.GetLastRect().y + 2, GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().height), "R", EditorStyles.whiteLabel);
				GUI.color = Color.white;
				
				//Green Color Button
				if (GUILayout.Button("", EditorStyles.textField, GUILayout.Width(20), GUILayout.Height(20)))
				{
					Undo.RecordObject(this, "Green Color Value");
					if (paintMode)
						paintColor = Color.green;
					if (eraseMode)
						eraseColor = Color.green;
				}
				EditorGUI.DrawRect(new Rect(GUILayoutUtility.GetLastRect().x + 1, GUILayoutUtility.GetLastRect().y + 1, 18, 18), new Color(0, colorValue, 0));
				GUI.color = new Color(0, 0.3f, 0);
				GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + 3, GUILayoutUtility.GetLastRect().y + 2, GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().height), "G", EditorStyles.whiteLabel);
				GUI.color = Color.white;
				
				//Blue Color Button
				if (GUILayout.Button("", EditorStyles.textField, GUILayout.Width(20), GUILayout.Height(20)))
				{
					Undo.RecordObject(this, "Blue Color Value");
					if (paintMode)
						paintColor = Color.blue;
					if (eraseMode)
						eraseColor = Color.blue;
				}
				EditorGUI.DrawRect(new Rect(GUILayoutUtility.GetLastRect().x + 1, GUILayoutUtility.GetLastRect().y + 1, 18, 18), new Color(0, 0, colorValue));
				GUI.color = new Color(0, 0, 0.3f);
				GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + 3, GUILayoutUtility.GetLastRect().y + 2, GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().height), "B", EditorStyles.whiteLabel);
				GUI.color = Color.white;
				
				//Alpha Color Button
				GUI.DrawTexture(new Rect(GUILayoutUtility.GetLastRect().x + 24, GUILayoutUtility.GetLastRect().y, 20, 20), AssetDatabase.LoadAssetAtPath(folderPath + "Interface/alphaBackground.png", typeof(Texture2D)) as Texture2D);
				GUI.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
				if (GUILayout.Button("", EditorStyles.textField, GUILayout.Width(20), GUILayout.Height(20)))
				{
					Undo.RecordObject(this, "Alpha Color Value");
					if (paintMode)
						paintColor = Color.clear;
					if (eraseMode)
						eraseColor = Color.clear;
				}
				GUI.color = new Color(0.85f, 0.85f, 0.85f, 1f);
				GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + 3, GUILayoutUtility.GetLastRect().y + 2, GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().height), "A", EditorStyles.label);
				GUI.color = Color.white;

				GUILayout.BeginVertical();
				GUILayout.Space(6);
				//If we are on paintMode use that color for the colorField.
				if (paintMode)
					paintColor = EditorGUILayout.ColorField(paintColor);
				
				//If we are on eraseMode use that color for the colorField.
				if (eraseMode)
					eraseColor = EditorGUILayout.ColorField(eraseColor);
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
				GUILayout.Space(5);
			}
			#endregion

			#region Main Menu - Tool Properties
			toolPropertiesFoldout = DrawHeaderTitle("Tool Properties", toolPropertiesFoldout, headerColor);
			if (toolPropertiesFoldout)
			{
				GUILayout.Space(10);
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Brush", EditorStyles.toggle, GUILayout.ExpandWidth(false)))
				{
					brushMode = true;
					bucketMode = false;
				}
				if (brushMode)
					GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x+0.5f, GUILayoutUtility.GetLastRect().y, 20, 20), "X", EditorStyles.boldLabel);
				
				if (GUILayout.Button("Bucket", EditorStyles.toggle, GUILayout.ExpandWidth(false)))
				{
					brushMode = false;
					bucketMode = true;
				}
				if (bucketMode)
					GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x+0.5f, GUILayoutUtility.GetLastRect().y, 20, 20), "X", EditorStyles.boldLabel);
				GUILayout.EndHorizontal();
			}
			#endregion

			#region Main Menu - Brush Properties
			if (brushMode)
			{
				brushPropertiesFoldout = DrawHeaderTitle("Brush Properties", brushPropertiesFoldout, headerColor);
				if (brushPropertiesFoldout)
				{
					GUILayout.Space(10);
					
					//Brush Size property
					brushSize = DrawProperty("Brush Size", brushSize, 0, 8);
					
					//Brush Strenght property
					brushStrenght = DrawProperty("Brush Strenght", brushStrenght, 0, 1);
					
					//Brush FallOff Property
					//brushFallOff = DrawProperty("Brush Fall Off", brushFallOff, 0, 1);
					
					//Angle Limit Property
					angleLimit = DrawProperty("Angle Limit", angleLimit, 0, 180);
				}
			}
			#endregion

			#region Main Menu - Bucket Properties
			if (bucketMode)
			{
				bucketPropertiesFoldout = DrawHeaderTitle("Bucket Properties", bucketPropertiesFoldout, headerColor);
				if (bucketPropertiesFoldout)
				{
					GUILayout.Space(10);
					bucketSize = DrawProperty("Bucket Size", bucketSize, 0, 8);
				}
			}
			#endregion

			#region Main Menu - Paint Properties
			paintPropertiesFoldout = DrawHeaderTitle("Paint Properties", paintPropertiesFoldout, headerColor);
			if (paintPropertiesFoldout)
			{
				GUILayout.Space(5);
				
				GUILayout.BeginHorizontal();
				GUILayout.Label("Object Mode: ", GUILayout.ExpandWidth(false));
				
				//Gui Toggle allObjects
				if (GUILayout.Button("All", EditorStyles.toggle, GUILayout.ExpandWidth(false)))
				{
					allObjects = true;
					perObjects = false;
				}
				if (allObjects)
					GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x+0.5f, GUILayoutUtility.GetLastRect().y, 20, 20), "X", EditorStyles.boldLabel);
				
				//Gui Toggle perObjects
				if (GUILayout.Button("Per Object/s", EditorStyles.toggle, GUILayout.ExpandWidth(false)))
				{
					allObjects = false;
					perObjects = true;
				}
				if (perObjects)
					GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x+0.5f, GUILayoutUtility.GetLastRect().y, 20, 20), "X", EditorStyles.boldLabel);
				//End of the selection group
				GUILayout.EndHorizontal();
				
				GUILayout.Space(5);
				
				GUILayout.BeginHorizontal();
				GUILayout.Label("Paint Channel: ", GUILayout.ExpandWidth(false));
				
				//Red Channel Button
				if (rChannel)
					GUI.color = Color.red;
				if (GUILayout.Button("R", EditorStyles.toggle, GUILayout.ExpandWidth(false)))
				{
					Undo.RecordObject(this, "Red Channel Value");
					rChannel = !rChannel;
				}
				GUI.color = Color.white;

				//Green Channel Button
				if (gChannel)
					GUI.color = Color.green;
				if (GUILayout.Button("G", EditorStyles.toggle, GUILayout.ExpandWidth(false)))
				{
					Undo.RecordObject(this, "Green Channel Value");
					gChannel = !gChannel;
				}
				GUI.color = Color.white;

				//Blue Channel Button
				if (bChannel)
					GUI.color = Color.blue;
				if (GUILayout.Button("B", EditorStyles.toggle, GUILayout.ExpandWidth(false)))
				{
					Undo.RecordObject(this, "Blue Channel Value");
					bChannel = !bChannel;
				}
				GUI.color = Color.white;
				
				if (aChannel)
					GUI.color = Color.grey;
				if (GUILayout.Button("A", EditorStyles.toggle, GUILayout.ExpandWidth(false)))
				{
					Undo.RecordObject(this, "Alpha Channel Value");
					aChannel = !aChannel;
				}
				GUI.color = Color.white;
				
				GUILayout.EndHorizontal();
			}
			#endregion

			#region Main Menu - Gizmo Properties
			gizmoPropertiesFoldout = DrawHeaderTitle("Gizmo Properties", gizmoPropertiesFoldout, headerColor);
			if (gizmoPropertiesFoldout)
			{
				
				GUILayout.Space(5);
				handleColor = EditorGUILayout.ColorField("Handle Color", handleColor);
				outlineHandleColor = EditorGUILayout.ColorField("Outline Handle Color", outlineHandleColor);
				
				
				if (GUILayout.Button("Solid Handle", "Toggle"))
				{
					Undo.RecordObject(this, "Solid Handle Value");
					solidHandle = !solidHandle;
				}
				if (solidHandle)
					GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x+0.5f, GUILayoutUtility.GetLastRect().y, 20, 20), "X", EditorStyles.boldLabel);
				
				
				if (GUILayout.Button("Draw Handle Outline", "Toggle"))
				{
					Undo.RecordObject(this, "Draw Handle Outline Value");
					drawHandleOutline = !drawHandleOutline;
				}
				if (drawHandleOutline)
					GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x+0.5f, GUILayoutUtility.GetLastRect().y, 20, 20), "X", EditorStyles.boldLabel);
				
				if (GUILayout.Button("Draw Handle Angle", "Toggle"))
				{
					Undo.RecordObject(this, "Draw Handle Angle Value");
					drawHandleAngle = !drawHandleAngle;
				}
				if (drawHandleAngle)
					GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x+0.5f, GUILayoutUtility.GetLastRect().y, 20, 20), "X", EditorStyles.boldLabel);
				
			}
			#endregion

			GUILayout.Space(5);
			
			if (!painting)
			{
				
				//Button for painting
				if (GUILayout.Button("Start Painting"))
				{
					paintButton = true;
					painting = true;
					
					if (allObjects)
					{
						selectedTransforms.Clear();
						editedGameObjects.Clear();
					}
					
					if (perObjects)
					{
						selectedTransforms.Clear();
						editedGameObjects.Clear();
					}
				}
			}
			
			
			if (painting)
			{
				if (GUILayout.Button("Stop Painting"))
				{
					
					painting = false;
					paintButton = false;	
				}
			}
			GUILayout.EndScrollView();
		}
		#endregion
		
		#region Editor Menu
		if (menuOptionsIndex == 1)
		{
			scrollEditorPage = GUILayout.BeginScrollView(scrollEditorPage);
			GUILayout.Space(-5);
			GUILayout.Label("");
			GUILayout.Space(-16);
			
			hotkeysPropertiesFoldout = DrawHeaderTitle("Hotkeys", hotkeysPropertiesFoldout, headerColor);
			if (hotkeysPropertiesFoldout)
			{
				GUILayout.Space(5);
				paintHotkeyState = HotkeyFunction("Paint", paintHotkey, paintHotkeyState);
				GUILayout.Space(1);
				eraseHotkeyState = HotkeyFunction("Erase", eraseHotkey, eraseHotkeyState);
				GUILayout.Space(1);
				increaseSizeHotkeyState = HotkeyFunction("Increase Size", increaseSizeHotkey, increaseSizeHotkeyState);
				GUILayout.Space(1);
				decreaseSizeHotkeyState = HotkeyFunction("Decrease Size", decreaseSizeHotkey, decreaseSizeHotkeyState);
				GUILayout.Space(1);
				showVertexColorsHotkeyState = HotkeyFunction("Show Vertex Colors", showVertexColorsHotkey, showVertexColorsHotkeyState);
				GUILayout.Space(1);
				copyVertexColorsHotkeyState = HotkeyFunction("Copy Vertex Colors", copyVertexColorsHotkey, copyVertexColorsHotkeyState);
				GUILayout.Space(1);
				pasteVertexColorsHotkeyState = HotkeyFunction("Paste Vertex Colors", pasteVertexColorsHotkey, pasteVertexColorsHotkeyState);
				
			}
			
			/*editorPropertiesFoldout = DrawHeaderTitle("Editor", editorPropertiesFoldout, headerColor);
			if (editorPropertiesFoldout)
			{
				GUILayout.Space(5);
			}*/

			uninstallFoldout = DrawHeaderTitle("Uninstaller", uninstallFoldout, headerColor);
			if(uninstallFoldout){
				
				if(!tryingTouninstall || !tryingToClear){
				GUILayout.Space(5);
					GUILayout.Label("Since Vax Vertex Painter uses a script on gameobjects\nfor save data the only way to delete this asset is doing a\nuninstaller.\n\nIf you want to uninstall it first you need to use the clear\nbutton, use it in all the scenes you painted, when you are\ndone use the uninstall button.\n\nBoth buttons will ask you for a confirmation and will give\nyou some info");
					GUILayout.Space(10);
					GUILayout.Label("1- If you wish to uninstall first press the clear button:");
					if(GUILayout.Button("Clear Scripts")){
						tryingTouninstall = true;
						tryingToClear = true;
						tryingToClear2 = true;
					}

					GUILayout.Label("2- If you are done with clear press the uninstall button:");
					if(GUILayout.Button("Uninstall Vax Vertex Painter")){
						tryingToClear = true;
						tryingTouninstall = true;
						tryingTouninstall2 = true;
					}
				}
				
				if(tryingToClear2){
					GUILayout.Label("It seems that you are trying to clear the scripts,\nkeep in mind that the process will delete:\n\n-Saved data of objects (Not colors)");
					
					if(GUILayout.Button("I don't want to clear the scripts!")){
						tryingToClear = false;
						tryingToClear2 = false;
					}
					if(GUILayout.Button("I read all the info and i won't blame the developer for\n lose anything.")){
						tryingToClear2 = false;
						tryingToClear3 = true;
					}
				}

				if(tryingToClear3){
					GUILayout.Label("The steps for clear are:\n1- Open the scene in which you painted objects\n2- Press clear button\n3-Save scene and if you need, open other and repeat");
					
					if(GUILayout.Button("I don't want to clear the scripts!")){
						tryingToClear = false;
						tryingToClear2 = false;
						tryingToClear3 = false;
						tryingTouninstall = false;
					}
					if(GUILayout.Button("Clear scripts from this scene")){
						finalClear = true;
					}
					if(GUILayout.Button("I finished!")){
						tryingToClear = false;
						tryingToClear2 = false;
						tryingToClear3 = false;
						tryingTouninstall = false;
					}
				}

				if(finalClear){
					ObjectPropertiesEditor.UninstallIt();
					finalClear = false;
				}

				if(tryingTouninstall2){
					GUILayout.Label("It seems that you are trying to delete Vax Vertex Painter,\nkeep in mind that the process will delete:\n\n-Saved data of objects\n-The entire Vax Vertex Painter folder\n-Shaders of Vax Vertex Painter");

					if(GUILayout.Button("I don't want to uninstall Vax Vertex Painter!")){
						tryingTouninstall = false;
						tryingTouninstall2 = false;
						tryingToClear = false;
					}
					if(GUILayout.Button("I read all the info and i won't blame the developer for\n lose anything.")){
						tryingTouninstall = true;
						tryingTouninstall2 = false;
						tryingTouninstall3 = true;
					}
				}

				if(tryingTouninstall3){
					GUILayout.Label("The developer will miss you :(\nPress the button if you really want to uninstall it");
					if(GUILayout.Button("I don't want to uninstall Vax Vertex Painter!")){
						tryingTouninstall = false;
						tryingTouninstall2 = false;
						tryingTouninstall3 = false;
						tryingToClear = false;
					}

					if(GUILayout.Button("Uninstall it!")){
						tryingTouninstall = false;
						tryingTouninstall2 = false;
						tryingTouninstall3 = false;
						tryingToClear = false;
						finaluninstallation = true;
					}
				}

				if(finaluninstallation){
									

				}
			}
			GUILayout.EndScrollView();
		}
		#endregion
		
		#region Help Menu
		if (menuOptionsIndex == 2)
		{
			
			scrollHelpPage = GUILayout.BeginScrollView(scrollHelpPage, GUIStyle.none, "VerticalScrollBar");
			GUILayout.Space(-5);
			GUILayout.Label("");
			GUILayout.Space(-16);
			
			questionsFoldout = DrawHeaderTitle("Questions", questionsFoldout, headerColor);
			if (questionsFoldout)
			{
				GUILayout.Space(5);
				
				GUILayout.Label("Write down your question.\nPlease try to explain it with details.\nIt would be good if you add at least a smiley face :)");
				questionString = GUILayout.TextArea(questionString);
				
				GUILayout.Label("Write down your order number.\nYou can get it from the confirmation mail of the purchase");
				orderNumber = EditorGUILayout.TextField(orderNumber);
				
				if (GUILayout.Button("Make the question"))
				{
					string tempString = questionString.Replace("\n", "%0A");
					Application.OpenURL("mailto:eduardowagener@gmail.com?Subject=Question - Vax Vertex Painter&body=" + tempString + "%0AOrderNumber: " + orderNumber);
				}
			}
			
			suggestionsFoldout = DrawHeaderTitle("Suggestions", suggestionsFoldout, headerColor);
			if (suggestionsFoldout)
			{
				GUILayout.Space(5);
				
				GUILayout.Label("Write down your suggestion.\nPlease try to explain it with details.\nIt would be good if you add at least a smiley face :)");
				suggestionString = GUILayout.TextArea(suggestionString);
				
				GUILayout.Label("Write down your order number.\nYou can get it from the confirmation mail of the purchase");
				orderNumber = EditorGUILayout.TextField(orderNumber);
				
				if (GUILayout.Button("Make the suggestion"))
				{
					string tempString = suggestionString.Replace("\n", "%0A");
					Application.OpenURL("mailto:eduardowagener@gmail.com?Subject=Suggestion - Vax Vertex Painter&body=" + tempString + "%0AOrderNumber: " + orderNumber);
				}
			}
			
			bugsFoldout = DrawHeaderTitle("Bugs", bugsFoldout, headerColor);
			if (bugsFoldout)
			{
				GUILayout.Space(5);
				
				GUILayout.Label("Write down the bug/error you want to report.\nPlease try to explain it with details.\nI will try to fix it as soon as i can, and thanks!");
				bugString = GUILayout.TextArea(bugString);
				
				GUILayout.Label("Write down your order number.\nYou can get it from the confirmation mail of the purchase");
				orderNumber = EditorGUILayout.TextField(orderNumber);
				
				if (GUILayout.Button("Make the bug/error report"))
				{
					string tempString = bugString.Replace("\n", "%0A");
					Application.OpenURL("mailto:eduardowagener@gmail.com?Subject=Bug Report - Vax Vertex Painter&body=" + tempString + "%0AOrderNumber: " + orderNumber);
				}
			}
			
			
			GUILayout.EndScrollView();
			
			
		}
		#endregion
			
		if(GUI.changed){
			guiChanged = true;
		} 
			Repaint();
		}	

		#region Save Variables	

		void OnDisable()
		{
			if (SceneView.onSceneGUIDelegate == this.OnSceneGUI) SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            if (EditorApplication.playmodeStateChanged == HandleOnPlayModeChanged) { EditorApplication.playmodeStateChanged -= HandleOnPlayModeChanged; }
            SaveVariables();
		}

		void OnDestroy()
		{
			if (SceneView.onSceneGUIDelegate == this.OnSceneGUI) SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
			SaveVariables();
		}

		void OnLostFocus(){
			if(guiChanged){
				SaveVariables();
				guiChanged = false;
			}
		}


		#endregion	
		void OnSceneGUI(SceneView sceneView)
		{
		//Hotkeys 
		#region Hotkeys Function
		
		//Paint Hotkey
		if (paintHotkeyState && !paintButton)
		{
			if (Event.current.keyCode == paintHotkey.keyCode && Event.current.control == paintHotkey.control && Event.current.shift == paintHotkey.shift && Event.current.alt == paintHotkey.alt)
			{
				painting = true;
				paintMode = true;
				eraseMode = false;
			}

			else{
				if(!eraseMode){
				painting = false;
				paintMode = false;
				}
			}
		}

		//Erase Hotkey
		if(eraseHotkeyState && !paintButton){
			if(Event.current.keyCode == eraseHotkey.keyCode && Event.current.control == eraseHotkey.control &&  Event.current.shift == eraseHotkey.shift && Event.current.alt == eraseHotkey.alt){
				painting = true;
				paintMode = false;
				eraseMode = true;
				
			}	
			else{
				if(!paintMode){
				painting = false;
				eraseMode = false;
				}
			}
		}

		if(!paintMode && !eraseMode) paintMode = true;

		//Increase Size Hotkey
		if(increaseSizeHotkeyState){
			if(Event.current.keyCode == increaseSizeHotkey.keyCode && Event.current.control == increaseSizeHotkey.control &&  Event.current.shift == increaseSizeHotkey.shift && Event.current.alt == increaseSizeHotkey.alt){
				if(brushMode)brushSize = brushSize+0.02f;
				if(bucketMode)bucketSize = bucketSize+0.02f;
			 }
		}

		//Decrease Size Hotkey
		if(decreaseSizeHotkeyState){
			if(Event.current.keyCode == decreaseSizeHotkey.keyCode && Event.current.control == decreaseSizeHotkey.control &&  Event.current.shift == decreaseSizeHotkey.shift && Event.current.alt == decreaseSizeHotkey.alt){
				if(brushMode)brushSize = brushSize-0.02f;
				if(bucketMode)bucketSize = bucketSize-0.02f;
			}
		}
		
		//Show Vertex Colors Hotkey
		if(showVertexColorsHotkeyState){
		    if(Event.current.keyCode == showVertexColorsHotkey.keyCode && Event.current.control == showVertexColorsHotkey.control &&  Event.current.shift == showVertexColorsHotkey.shift && Event.current.alt == showVertexColorsHotkey.alt){
				if(Event.current.keyCode.ToString() != "null" && Event.current.type != EventType.KeyUp) return;
		        if(Selection.activeGameObject != null){
					GameObject selectedTransform = Selection.activeGameObject;
					if(selectedTransform.GetComponent<MeshFilter>() != null){
						if(selectedTransform.GetComponent<ObjectProperties>() == null){
							ObjectProperties objectPropertiesScript = selectedTransform.AddComponent<ObjectProperties>();
			                    objectPropertiesScript.originalMaterial = Selection.activeTransform.GetComponent<Renderer>().sharedMaterial;
			                    Undo.RecordObjects (new Object[] {Selection.activeTransform.GetComponent<Renderer>(),objectPropertiesScript}, "Show Vertex Color");
								Selection.activeTransform.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Vax Vertex Painter/Debug/Vertex Colors"));
			                    EditorUtility.SetDirty (Selection.activeTransform.GetComponent<Renderer>());
			                    objectPropertiesScript.showingVertex = true;
		               		 }
		           		 }
		
						if(selectedTransform.GetComponent<ObjectProperties>() != null){
							ObjectProperties objectPropertiesScript = selectedTransform.GetComponent<ObjectProperties>();
		                    if(!objectPropertiesScript.showingVertex){
									objectPropertiesScript.originalMaterial = selectedTransform.GetComponent<Renderer>().sharedMaterial;
									Undo.RecordObjects (new Object[] { selectedTransform.GetComponent<Renderer>(),objectPropertiesScript}, "Show Vertex Color");
									selectedTransform.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Vax Vertex Painter/Debug/Vertex Colors"));
									EditorUtility.SetDirty (selectedTransform.GetComponent<Renderer>());
		                            objectPropertiesScript.showingVertex = true;
									return;
		                    }
									
		                    //If the object is showing vertex material
		                    if(selectedTransform.GetComponent<ObjectProperties>().showingVertex){
									objectPropertiesScript =  selectedTransform.gameObject.GetComponent<ObjectProperties>();
									Undo.RecordObjects (new Object[] { selectedTransform.GetComponent<Renderer>(),objectPropertiesScript}, "Show Normal Color");
									selectedTransform.GetComponent<Renderer>().sharedMaterial = objectPropertiesScript.originalMaterial;
									EditorUtility.SetDirty (selectedTransform.GetComponent<Renderer>());
		                            objectPropertiesScript.showingVertex = false;
									Resources.UnloadUnusedAssets();
									return;
		                    }
		                }
		            }
		        }
		    }

		//Copy Vertex Colors Hotkey
		if(copyVertexColorsHotkeyState){
		    if(Event.current.keyCode == copyVertexColorsHotkey.keyCode && Event.current.control == copyVertexColorsHotkey.control &&  Event.current.shift == copyVertexColorsHotkey.shift && Event.current.alt == copyVertexColorsHotkey.alt){
				if(Selection.activeTransform != null){
					GameObject selectedTransform = Selection.activeGameObject;
					if(selectedTransform.GetComponent<MeshFilter>() != null){
						colorClipboard = selectedTransform.GetComponent<MeshFilter>().sharedMesh.colors.ToList();
		            }
		        }
		    }
		}
		
		//Paste Vertex Colors Hotkey
		if(pasteVertexColorsHotkeyState){
		    if(Event.current.keyCode == pasteVertexColorsHotkey.keyCode && Event.current.control == pasteVertexColorsHotkey.control &&  Event.current.shift == pasteVertexColorsHotkey.shift && Event.current.alt == pasteVertexColorsHotkey.alt){
		        if(Selection.activeTransform != null){
					GameObject selectedTransform = Selection.activeGameObject;
					if(selectedTransform.GetComponent<MeshFilter>() != null){
						colorClipboard = selectedTransform.GetComponent<MeshFilter>().sharedMesh.colors.ToList();
		            }
		        }
		    }
		}
		#endregion
		
		if (painting)
		{
			//Disable click object when painting
			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
			if(!toolChange){
				toolSelected = Tools.current;
				Tools.current = Tool.None;
				toolChange = true;
			}
			
			//If we move the mouse or we drag the mouse
			if (Event.current.type == EventType.MouseMove && Event.current.button == 0 || Event.current.type == EventType.MouseDrag && Event.current.button == 0)
			{
				//Cast a ray from the scene
				ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
				hit = new RaycastHit();
				
				//If the ray hits something
				if (Physics.Raycast(ray, out hit, 1000.0f))
				{
					//If allObjects true
					if (allObjects)
						hitting = true;
					
					//If perObjects true
					if (perObjects)
					{
						
						selectedTransforms = Selection.gameObjects.ToList();
						if (selectedTransforms.Contains(hit.collider.gameObject))
							hitting = true;
						else
							hitting = false;
					}
				}
				//If the ray don't hit nothing
				else
					hitting = false;
			}
			
			//If we are hitting something
			if (hitting)
				Paint();
		}
	
		else{
			if(toolChange){
			Tools.current = toolSelected;
			toolChange = false;
			}
		}

	}	
	//Path function
	string GetScriptPath()
	{
		string scriptFilePath;
		MonoScript ms = MonoScript.FromScriptableObject(this);
		scriptFilePath = AssetDatabase.GetAssetPath(ms);
		return scriptFilePath;
	}	
	#region Paint Function
	public void Paint()
	{
		
		SceneView.RepaintAll();
		if (solidHandle)
		{
			Handles.color = handleColor;
			Handles.DrawSolidDisc(hit.point, hit.normal, brushSize);
			Handles.color = handleColor;
			Handles.DrawSolidDisc(hit.point, hit.normal, brushFallOff + brushSize);
		}
		else
		{
			Handles.color = handleColor;
			Handles.DrawWireDisc(hit.point, hit.normal, brushSize);
			Handles.color = handleColor;
			Handles.DrawWireDisc(hit.point, hit.normal, brushFallOff + brushSize);
		}
		if (drawHandleOutline)
		{
			Handles.color = outlineHandleColor;
			Handles.DrawWireDisc(hit.point, hit.normal, brushSize - 0.005f);
		}
		if (drawHandleAngle)
		{	
			Handles.Label(new Vector3(hit.point.x - 0.12f, hit.point.y + 0.25f, hit.point.z), Vector3.Angle(hit.normal, Vector3.up).ToString("#.##"), EditorStyles.largeLabel);
		}
		
		if(Event.current.commandName == "UndoRedoPerformed"){
			for (int i = 0; i < editedGameObjects.Count; i++) {
				Color[] tempColor = editedGameObjects[i].GetComponent<MeshFilter>().sharedMesh.colors;
				editedGameObjects[i].GetComponent<MeshFilter>().sharedMesh.colors = new Color[tempColor.Length];
				editedGameObjects[i].GetComponent<MeshFilter>().sharedMesh.colors = tempColor;	
			}
		}

		//Actual Paint
		if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
		{
			if (hit.collider.gameObject.GetComponent<ObjectProperties>() == null)
			{
				ObjectProperties objectProperties = hit.collider.gameObject.AddComponent<ObjectProperties>();
				objectProperties.instanceID = objectProperties.GetInstanceID();
			}
			else
			{
				ObjectProperties objectProperties = hit.collider.gameObject.GetComponent<ObjectProperties>();
				
				if (!objectProperties.instance && instanceMode)
				{
					MeshFilter meshFilter = hit.collider.gameObject.GetComponent<MeshFilter>();
					Mesh meshClone = Mesh.Instantiate(meshFilter.sharedMesh) as Mesh;
					meshClone.name = meshFilter.sharedMesh.name;
					objectMesh = meshFilter.mesh = meshClone;
					objectProperties.instanceID = objectProperties.GetInstanceID();
					objectProperties.instance = true;
				}
				
				if (objectProperties.instance && objectProperties.instanceID != objectProperties.GetInstanceID() && instanceMode)
				{
					objectProperties.instance = false;
					MeshFilter meshFilter = hit.collider.gameObject.GetComponent<MeshFilter>();
					Mesh meshClone = Mesh.Instantiate(meshFilter.sharedMesh) as Mesh;
					meshClone.name = meshFilter.sharedMesh.name;
					objectMesh = meshFilter.mesh = meshClone;
					objectProperties.instanceID = objectProperties.GetInstanceID();
					objectProperties.instance = true;
					objectProperties.meshCreated = false;
					objectProperties.meshName = null;
				}

				if (!objectProperties.instance && assetMode)
				{
					MeshFilter meshFilter = hit.collider.gameObject.GetComponent<MeshFilter>();
					Mesh meshClone = Mesh.Instantiate(meshFilter.sharedMesh) as Mesh;
					meshClone.name = meshFilter.sharedMesh.name;
					objectMesh = meshFilter.mesh = meshClone;
					objectProperties.instanceID = objectProperties.GetInstanceID();
					objectProperties.instance = true;
				}
				
				if (objectProperties.instance && objectProperties.instanceID != objectProperties.GetInstanceID() && assetMode)
				{
					MeshFilter meshFilter = hit.collider.gameObject.GetComponent<MeshFilter>();
					objectMesh = meshFilter.sharedMesh;
				}

			}
			
			objectMesh = hit.collider.GetComponent<MeshFilter>().sharedMesh;
			
			if (!editedGameObjects.Contains(hit.collider.gameObject))
			{
				editedGameObjects.Add(hit.collider.gameObject);
			}
		}

		//Brush Tool
		if (brushMode)
		{	
			if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
			{

				if(meshVertex == null || meshVertex.Length != objectMesh.vertices.Length) {
					meshVertex = objectMesh.vertices;
				}

				meshColors = objectMesh.colors;
		
				if (objectMesh.colors.Length != objectMesh.vertices.Length)
				{
					objectMesh.colors = new Color[objectMesh.vertexCount];
					meshColors = objectMesh.colors;
				}
				
				pos = hit.collider.transform.InverseTransformPoint(hit.point);
				
				if (Vector3.Angle(hit.normal, Vector3.up) < angleLimit)
				{
					for (int i = 0; i < meshVertex.Length; i++)
					{
						posMagnitude = (meshVertex[i] - pos).magnitude*hit.collider.transform.localScale.magnitude;
						
						if (posMagnitude > brushSize) continue;

						if ((Vector4)meshColors[i] == redVector && rChannel || (Vector4)meshColors[i] == greenVector && gChannel || (Vector4)meshColors[i] == blueVector && bChannel || meshColors[i].a == 0 && aChannel || rChannel && gChannel && bChannel && meshColors[i].a != 0 || rChannel && gChannel && bChannel && aChannel)
						{ 
							vColor = Color.white;
							if(paintMode) vColor = Color.Lerp(meshColors[i], paintColor, brushStrenght);
							if(eraseMode) vColor = Color.Lerp(meshColors[i], eraseColor, brushStrenght);
							meshColors[i] = vColor;
						}
					}
					Undo.RegisterCompleteObjectUndo	(objectMesh, "Vertex Paint");
					objectMesh.colors = meshColors;
					EditorUtility.SetDirty(objectMesh);
				}
			}
		}
		
		//Bucket Tool
		if (bucketMode)
		{
			if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
			{
				meshVertex = objectMesh.vertices;
				meshColors = objectMesh.colors;
				
				if (objectMesh.colors.Length != objectMesh.vertexCount)
				{
					objectMesh.colors = new Color[objectMesh.vertexCount];
					meshColors = objectMesh.colors;
				}
				
				pos = hit.collider.transform.InverseTransformPoint(hit.point);
				
				if (Vector3.Angle(hit.normal, Vector3.up) < angleLimit)
				{
					for (int i = 0; i < meshVertex.Length; i++)
					{
						posMagnitude = (meshVertex[i] - pos).magnitude;
						
						if (posMagnitude > bucketSize) continue;

						if ((Vector4)meshColors[i] == redVector && rChannel)
						{
							for (int r = 0; r < meshColors.Length; r++)
							{
								if ((Vector4)meshColors[r] == redVector)
								{
									Color newColor = Color.Lerp(meshColors[r], paintColor, 1);
									meshColors[r] = newColor;
								}
							}
						}
						
						if ((Vector4)meshColors[i] == greenVector && gChannel)
						{
							for (int g = 0; g < meshColors.Length; g++)
							{
								if ((Vector4)meshColors[g] == greenVector)
								{
									Color newColor = Color.Lerp(meshColors[g], paintColor, 1);
									meshColors[g] = newColor;
								}
							}
						}
						
						if ((Vector4)meshColors[i] == blueVector && bChannel)
						{
							for (int b = 0; b < meshColors.Length; b++)
							{
								if ((Vector4)meshColors[b] == blueVector)
								{
									Color newColor = Color.Lerp(meshColors[b], paintColor, 1);
									meshColors[b] = newColor;
								}
							}
						}
						
						if (meshColors[i].a == 0 && aChannel)
						{
							for (int a = 0; a < meshColors.Length; a++)
							{
								if (meshColors[a].a == 0)
								{
									Color newColor = Color.Lerp(meshColors[a], paintColor, 1);
									meshColors[a] = newColor;
								}
							}
						}
						
					}
					//	Undo.RecordObject(hit.collider.gameObject.GetComponent<MeshFilter>().sharedMesh, "Adjust Terrain");
					objectMesh.colors = meshColors;
					//	EditorUtility.SetDirty(hit.collider.gameObject);
				}
			}
		}
		SaveMesh();
	}
	#endregion
	public void SaveMesh()
	{
		if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
		{
			if (instanceMode)
			{
				for (int i = 0; i < editedGameObjects.Count; i++) {
					EditorUtility.SetDirty(editedGameObjects[i]);
				}
			}
			
			if (assetMode)
			{
				for (int i = 0; i < editedGameObjects.Count; i++) {
					if(string.IsNullOrEmpty(editedGameObjects[i].GetComponent<ObjectProperties>().meshName)){
						var filePath = EditorUtility.SaveFilePanelInProject("Save Mesh as Asset","","asset","");

						if(!string.IsNullOrEmpty(filePath)) {
							var editedObject = editedGameObjects[i];
							var editedMesh = editedObject.GetComponent<MeshFilter>().sharedMesh;
							Mesh createdMesh = AssetDatabase.LoadMainAssetAtPath(filePath) as Mesh;
							if(createdMesh != null){
									
								AssetDatabase.CreateAsset(editedMesh,filePath.Replace(".asset","temporalFileVaxVertex132.asset"));
								FileUtil.ReplaceFile(filePath.Replace(".asset","temporalFileVaxVertex132.asset"),filePath);
								AssetDatabase.DeleteAsset(filePath.Replace(".asset","temporalFileVaxVertex132.asset"));
								editedObject.GetComponent<MeshFilter>().sharedMesh = AssetDatabase.LoadAssetAtPath(filePath,typeof(Mesh)) as Mesh;
								AssetDatabase.SaveAssets();
								AssetDatabase.Refresh();
							}
							else{
								AssetDatabase.CreateAsset(editedMesh,filePath);
							}
							editedObject.GetComponent<ObjectProperties>().meshCreated = true;
							int pos = filePath.LastIndexOf("/") + 1;
							editedObject.GetComponent<ObjectProperties>().meshName = filePath.Substring(pos, filePath.Length - pos);
							AssetDatabase.LoadAssetAtPath(filePath,typeof(Mesh)).name = filePath.Substring(pos, filePath.Length - pos).Replace(".asset","");
						}

					}
				}
			}
			objectMesh = null;
			Resources.UnloadUnusedAssets();
		}
	}
	public void DrawHeaderTitle(string title, Color backgroundColor)
	{
		
		GUILayout.Space(0);
		GUI.color = backgroundColor;
		GUI.Box(new Rect(1, GUILayoutUtility.GetLastRect().y + 4, position.width, 27), "");
		GUI.color = Color.white;
		GUILayout.Space(4);
		GUILayout.Label(title, EditorStyles.largeLabel);
	}	
	public bool DrawHeaderTitle(string title, bool foldoutProperty, Color backgroundColor)
	{
		
		GUILayout.Space(0);
        Rect lastRect = GUILayoutUtility.GetLastRect();
		GUI.Box(new Rect(1, lastRect.y + 4, position.width, 27), "");
        lastRect = GUILayoutUtility.GetLastRect();
		EditorGUI.DrawRect(new Rect(lastRect.x, lastRect.y + 5f, position.width + 1, 25f), headerColor);
		GUILayout.Space(4);
		
		GUILayout.Label(title, EditorStyles.largeLabel);
		GUI.color = Color.clear;

        lastRect = GUILayoutUtility.GetLastRect();
		if (GUI.Button(new Rect(0, lastRect.y - 4, position.width, 27), ""))
		{
			foldoutProperty = !foldoutProperty;
		}
		GUI.color = Color.white;
		return foldoutProperty;
	}	
	public float DrawProperty(string propertyName, float valueName, float minValue, float maxValue)
	{
		GUILayout.BeginHorizontal();
		//Label of the property
		GUILayout.Label(propertyName + ": ", GUILayout.ExpandWidth(false));
		
		//Float field
		GUILayout.Label("");
        Rect lastrect = GUILayoutUtility.GetLastRect();
		if (lastrect.Contains(Event.current.mousePosition))
		{
			if (Event.current.isKey)
			{
				Undo.RecordObject(this, propertyName+" Value");
			}
		}
		valueName = EditorGUI.FloatField(lastrect, valueName);
		GUILayout.EndHorizontal();
		
		//Hacky stuff for see if you are clicking the slider for get the undo.
		GUILayout.Label("");
        lastrect = GUILayoutUtility.GetLastRect();
		if (lastrect.Contains(Event.current.mousePosition))
		{
			if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
			{
				GUI.FocusControl("");
				Undo.RecordObject(this, propertyName+" Value");
			}
				
		}
		
		//Slider 
		valueName = GUI.HorizontalSlider(lastrect, valueName, minValue, maxValue);
		return valueName;
		}
	public bool HotkeyFunction(string nameOfAction, Event hotkeyVariable, bool hotkeyState)
	{
		
		GUILayout.Label(nameOfAction + ": ", GUILayout.ExpandWidth(false));
		GUILayout.BeginHorizontal();
		if (hotkeyState)
		{
			if (GUILayout.Button(hotkeyVariable.keyCode.ToString()))
			{
				
				if (!waitingForInput)
				{
					nameOfInput = nameOfAction;
					waitingForInput = true;
				}
				else
					nameOfInput = nameOfAction;
				
			}
			
			if (GUILayout.Button("Control", EditorStyles.toggle, GUILayout.ExpandWidth(false)))
			{
				if (!hotkeyVariable.control)
					hotkeyVariable.control = true;
				else
					hotkeyVariable.control = false;
	
			}
			if (hotkeyVariable.control)
				GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x+0.5f, GUILayoutUtility.GetLastRect().y, 20, 20), "X", EditorStyles.boldLabel);
			
			if (GUILayout.Button("Shift", EditorStyles.toggle, GUILayout.ExpandWidth(false)))
			{
				Undo.RecordObject(this, nameOfAction+" Hotkey Shift Value");
				if (!hotkeyVariable.shift)
					hotkeyVariable.shift = true;
				else
					hotkeyVariable.shift = false;
				
			}
			if (hotkeyVariable.shift)
				GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x+0.5f, GUILayoutUtility.GetLastRect().y, 20, 20), "X", EditorStyles.boldLabel);
			
			if (Application.platform == RuntimePlatform.WindowsEditor)
			{
				if (GUILayout.Button("Alt", EditorStyles.toggle, GUILayout.ExpandWidth(false)))
				{
					Undo.RecordObject(this, nameOfAction+" Hotkey Alt Value");
					if (!hotkeyVariable.alt)
						hotkeyVariable.alt = true;
					else
						hotkeyVariable.alt = false;
				}
				if (hotkeyVariable.alt)
					GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x+0.5f, GUILayoutUtility.GetLastRect().y, 20, 20), "X", EditorStyles.boldLabel);
			}
			
			if (Application.platform == RuntimePlatform.OSXEditor)
			{
				if (GUILayout.Button("Command", EditorStyles.toggle, GUILayout.ExpandWidth(false)))
				{	
					Undo.RecordObject(this, nameOfAction+" Hotkey Command Value");
					if (!hotkeyVariable.command)
						hotkeyVariable.command = true;
					else
						hotkeyVariable.command = false;
				}
				if (hotkeyVariable.command)
					GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x+0.5f, GUILayoutUtility.GetLastRect().y, 20, 20), "X", EditorStyles.boldLabel);
			}
		}
		
		if (GUILayout.Button("Enable", EditorStyles.toggle, GUILayout.ExpandWidth(false)))
		{
				Undo.RecordObject(this, nameOfAction+" Hotkey Value");
			hotkeyState = !hotkeyState;
		}
		if (hotkeyState)
			GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x+0.5f, GUILayoutUtility.GetLastRect().y, 20, 20), "X", EditorStyles.boldLabel);
		
		GUILayout.EndHorizontal();
		
		if (waitingForInput && Event.current.isKey && Event.current.type == EventType.KeyUp && nameOfInput == nameOfAction)
		{
			Undo.RecordObject(this, nameOfAction+" Hotkey Keycode Value");
			if (Event.current.keyCode != KeyCode.Escape)
			{
				hotkeyVariable.keyCode = Event.current.keyCode;
				waitingForInput = false;
				Event.current.Use();
			}
			else
			{
				hotkeyVariable.keyCode = KeyCode.None;
				waitingForInput = false;
				Event.current.Use();
			}
		}
		return hotkeyState;
	}
	void HandleOnPlayModeChanged()
		{
			// This method is run whenever the playmode state is changed.
			
			if (EditorApplication.isPlayingOrWillChangePlaymode)
			{
				SaveVariables();
			}

		}
	void SaveVariables(){
			//Object properties
			serializedScript.instanceMode = instanceMode;
			serializedScript.assetMode = assetMode;
			
			//Color properties
			serializedScript.paintMode = paintMode;
			serializedScript.eraseMode = eraseMode;
			serializedScript.brushMode = brushMode;
			serializedScript.bucketMode = bucketMode;
			serializedScript.paintColor = paintColor;
			serializedScript.eraseColor = eraseColor;
			
			//Brush properties
			serializedScript.brushSize = brushSize;
			serializedScript.brushStrenght = brushStrenght;
			serializedScript.brushFallOff = brushFallOff;
			serializedScript.angleLimit = angleLimit;
			
			//Bucket Properties
			serializedScript.bucketSize = bucketSize;
			
			//Paint properties
			serializedScript.perObjects = perObjects;
			serializedScript.allObjects = allObjects;
			serializedScript.rChannel = rChannel;
			serializedScript.gChannel = gChannel;
			serializedScript.bChannel = bChannel;
			serializedScript.aChannel = aChannel;
			
			//Gizmo properties
			serializedScript.handleColor = handleColor;
			serializedScript.outlineHandleColor = outlineHandleColor;
			serializedScript.solidHandle = solidHandle;
			serializedScript.drawHandleOutline = drawHandleOutline;
			serializedScript.drawHandleAngle = drawHandleAngle;
			
			//Foldouts
			serializedScript.objectPropertiesFoldout = objectPropertiesFoldout;
			serializedScript.colorPropertiesFoldout = colorPropertiesFoldout;
			serializedScript.toolPropertiesFoldout = toolPropertiesFoldout;
			serializedScript.paintPropertiesFoldout = paintPropertiesFoldout;
			serializedScript.gizmoPropertiesFoldout = gizmoPropertiesFoldout;
			serializedScript.hotkeysPropertiesFoldout = hotkeysPropertiesFoldout;
			serializedScript.uninstallerFoldout = uninstallFoldout;
			serializedScript.questionsFoldout = questionsFoldout;
			serializedScript.suggestionsFoldout = suggestionsFoldout;
			serializedScript.bugsFoldout = bugsFoldout;

			//Hotkeys
			serializedScript.paintHotkey = new SerializedWindowProperties.Hotkey(paintHotkey.keyCode,paintHotkey.control,paintHotkey.shift,paintHotkey.alt);
			serializedScript.eraseHotkey =  new SerializedWindowProperties.Hotkey(eraseHotkey.keyCode,eraseHotkey.control,eraseHotkey.shift,eraseHotkey.alt);
			serializedScript.increaseSizeHotkey = new SerializedWindowProperties.Hotkey(increaseSizeHotkey.keyCode,increaseSizeHotkey.control,increaseSizeHotkey.shift,increaseSizeHotkey.alt);
			serializedScript.decreaseSizeHotkey = new SerializedWindowProperties.Hotkey(decreaseSizeHotkey.keyCode,decreaseSizeHotkey.control,decreaseSizeHotkey.shift,decreaseSizeHotkey.alt);		
			serializedScript.showVertexColorsHotkey = new SerializedWindowProperties.Hotkey(showVertexColorsHotkey.keyCode,showVertexColorsHotkey.control,showVertexColorsHotkey.shift,showVertexColorsHotkey.alt);
			serializedScript.copyVertexColorsHotkey = new SerializedWindowProperties.Hotkey(copyVertexColorsHotkey.keyCode,copyVertexColorsHotkey.control,copyVertexColorsHotkey.shift,copyVertexColorsHotkey.alt);
			serializedScript.pasteVertexColorsHotkey = new SerializedWindowProperties.Hotkey(pasteVertexColorsHotkey.keyCode,pasteVertexColorsHotkey.control,pasteVertexColorsHotkey.shift,pasteVertexColorsHotkey.alt);

			//Hotkeys States
			serializedScript.paintHotkeyState = paintHotkeyState;
			serializedScript.eraseHotkeyState = eraseHotkeyState;
			serializedScript.increaseSizeHotkeyState = increaseSizeHotkeyState;
			serializedScript.decreaseSizeHotkeyState = decreaseSizeHotkeyState;			
			serializedScript.showVertexColorsHotkeyState = showVertexColorsHotkeyState;
			serializedScript.copyVertexColorsHotkeyState = copyVertexColorsHotkeyState;
			serializedScript.pasteVertexColorsHotkeyState = pasteVertexColorsHotkeyState;

			//AssetDatabase.SaveAssets();
			EditorUtility.SetDirty(serializedScript);
	}	
	void LoadVariables(){
		//Object properties
		instanceMode = serializedScript.instanceMode;
		assetMode = serializedScript.assetMode;
		
		//Color properties
		paintMode = serializedScript.paintMode;
		eraseMode = serializedScript.eraseMode;
		brushMode = serializedScript.brushMode;
		bucketMode = serializedScript.bucketMode;
		paintColor = serializedScript.paintColor;
		eraseColor = serializedScript.eraseColor;
		
		//Brush properties
		brushSize = serializedScript.brushSize;
		brushStrenght = serializedScript.brushStrenght;
		brushFallOff = serializedScript.brushFallOff;
		angleLimit = serializedScript.angleLimit;
		
		//Bucket Properties
		bucketSize = serializedScript.bucketSize;
		
		//Paint properties
		perObjects = serializedScript.perObjects;
		allObjects = serializedScript.allObjects;
		rChannel = serializedScript.rChannel;
		gChannel = serializedScript.gChannel;
		bChannel = serializedScript.bChannel;
		aChannel = serializedScript.aChannel;
		
		//Gizmo properties
		handleColor = serializedScript.handleColor;
		outlineHandleColor = serializedScript.outlineHandleColor;
		solidHandle = serializedScript.solidHandle;
		drawHandleOutline = serializedScript.drawHandleOutline;
		drawHandleAngle = serializedScript.drawHandleAngle;
		
		//Foldouts
		objectPropertiesFoldout = serializedScript.objectPropertiesFoldout;
		colorPropertiesFoldout = serializedScript.colorPropertiesFoldout;
		toolPropertiesFoldout = serializedScript.toolPropertiesFoldout;
		paintPropertiesFoldout = serializedScript.paintPropertiesFoldout;
		gizmoPropertiesFoldout = serializedScript.gizmoPropertiesFoldout;
		hotkeysPropertiesFoldout = serializedScript.hotkeysPropertiesFoldout;
		uninstallFoldout = serializedScript.uninstallerFoldout;
		questionsFoldout = serializedScript.questionsFoldout;
		suggestionsFoldout = serializedScript.suggestionsFoldout;
		bugsFoldout = serializedScript.bugsFoldout;
		
		//Hotkeys
		//Paint Hotkey
		paintHotkey.keyCode = serializedScript.paintHotkey.keycode;
		paintHotkey.control = serializedScript.paintHotkey.control;
		paintHotkey.shift = serializedScript.paintHotkey.shift;
		paintHotkey.alt = serializedScript.paintHotkey.alt;
		
		//Erase Hotkey
		eraseHotkey.keyCode = serializedScript.eraseHotkey.keycode;
		eraseHotkey.control = serializedScript.eraseHotkey.control;
		eraseHotkey.shift = serializedScript.eraseHotkey.shift;
		eraseHotkey.alt = serializedScript.eraseHotkey.alt;

		//Increase Size Hotkey
		increaseSizeHotkey.keyCode = serializedScript.increaseSizeHotkey.keycode;
		increaseSizeHotkey.control = serializedScript.increaseSizeHotkey.control;
		increaseSizeHotkey.shift = serializedScript.increaseSizeHotkey.shift;
		increaseSizeHotkey.alt = serializedScript.increaseSizeHotkey.alt;

		//Decrease Size Hotkey
		decreaseSizeHotkey.keyCode = serializedScript.decreaseSizeHotkey.keycode;
		decreaseSizeHotkey.control = serializedScript.decreaseSizeHotkey.control;
		decreaseSizeHotkey.shift = serializedScript.decreaseSizeHotkey.shift;
		decreaseSizeHotkey.alt = serializedScript.decreaseSizeHotkey.alt;

		//Show Vertex Colors Hotkey
		showVertexColorsHotkey.keyCode = serializedScript.showVertexColorsHotkey.keycode;
		showVertexColorsHotkey.control = serializedScript.showVertexColorsHotkey.control;
		showVertexColorsHotkey.shift = serializedScript.showVertexColorsHotkey.shift;
		showVertexColorsHotkey.alt = serializedScript.showVertexColorsHotkey.alt;		

		//Copy Vertex Colors Hotkey
		copyVertexColorsHotkey.keyCode = serializedScript.copyVertexColorsHotkey.keycode;
		copyVertexColorsHotkey.control = serializedScript.copyVertexColorsHotkey.control;
		copyVertexColorsHotkey.shift = serializedScript.copyVertexColorsHotkey.shift;
		copyVertexColorsHotkey.alt = serializedScript.copyVertexColorsHotkey.alt;

		//Paste Vertex Colors Hotkey
		pasteVertexColorsHotkey.keyCode = serializedScript.pasteVertexColorsHotkey.keycode;
		pasteVertexColorsHotkey.control = serializedScript.pasteVertexColorsHotkey.control;
		pasteVertexColorsHotkey.shift = serializedScript.pasteVertexColorsHotkey.shift;
		pasteVertexColorsHotkey.alt = serializedScript.pasteVertexColorsHotkey.alt;
	
		//Hotkeys States
		paintHotkeyState = serializedScript.paintHotkeyState;
		eraseHotkeyState = serializedScript.eraseHotkeyState;
		increaseSizeHotkeyState = serializedScript.increaseSizeHotkeyState;
		decreaseSizeHotkeyState = serializedScript.decreaseSizeHotkeyState;			
		showVertexColorsHotkeyState = serializedScript.showVertexColorsHotkeyState;
		copyVertexColorsHotkeyState = serializedScript.copyVertexColorsHotkeyState;
		pasteVertexColorsHotkeyState = serializedScript.pasteVertexColorsHotkeyState;
	}	
}
}