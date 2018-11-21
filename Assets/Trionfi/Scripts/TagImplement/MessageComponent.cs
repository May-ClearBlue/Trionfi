﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace Trionfi
{
    //[story val="メッセージ"]
    public class MessageComponent : AbstractComponent
    {
        public MessageComponent()
        {
#if UNITY_EDITOR && TR_DEBUG
            //必須項目
            essentialParams = new List<string> {
                "val"
            };
#endif
        }

        protected override void TagFunction()
        {
            string message = tagParam["val"].Literal();
            Trionfi.Instance.messageWindow.ShowMessage(message, TRGameConfig.Instance.configData.textspeed);
        }

        public override IEnumerator TagSyncFunction()
        {
            yield return new WaitWhile(() => Trionfi.Instance.messageWindow.state != TRMessageWindow.MessageState.None);
        }
    }

    //[name val="なまえ" face="表情"]
    public class NameComponent : AbstractComponent
    {
        public NameComponent()
        {
#if UNITY_EDITOR && TR_DEBUG
            //必須項目
            essentialParams = new List<string> {
                "val"
            };
#endif
        }

        protected override void TagFunction()
        {
            string name = tagParam["val"].Literal();
            Trionfi.Instance.messageWindow.ShowName(name);
        }
    }

    public class MesspeedComponent : AbstractComponent
    {
        public MesspeedComponent()
        {
#if UNITY_EDITOR && TR_DEBUG
            //必須項目
            essentialParams = new List<string> {
                "ratio"
            };
#endif
        }

        protected override void TagFunction()
        {
            float ratio = tagParam["ratio", 1.0f];
            Trionfi.Instance.messageWindow.speedRatio = ratio;
        }
    }

    public class MesshakeComponent : AbstractComponent
    {
        public MesshakeComponent()
        {
#if UNITY_EDITOR && TR_DEBUG
            //必須項目
            essentialParams = new List<string>
            {
//                "layer",
            };
#endif
        }

        protected override void TagFunction()
        {
            //振幅
            int strength = tagParam["strength", 5];
            //振動頻度
            int vibratio = tagParam["vibrato", 20];

            RectTransform _rect = Trionfi.Instance.messageWindow.gameObject.GetComponent<RectTransform>();

            Trionfi.Instance.messageWindow.tweener = _rect.GetComponent<RectTransform>().DOShakePosition(1.0f, strength, vibratio, 90.0f, false, false).SetLoops(-1);
        }
    }
    
    /*
        //改行命令 [r]
        public class RComponent : AbstractComponent
        {
            public RComponent()
            {
                originalParamDic = new ParamDictionary() { };
            }

            protected override void TagFunction()
            {
                TRUIManager.Instance.currentMessageWindow.currentMessage.text += "\n";
                yield return null;
            }
        }

    public class LComponent : AbstractComponent
    {
        protected override void TagFunction()
        {
        }

        public override IEnumerator TagAsyncWait()
        {
            yield return TRUIInstance.Instance.messageWindow.Wait();
        }
    }
    */


    //クリック待ち。novelmodeの時はメッセージクリアをしない（のでcmタグを手動で入れなければならない）
    public class PComponent : AbstractComponent
    {
        protected override void TagFunction()
        {
        }

        public override IEnumerator TagSyncFunction()
        {
            yield return Trionfi.Instance.messageWindow.Wait();
        }
    }

    //メッセージクリア
    public class CmComponent : AbstractComponent
    {
        protected override void TagFunction()
        {
            Trionfi.Instance.messageWindow.ClearMessage();
        }
    }

    //フォント設定    
    //[font size=26 color=#FFFFFF80]
	public class FontComponent : AbstractComponent {
		public FontComponent() {
#if UNITY_EDITOR && TR_DEBUG
            //必須項目
            essentialParams = new List<string> { };
            //			originalParamDic = new ParamDictionary() {
            //				{"size",""},
            //				{"color",""},
            //			};
#endif
        }

		protected override void TagFunction() {
            int size = tagParam["size", TRSystemConfig.Instance.fontSize];
            uint colorValue = tagParam["color", 0xFFFFFFFF];

            Color color = TRVariableDictionary.ToRGB(colorValue);
            Trionfi.Instance.messageWindow.currentMessage.fontSize = size;
            Trionfi.Instance.messageWindow.currentMessage.color = color;
        }
	}
}
