﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Trionfi
{
    //[story val="メッセージ"]
    public class MessageComponent : AbstractComponent
    {
        public MessageComponent()
        {
            //必須項目
            essentialParams = new List<string> {
                "val"
            };
        }

        protected override void TagFunction()
        {
            string message = tagParam.Identifier("val");
            TRUIInstance.Instance.messageWindow.ShowMessage(message, TRGameConfig.Instance.configData.textspeed);
        }
    }

    //[name val="なまえ" face="表情"]
    public class NameComponent : AbstractComponent
    {
        public NameComponent()
        {
            //必須項目
            essentialParams = new List<string> {
                "val"
            };
        }

        protected override void TagFunction()
        {
            string name = tagParam.Identifier("val");
            TRUIInstance.Instance.messageWindow.ShowName(name);
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

        protected override IEnumerator TagSyncFunction()
        {
            yield return TRUIInstance.Instance.messageWindow.Wait();
        }
    }

    //メッセージクリア
    public class CmComponent : AbstractComponent
    {
        protected override void TagFunction()
        {
            TRUIInstance.Instance.messageWindow.ClearMessage();
        }
    }

    //フォント設定    
    //[font size=26 color=#FFFFFF80]
	public class FontComponent : AbstractComponent {
		public FontComponent() {
			//必須項目
			essentialParams = new List<string> { };
//			originalParamDic = new ParamDictionary() {
//				{"size",""},
//				{"color",""},
//			};
		}

		protected override void TagFunction() {
            int size = tagParam.Int("size");
            uint colorValue = tagParam.Uint("color", 0xFFFFFFFF);

            Color color = TRVariable.ToRGB(colorValue);
            TRUIInstance.Instance.messageWindow.currentMessage.fontSize = size;
            TRUIInstance.Instance.messageWindow.currentMessage.color = color;
        }
	}
}
