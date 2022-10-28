using System;
using System.Collections.Generic;
using System.Text;

namespace _MqttClient.utils
{
    public static class MsgUtils
    {
        public static string parseMsgToNumber(string msg)
        {
            StringBuilder builder=new StringBuilder();
            if (msg.Length > 0)
            {
                char[] tmp = msg.ToCharArray();
                for (int i = 0; i < msg.Length; i++)
                {
                    switch (tmp[i])
                    {
                        case '一':
                            builder.Append("1");
                            break;
                        case '二':
                            builder.Append("2");
                            break;
                        case '两':
                            builder.Append("2");
                            break;
                        case '三':
                            builder.Append("3");
                            break;
                        case '四':
                            builder.Append("4");
                            break;
                        case '五':
                            builder.Append("5");
                            break;
                        case '六':
                            builder.Append("6");
                            break;
                        case '七':
                            builder.Append("7");
                            break;
                        case '八':
                            builder.Append("8");
                            break;
                        case '九':
                            builder.Append("9");
                            break;
                        case '零':
                            builder.Append("0");
                            break;
                        default:
                            builder.Append(tmp[i]);
                            break;
                    }
                }
            }
            return builder.ToString();
        }
    }
}
