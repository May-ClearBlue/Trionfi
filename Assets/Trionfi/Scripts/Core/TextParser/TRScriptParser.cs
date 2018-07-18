﻿using UnityEngine;
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Trionfi;

namespace Trionfi
{
    //スクリプトファイルを読み込んで、適切な形にパースして返します

    public class TRScriptParser : SingletonMonoBehaviour<TRScriptParser>
    {
        private string errorMessage = "";
        private string warningMessage = "";
        public bool onRegistMacro = false;

        private System.Globalization.TextInfo tf = new System.Globalization.CultureInfo("ja-JP"/*"en-US"*/, false).TextInfo;
        private const string nameSpace = "Trionfi";

        [SerializeField]
        public bool ignoreCR = true;

        [SerializeField]
        public string actorMarker = "【】";

        [SerializeField]
        public string actorTag = "talk_name";

        struct LineObject
        {
            public int line_num;
            public string line;
            public LineObject(int _line_num, string _line)
            {
                line_num = _line_num;
                line = _line;
            }
        }

        //予約語
        private List<string> RevervedWords = new List<string>
        {
            "#SetignoreCR",
            "#ResetignoreCR",
            "#include",
            "#define",
        };

        public bool ParsePreproseccor(string lineText)
        {
            switch (lineText) {
                case "#SetignoreCR":
                    ignoreCR = true;
                    break;
                case "#ResetignoreCR":
                    ignoreCR = false;
                    break;
                default:
                    return false;
            }
            return true;
        }

        enum ParseState
        {
            Begin,


        }

        public int lineCount = 0;

        int currentPos = 0;
        int startPos = 0;
        int endPos = 0;

        public void SkipSpace(ref char[] array)
        {
            while (array[currentPos] == '\r' || array[currentPos] == '\n' || array[currentPos] == ' ' || array[currentPos] == '\t')
            {
                if (array[currentPos] == '\r' || array[currentPos] == '\n')
                    lineCount++;

                currentPos++;
            }
        }

        public string SkipCharacter(ref char[] array, char terminator)
        {
            string buffer = "";

            while (array[currentPos] !=  terminator)
            {
                if (array[currentPos] == '\r' || array[currentPos] == '\n')
                    lineCount++;
                
                buffer += array[currentPos++];
            }
        }

        public string ReadLine(ref char[] array)
        {
            string buffer = "";

            startPos = currentPos;

            while (array[currentPos] != '\r' && array[currentPos] != '\n')
                buffer += array[currentPos++];

            endPos = currentPos;

            return buffer;
        }

        private bool IsSpace(char character)
        {
            return character == '\r' || character == '\n' || character == ' ' || character == '\t';
        }

        private bool IsPartOfNumeric(char character)
        {
            return character >= '0' && character <= '9';
        }

        private bool IsAlphabet(char character)
        {
            return (character >= 'a' && character <= 'z') || (character >= 'A' && character <= 'Z');
        }

        private bool IsPartOfVariable(char character)
        {
            return IsAlphabet(character) || IsPartOfNumeric(character) || character == '_';
        }

        private string GetFirstToken(ref char[] array)
        {
            string buffer = "";

            if (IsAlphabet(array[currentPos]))
                return null;

            while(IsPartOfVariable(array[currentPos]))
                buffer += array[currentPos++];

            if (IsSpace(array[currentPos]))
                endPos = currentPos;
            else
                return null;

            return buffer;
        }

        public List<AbstractComponent> _Parse(string script_text)
        {

            char[] characters = script_text.ToCharArray();

            ParseState _state = ParseState.Begin;

            SkipSpace(ref chara);

            if (characters[currentPos] == '#')
            {
                ReadLine(ref characters);
            }
            else if (characters[currentPos] == '@')
            {
                ReadLine(ref characters);
            }
            else if ((characters[currentPos] == '/' && characters[currentPos + 1] == '/') || characters[currentPos] == ';')
            {
                ReadLine(ref characters);
            }
            else if (characters[currentPos] == '/' && characters[currentPos + 1] == '*')
            {
            }
            else if (characters[currentPos] == '*')
            {
                ReadLine(ref characters);
            }

            else if (characters[currentPos] == '[')
            {
                string _tag = GetFirstToken(ref characters);
                if(_tag != null)
                    return;
                else
                    SkipCharacter(ref characters, ']');
            }
            else
            {
                ReadLine(ref characters);
            }
        }

        //パースしたタグクラスのListを返す
        public List<AbstractComponent> Parse(string script_text)
        {
            List<AbstractComponent> components = new List<AbstractComponent>();
            string[] lines = script_text.Split('\n');

            List<LineObject> line_objects = new List<LineObject>();

            bool isCommentNow = false;

            //lines の前に、一文字ずつ解析してすべての要素を分解する必要がある
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                //EX:
                if (line == "\r" || line == "\n" || line == "\r\n" || line == "") {
                    line_objects.Add(new LineObject(i + 1, "\r"));
                    continue;
                }

                line = line.Replace("\r", "").Replace("\n", "");

                //Debug.Log (line);

                if (line == "")
                    continue;

                string firstChar = line[0].ToString();

                //コメント開始
                if (line.IndexOf("/*") != -1)
                    isCommentNow = true;

                if (line.IndexOf("*/") != -1) {
                    isCommentNow = false;
                    continue;
                }

                if (isCommentNow == true)
                    continue;

                // ;で始まってたらコメントなので無視する
                if (firstChar == ";")
                    continue;

                line = line.Replace("|", "\r\n");

                //ラベルを表します
                if (line.IndexOf("*/") == -1 && firstChar == "*")
                    line = "[label name='" + line.Replace("*", "").Trim() + "' ]";

                //１行の命令なので、残りの文字列をまとめて、タグ作成に回す
                //１行のタグ命令にして渡す
                if (firstChar == "@")
                    line = "[" + line.Replace("@", "") + "]";

                if (firstChar == "#") {
                    line_objects.Add(new LineObject(i + 1, line));
                    continue;
                }

                bool flag_now_tag = false;
                StringBuilder tag_line = new StringBuilder();

                for (int k = 0; k < line.Length; k++) {
                    string c = line[k].ToString();

                    if (c == "[" && flag_now_tag == true) {
                        line_objects.Add(new LineObject(i + 1, tag_line.ToString()));
                        flag_now_tag = false;
                        tag_line = new StringBuilder();
                    }

                    tag_line.Append(c);

                    //最後の一文字の場合
                    if (k == line.Length - 1) {
                        line_objects.Add(new LineObject(i + 1, tag_line.ToString()));
                        continue;
                    }

                    flag_now_tag = true;

                    if (c == "]") {
                        flag_now_tag = false;
                        line_objects.Add(new LineObject(i + 1, tag_line.ToString()));
                        tag_line = new StringBuilder();
                    }
                }
            }


            bool isText = false;

            foreach (LineObject lo in line_objects) {
                string line = lo.line;
                int line_num = lo.line_num;

                string firstChar = line[0].ToString();

                //プリプロセッサ的なアレ。
                //ToDo:名前タグを【】にする
                if (!string.IsNullOrEmpty(actorMarker)) {
                    if (firstChar == actorMarker.Substring(0, 1)) {
                        if (actorMarker.Length <= 1) {
                            line = "[" + actorTag/*  talk_name */ + " val='" + line.Replace(firstChar, "") + "' ]";
                            AbstractComponent cmp = MakeTag(line, line_num);
                            components.Add(cmp);
                        }
                        else if (line[line.Length - 1] == actorMarker[1]) {
                            line = line.Replace(actorMarker[1].ToString(), "");
                            line = "[" + actorTag/*  talk_name */ + " val='" + line.Replace(firstChar, "") + "' ]";
                            AbstractComponent cmp = MakeTag(line, line_num);
                            components.Add(cmp);
                        }
                        else { /*error?*/}
                        continue;
                    }
                }

                if (firstChar == "#") {
                    if (RevervedWords.Contains(line)) {
                        ParsePreproseccor(line);
                    }
                    else {
                        line = "[" + actorTag/* talk_name */ + " val='" + line.Replace("#", "") + "' ]";
                        AbstractComponent cmp = MakeTag(line, line_num);
                        components.Add(cmp);
                    }
                    continue;
                }

                if (line == "\r") {
                    //ToDo:直前のRを消す
                    if (isText == true && ignoreCR)
                        components.Add(new PComponent());

                    isText = false;

                    continue;
                }

                //テキストの場合
                if (firstChar != "[" && firstChar != "@") {
                    line = "[message val=\"" + line + "\"]";
                    firstChar = "[";
                    isText = true;
                }
                else
                    isText = false;

                if (firstChar == "[" || firstChar == "@") {

                    AbstractComponent cmp = MakeTag(line, line_num);

                    //リストに追加
                    components.Add(cmp);
                }

                //ToDo:
                //				if(isText == true && ignoreCR)
                //					components.Add(new RComponent());

            }

            Dump(line_objects, Application.dataPath + "log.txt");

            return components;
        }

        //１行のstringからタグを作成
        public AbstractComponent MakeTag(string line)
        {
            AbstractComponent _component = MakeTag(line, 0);
            return _component;
        }

        //タグ名と引数の辞書からタグを生成
        public AbstractComponent MakeTag(string tag_name, TRVariable param)
        {
            string line = "[" + tag_name + " ";
            string param_str = "";
            foreach (KeyValuePair<string, string> pair in param)
            {
                param_str += pair.Key + "=" + pair.Value + " ";
            }

            line = line + param_str + "]";

//            Debug.Log(line);

            AbstractComponent cmp = MakeTag(line, 0);

            return cmp;
        }

        public AbstractComponent MakeTag(string statement, int lineNum) {
            TagParam tag = new TagParam(statement, lineNum);

            string className = nameSpace + "." + tf.ToTitleCase(tag.tagName) + "Component";

            // リフレクションで動的型付け
            Type masterType = Type.GetType(className);
            AbstractComponent cmp = (AbstractComponent)Activator.CreateInstance(masterType);
#if false
            // 実行中のアセンブリを取得
            Assembly assembly = Assembly.GetExecutingAssembly();

            // インスタンスを生成
\            AbstractComponent cmp = (AbstractComponent)assembly.CreateInstance(
　　              className,    // 名前空間を含めたクラス名
                  false,        // 大文字小文字を無視するかどうか
                  BindingFlags.CreateInstance,      // インスタンスを生成
                  null,         // 通常はnullでOK
                  new object[] { tag },    // コンストラクタの引数
                  null,         // カルチャ設定（通常はnullでOK）
                  null          // ローカル実行の場合はnullでOK
                );
#endif

            if (cmp != null)
                cmp.Init(tag);
            else
            {
#if UNITY_EDITOR
                ErrorLogger.Log("Invalid Tag:\"" + tag.tagName + "\"");
#else
        	//マクロとして登録
            ErrorLogger.Log("MacroStart:"+tag.tagName);
		    cmp = new _MacrostartComponent();
#endif
            }

            return cmp;
        }


        [Conditional("UNITY_EDITOR"), Conditional("TRIONFI_DEBUG"), Conditional("DEVELOPMENT_BUILD")]
        void Dump(List<LineObject> list, string path) 
        {
            System.IO.StreamWriter stream = new System.IO.StreamWriter(path, false,Encoding.GetEncoding("utf-8"));
            foreach (LineObject line in list)
            {
                stream.WriteLine(line.line_num.ToString()+":"+line.line);
            }
            stream.Close();
        } 
    }
}
