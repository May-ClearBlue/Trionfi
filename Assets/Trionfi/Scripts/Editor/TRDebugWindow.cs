﻿
#if UNITY_EDITOR
using UnityEditor;
using EditorCoroutines;
#endif

using UnityEngine;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Trionfi
{
#if UNITY_EDITOR
    public class TRDebugger : EditorWindow
    {
        string consoleLog;
        Vector2 logScroll;

        string scriptText;
        Vector2 scriptScroll;

        string statementText;

        [MenuItem("Tools/Trionfi/DebugWindow")]
        private static void Open()
        {
            var window = GetWindow<TRDebugger>("Trinofi DebugWindow");
        }

        [MenuItem("Tools/Trionfi/OpenScriptFile")]
        private static void ExecuteScriptFile()
        {
            
            string path = EditorUtility.OpenFilePanel("シナリオファイル", Application.dataPath, "txt");
            if (path.Length != 0)
            {
                string fileContent = File.ReadAllText(path);
                TRVirtualMachine.currentTagInstance.CompileScriptString(fileContent);
            }
        }

        [MenuItem("Tools/Trionfi/Reset PlayerPrefs")]
        public static void ResetPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        private void OnGUI()
        {
            GUILayout.Label("1行スクリプト（[]は不要）");

            statementText = EditorGUILayout.TextField("", statementText);
            if (GUILayout.Button("実行"))
            {
                TRTagParser tagParser = new TRTagParser(statementText);
                AbstractComponent tagComponent = tagParser.Parse();
                if (tagComponent != null)
                {
                    consoleLog += ("Tag: " + tagComponent.tagName + "\n");

                    tagComponent.Execute();
                    this.StartCoroutine(TRVirtualMachine.Instance.tagSyncFunction());
                }
                else
                    consoleLog += ("Invalid Tag!\n");
            }

            GUILayout.Space(20);
            GUILayout.Label("【デバッグログ】");

            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label("最大サイズ");
            //        GUILayout.TextField("ログ最大サイズ");
            if (GUILayout.Button("クリア"))
            {
                consoleLog = "";
            }

            GUILayout.EndHorizontal();

            logScroll = EditorGUILayout.BeginScrollView(logScroll);
            consoleLog = EditorGUILayout.TextArea(consoleLog, GUILayout.Height(200));
            EditorGUILayout.EndScrollView();

            GUILayout.Space(20);
            GUILayout.Label("【スクリプトテスト】");

            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label("内容");
            if (GUILayout.Button("実行"))
            {
                TRTagInstance _tagInstance = new TRTagInstance();

                if (_tagInstance.CompileScriptString(scriptText))
                    this.StartCoroutine(TRVirtualMachine.Instance.Run(_tagInstance));
                else
                    consoleLog += ("failed compile\n");
            }

            GUILayout.EndHorizontal();

            scriptScroll = EditorGUILayout.BeginScrollView(scriptScroll);
            scriptText = EditorGUILayout.TextArea(scriptText, GUILayout.Height(200));
            EditorGUILayout.EndScrollView();
        }
    }
#endif
}