using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace TOZEditor {

	public class SplatPainter : EditorWindow {
#region Editor Window
		//Variables
		private static SplatPainter window;

		[MenuItem("Window/TOZ/Tools/Splat Painter")]
		private static void CreateWindow() {
			//Init window
			int x = 40, y = 40, w = 330, h = 246;
			window = (SplatPainter)GetWindow(typeof(SplatPainter), true);
			window.position = new Rect(x, y, w, h);
			window.minSize = new Vector2(w, h);
			window.maxSize = new Vector2(w, h);
			window.titleContent = new GUIContent("Splat Painter");
			window.Show();
		}
#endregion Editor Window

#region Variables
		//Variables
		private static Color _r = new Color(1f, 0f, 0f, 0f);
		private static Color _g = new Color(0f, 1f, 0f, 0f);
		private static Color _b = new Color(0f, 0f, 1f, 0f);
		private static Color _a = new Color(0f, 0f, 0f, 1f);
		private const string editorResourcesPath = "Assets/TOZ/_Common/Editor/Resources";

		private GameObject go;
		private Collider coll;
		private MeshFilter mf;
		private MeshRenderer mr;
		private Material originalMaterial, debugMaterial;
		private Texture2D texture;
		private int textureSize;
		//private Color[] brushColors;

		//GUI Variables
		private bool canPaint;
		private string gui_Notification;
		private bool tgl_Paint;
		private string str_Paint;
		private bool tgl_ShowTexture;
		private string str_ShowTexture;
		private float gui_BrushOpacity;
		private Color gui_BrushColor;
		private int gui_Brush;
		private bool gui_BrushClamped;
		private Texture2D[] originalBrushes, scaledBrushes;
#endregion Variables

		//Mono Methods
		private void OnEnable() {
			SceneView.duringSceneGui += OnSceneGUI;

			//Create debug material
			if(debugMaterial == null) {
				debugMaterial = new Material(Shader.Find("TOZ/Debug/SplatColors"));
			}

			//Load brushes and reset
			LoadOriginalBrushes();
			CreateScaledBrushes(256, 256);
			Initialize();
		}

		private void OnDisable() {
			SceneView.duringSceneGui -= OnSceneGUI;

			//Cleanup
			DestroyScaledBrushes();
			UnloadOriginalBrushes();
			ResetMe();
			DestroyImmediate(debugMaterial);

			//Show ads
			AdsView.CreateWindow();
		}

		private void OnSelectionChange() {
			Initialize();
			this.Repaint();
		}

		private void OnProjectChange() {
			Initialize();
			this.Repaint();
		}

		private void OnInspectorUpdate() {
			this.Repaint();
		}

		private void OnGUI() {
			EditorGUILayout.BeginVertical();

			//Warnings
			if(!canPaint) {
				EditorGUILayout.HelpBox(gui_Notification, MessageType.Warning);
				return;
			}

			EditorGUILayout.BeginHorizontal("box");
			if(GUILayout.Button(str_Paint, GUILayout.Width(144))) {
				tgl_Paint = !tgl_Paint;
				if(tgl_Paint) {
					texture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, true);
					FillTexture(texture, _r);
					debugMaterial.SetTexture("_MaskTex", texture);
					//Set temporary mask texture on
					if(originalMaterial.HasProperty("_MaskTex")) {
						originalMaterial.SetTexture("_MaskTex", texture);
					}
					str_Paint = "STOP PAINTING";
					//Debug Material
					mr.sharedMaterial = debugMaterial;
					//Other button
					tgl_ShowTexture = true;
					str_ShowTexture = "HIDE TEXTURE";
				}
				else {
					str_Paint = "START PAINTING";
					debugMaterial.SetTexture("_MaskTex", null);
					//Set temporary mask texture off
					if(originalMaterial.HasProperty("_MaskTex")) {
						originalMaterial.SetTexture("_MaskTex", null);
					}
					Initialize();
				}
			}

			if(GUILayout.Button(str_ShowTexture, GUILayout.Width(144))) {
				tgl_ShowTexture = !tgl_ShowTexture;
				if(tgl_ShowTexture) {
					str_ShowTexture = "HIDE TEXTURE";
					//Debug Material
					mr.sharedMaterial = debugMaterial;
				}
				else {
					str_ShowTexture = "SHOW TEXTURE";
					mr.sharedMaterial = originalMaterial;
				}
			}

			if(GUILayout.Button("?", GUILayout.Width(22))) {
				Application.OpenURL("https://tozlab.com/documentation/toz-splat-painter");
			}
			EditorGUILayout.EndHorizontal();

			if(!tgl_Paint) {
				textureSize = EditorGUILayout.IntField("Texture Size :", textureSize);
				EditorGUILayout.HelpBox("Ideal texture size should be power of 2!", MessageType.Info);
			}

			if(tgl_Paint) {
				//Top
				EditorGUILayout.Space();
				EditorGUILayout.BeginVertical("box");

				//EditorGUI.BeginChangeCheck();
				gui_Brush = GUILayout.SelectionGrid(gui_Brush, scaledBrushes, scaledBrushes.Length, GUILayout.Height(32));
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Brush Size :");
				if(GUILayout.Button("16", GUILayout.Width(30))) {
					CreateScaledBrushes(16, 16);
				}
				if(GUILayout.Button("32", GUILayout.Width(30))) {
					CreateScaledBrushes(32, 32);
				}
				if(GUILayout.Button("64", GUILayout.Width(30))) {
					CreateScaledBrushes(64, 64);
				}
				if(GUILayout.Button("128", GUILayout.Width(30))) {
					CreateScaledBrushes(128, 128);
				}
				if(GUILayout.Button("256", GUILayout.Width(30))) {
					CreateScaledBrushes(256, 256);
				}
				EditorGUILayout.EndHorizontal();
				//if(EditorGUI.EndChangeCheck()) {
					//brushColors = scaledBrushes[gui_Brush].GetPixels();
				//}

				gui_BrushOpacity = EditorGUILayout.Slider("Brush Opacity :", gui_BrushOpacity, 0.0f, 1.0f);
				gui_BrushClamped = EditorGUILayout.Toggle("Brush Clamp :", gui_BrushClamped);
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Brush Color :");
				if(GUILayout.Button("R", GUILayout.Width(22))) {
					gui_BrushColor = _r;
				}
				if(GUILayout.Button("G", GUILayout.Width(22))) {
					gui_BrushColor = _g;
				}
				if(GUILayout.Button("B", GUILayout.Width(22))) {
					gui_BrushColor = _b;
				}
				if(GUILayout.Button("A", GUILayout.Width(22))) {
					gui_BrushColor = _a;
				}
				gui_BrushColor = EditorGUILayout.ColorField(gui_BrushColor, GUILayout.Height(20));
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.EndVertical();

				//Center
				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal("box");
				EditorGUILayout.PrefixLabel("Fill Channel :");
				if(GUILayout.Button("R", GUILayout.Width(22))) {
					FillTexture(texture, _r);
				}
				if(GUILayout.Button("G", GUILayout.Width(22))) {
					FillTexture(texture, _g);
				}
				if(GUILayout.Button("B", GUILayout.Width(22))) {
					FillTexture(texture, _b);
				}
				if(GUILayout.Button("A", GUILayout.Width(22))) {
					FillTexture(texture, _a);
				}
				EditorGUILayout.EndHorizontal();

				//Bottom
				EditorGUILayout.Space();
				if(GUILayout.Button("SAVE PAINTED TEXTURE")) {
					//Create new texture and save to disk
					string file = EditorUtility.SaveFilePanelInProject("Save Texture", "New Texture", "png", "Please enter a file name");
					//string file = AssetDatabase.GenerateUniqueAssetPath("Assets/New Texture.png");
					if(file.Length != 0) {
						byte[] bytes = texture.EncodeToPNG();
						File.WriteAllBytes(file, bytes);
						AssetDatabase.Refresh();
						Debug.LogWarning("TEXTURE is saved at location:" + file);
						//Revert originals
						debugMaterial.SetTexture("_MaskTex", null);
						ResetMe();
					}
				}
			}

			EditorGUILayout.EndVertical();
		}

		//Methods
		private void OnSceneGUI(SceneView sceneView) {
			if(!tgl_Paint) {
				return;
			}

			Event current = Event.current;
			Ray ray = HandleUtility.GUIPointToWorldRay(current.mousePosition);
			RaycastHit hit;
			//Events
			int controlID = GUIUtility.GetControlID(sceneView.GetHashCode(), FocusType.Passive);
			switch(current.GetTypeForControl(controlID)) {
				case EventType.Layout: {
					if(!tgl_Paint) {
						return;
					}
					HandleUtility.AddDefaultControl(controlID);
				}
				break;
				case EventType.MouseDown:
				case EventType.MouseDrag: {
					if(!tgl_Paint) {
						return;
					}
					if(current.alt || current.control) {
						return;
					}
					if(HandleUtility.nearestControl != controlID) {
						return;
					}
					if(current.GetTypeForControl(controlID) == EventType.MouseDrag && GUIUtility.hotControl != controlID) {
						return;
					}
					if(current.button != 0) {
						return;
					}

					if(current.type == EventType.MouseDown) {
						GUIUtility.hotControl = controlID;
					}
					//Do painting
					if(Physics.Raycast(ray, out hit, float.MaxValue)) {
						if(hit.transform == go.transform) {
							Vector2 uv = hit.textureCoord;
							uv.x *= texture.width;
							uv.y *= texture.height;
							int width2 = scaledBrushes[gui_Brush].width / 2;
							int height2 = scaledBrushes[gui_Brush].height / 2;
/*
							int x = (int)uv.x - width2;
							int y = (int)uv.y - height2;
							Color[] oldCol = texture.GetPixels(x, y, scaledBrushes[gui_Brush].width, scaledBrushes[gui_Brush].height);
							Color[] newCol = new Color[brushColors.Length];
							for(int i = 0; i < brushColors.Length; i++) {
								newCol[i] = Color.Lerp(oldCol[i], brushColors[i] * gui_BrushColor, brushColors[i].a * gui_BrushOpacity);
							}
							texture.SetPixels(x, y, scaledBrushes[gui_Brush].width, scaledBrushes[gui_Brush].height, newCol);
*/
							for(int y = 0; y < scaledBrushes[gui_Brush].height; y++) {
								for(int x = 0; x < scaledBrushes[gui_Brush].width; x++) {
									int xpix = (int)uv.x - width2 + x;
									int ypix = (int)uv.y - height2 + y;
									if(gui_BrushClamped) {
										if(xpix < 0 || xpix >= texture.width || ypix < 0 || ypix >= texture.height)
											continue;
									}
									Color oldCol = texture.GetPixel(xpix, ypix);
									Color brushCol = scaledBrushes[gui_Brush].GetPixel(x, y);
									Color newCol = Color.Lerp(oldCol, gui_BrushColor, brushCol.a * gui_BrushOpacity);
									texture.SetPixel(xpix, ypix, newCol);
								}
							}
							texture.Apply();
						}
					}
					current.Use();
				}
				break;
				case EventType.MouseUp: {
					if(!tgl_Paint) {
						return;
					}
					if(GUIUtility.hotControl != controlID) {
						return;
					}
					GUIUtility.hotControl = 0;
					current.Use();
				}
				break;
				case EventType.Repaint: {
					//Draw paint brush
					if(Physics.Raycast(ray, out hit, float.MaxValue)) {
						if(hit.transform == go.transform) {
							Handles.color = new Color(gui_BrushColor.r, gui_BrushColor.g, gui_BrushColor.b, 1.0f);
							float scale = ((scaledBrushes[gui_Brush].width / (float)textureSize) / 2f) * coll.bounds.size.x;
							Handles.DrawWireDisc(hit.point, hit.normal, scale, 2f);
						}
					}
					HandleUtility.Repaint();
				}
				break;
			}
		}

		private void ResetMe() {
			//Reset previously worked on object (if any)
			if(go && originalMaterial) {
				mr.sharedMaterial = originalMaterial;
			}

			if(texture) {
				DestroyImmediate(texture);
			}

			//Reset variables
			go = null;
			coll = null;
			mf = null;
			mr = null;
			originalMaterial = null;
			texture = null;
			textureSize = 512;

			//Reset gui variables
			canPaint = false;
			gui_Notification = string.Empty;
			tgl_Paint = false;
			str_Paint = "START PAINTING";
			tgl_ShowTexture = false;
			str_ShowTexture = "SHOW TEXTURE";
			gui_Brush = 0;
			gui_BrushOpacity = 0.5f;
			gui_BrushColor = _g;
			gui_BrushClamped = true;
		}

		private void Initialize() {
			ResetMe();

			//Reset selected object
			go = Selection.activeGameObject;
			if(go != null) {
				coll = go.GetComponent<Collider>();
				if(coll != null && coll is MeshCollider) {
					mf = go.GetComponent<MeshFilter>();
					if(mf != null) {
						mr = go.GetComponent<MeshRenderer>();
						if(mr != null) {
							originalMaterial = mr.sharedMaterial;

							//All is okay, we can paint now
							canPaint = true;
						}
						else
							gui_Notification = "Object doesnt have a renderer!";
					}
					else
						gui_Notification = "Object doesnt have a MeshFilter!";
				}
				else
					gui_Notification = "Object doesnt have a MeshCollider!";
			}
			else
				gui_Notification = "No object selected!";
		}

		public static void FillTexture(Texture2D tex, Color color) {
			int size = tex.width * tex.height;
			Color[] col = new Color[size];
			for(int i = 0; i < size; i++) {
				col[i] = color;
			}
			tex.SetPixels(col);
			tex.Apply();
		}

		private void LoadOriginalBrushes() {
			UnloadOriginalBrushes();

			originalBrushes = new Texture2D[6];
			for(int i = 0; i < originalBrushes.Length; i++) {
				originalBrushes[i] = (Texture2D)AssetDatabase.LoadAssetAtPath(editorResourcesPath + "/Brushes/"+ i +".png", typeof(Texture2D));
			}
		}

		private void UnloadOriginalBrushes() {
			if(originalBrushes != null) {
				if(originalBrushes.Length > 0) {
					for(int i = 0; i < originalBrushes.Length; i++) {
						Resources.UnloadAsset(originalBrushes[i]);
					}
				}
				originalBrushes = null;
			}
		}

		private void CreateScaledBrushes(int width, int height) {
			DestroyScaledBrushes();

			int size = width * height;

			scaledBrushes = new Texture2D[originalBrushes.Length];
			Color[] col = new Color[size];
			for(int i = 0; i < scaledBrushes.Length; i++) {
				scaledBrushes[i] = (Texture2D)Instantiate(originalBrushes[i]);
				if(scaledBrushes[i].width == width && scaledBrushes[i].height == height)
					continue;

				scaledBrushes[i].Reinitialize(width, height);
				float x = (1f / originalBrushes[i].width) * ((float)originalBrushes[i].width / width);
				float y = (1f / originalBrushes[i].height) * ((float)originalBrushes[i].height / height);
				for(int j = 0; j < size; j++) {
					col[j] = originalBrushes[i].GetPixelBilinear(x * ((float)j % width), y * ((float)j / height));
				}
				scaledBrushes[i].SetPixels(col);
				scaledBrushes[i].Apply();
			}
		}

		private void DestroyScaledBrushes() {
			if(scaledBrushes != null) {
				if(scaledBrushes.Length > 0) {
					for(int i = 0; i < scaledBrushes.Length; i++) {
						DestroyImmediate(scaledBrushes[i]);
					}
				}
				scaledBrushes = null;
			}
		}
	}

}