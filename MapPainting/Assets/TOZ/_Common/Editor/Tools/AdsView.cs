using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;

namespace TOZEditor {

	public class AdsView : EditorWindow {
		//Variables
		private static AdsView window;

		public static void CreateWindow() {
			//Init window
			Rect rect = EditorGUIUtility.GetMainWindowPosition();
			int w = 500;
			int h = 252;
			int x = (int)(rect.width / 2) - 250;
			int y = (int)(rect.height / 2) - 126;
			window = (AdsView)GetWindow(typeof(AdsView), true);
			window.position = new Rect(x, y, w, h);
			window.minSize = new Vector2(w, h);
			window.maxSize = new Vector2(w, h);
			window.titleContent = new GUIContent("PLEASE Check our other ASSETS");
			window.Show();
		}

		private Vector2 scrollView = Vector2.zero;
		private string[] titles;
		private string[] descriptions;
		private string[] links;
		private string[] types;
		private const int _lineCount = 4;

		//Mono Methods
		private void OnEnable() {
			GetAssetsList("https://tozlab.com/assetlist.txt");
		}

		private void OnGUI() {
			if(titles == null) {
				this.Close();
				return;
			}

			//First Column start
			EditorGUILayout.BeginVertical(GUILayout.Width(496));
			scrollView = EditorGUILayout.BeginScrollView(scrollView);
			for(int i = 0; i < titles.Length; i++) {
				Line(titles[i], descriptions[i], links[i], types[i]);
			}
			EditorGUILayout.EndScrollView();
			//First Column end
			EditorGUILayout.EndVertical();
		}

		private void Line(string title, string txt, string link, string linkType) {
			EditorGUILayout.BeginHorizontal("box", GUILayout.Height(54));

			EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
			EditorGUILayout.LabelField(title, EditorStyles.whiteLargeLabel);
			EditorGUILayout.LabelField(txt, EditorStyles.wordWrappedLabel);
			EditorGUILayout.EndVertical();

			if(GUILayout.Button(linkType, GUILayout.Width(50), GUILayout.ExpandHeight(true))) {
				Application.OpenURL(link);
			}
			EditorGUILayout.EndHorizontal();
		}

		private void GetAssetsList(string url) {
			UnityWebRequest uwr = UnityWebRequest.Get(url);
			uwr.SendWebRequest();
			while(!uwr.isDone) {
				EditorUtility.DisplayProgressBar("Retrieving..", "Please Wait", uwr.downloadProgress);
			}
			EditorUtility.ClearProgressBar();

			if(uwr.result != UnityWebRequest.Result.Success) {
				Debug.Log("Error: " + uwr.error);
			}
			else {
				ParseString(uwr.downloadHandler.text);
			}
		}

		private void ParseString(string txt) {
			if(txt != string.Empty) {
				string[] assets = txt.Split('\n');
				int count = assets.Length / _lineCount;

				titles = new string[count];
				descriptions = new string[count];
				links = new string[count];
				types = new string[count];

				for(int i = 0; i < count; i++) {
					titles[i] = assets[(i*_lineCount)];
					descriptions[i] = assets[(i*_lineCount)+1];
					links[i] = assets[(i*_lineCount)+2];
					types[i] = assets[(i*_lineCount)+3];
				}
			}
		}
	}

}