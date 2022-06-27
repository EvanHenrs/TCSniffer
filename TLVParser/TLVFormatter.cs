﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCSniffer.utils;
using DotNetty.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCSniffer.TLVParser
{
    public class TLVFormatter : IParser
    {
       // private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();
        private static readonly Dictionary<short, IParser> Parsers = new Dictionary<short, IParser>();

        public static void RegTLVParsers()
        {

            Type[] types = typeof(attributes.TLVParser).GetExportedTypes().Where(o => IsTLVParserAttrbute(Attribute.GetCustomAttributes(o, false))).ToArray();
            foreach (Type type in types)
            {
                if (type.GetInterface(nameof(IParser)) != null)
                {
                    attributes.TLVParser t = (attributes.TLVParser)type.GetCustomAttributes(typeof(attributes.TLVParser), true).FirstOrDefault();
                    if (t != null)
                    {
                        if (Parsers.ContainsKey(t.Cmd))
                        {
                            string existClassFullName = string.Empty;
                            if (Parsers.TryGetValue(t.Cmd, out IParser value))
                            {
                                existClassFullName = value.GetType().FullName;
                            }
                            //Logger.Warn("parser[{0}] cmd[{1}]已注册, 请检查重复项[{2}]", existClassFullName, t.Cmd.HexPadLeft().HexDump(), type.FullName);
                        }
                        else
                        {
                            var clazz = (IParser)type.Assembly.CreateInstance(type.FullName);
                            Parsers.Add(t.Cmd, clazz);
                        }
                    }
                }
                else
                {
                    //Logger.Warn("【警告】标注了{0}属性的{1}类务必实现{2}接口, 否则无法正常解析数据",
                       // nameof(attributes.TLVParser), type.FullName, nameof(IParser));
                }
            }
        }

        private static bool IsTLVParserAttrbute(Attribute[] o)
        {
            foreach (Attribute a in o)
            {
                if (a is attributes.TLVParser)
                    return true;
            }
            return false;
        }

        public string Parse(IByteBuffer buf)
        {
            Dictionary<short, byte[]> tlv_map = buf.ReadTLVMap();
            StringBuilder sb = new StringBuilder();
            sb.Append($"{tlv_map.Count.HexPadLeft().HexDump()} //{tlv_map.Count}tlvs").AppendLine().AppendLine();
            tlv_map.ToList().ForEach(tlv =>
            {
                string text = string.Empty;
                string tag = tlv.Key.HexPadLeft().HexDump();
                string length = tlv.Value.Length.HexPadLeft().HexDump();
                string value = tlv.Value.HexDump();
                //注册一个map key=tag value=IParser
                //根据tlv tag到map中找到对应的IParser，存在则调用parse方法，并使用其返回值，否则默认hexDump()
                KeyValuePair<short, IParser> parser = Parsers.FirstOrDefault(p => p.Key == tlv.Key);
                if (parser.Key != 0 && parser.Value != null)
                {
                    //parse失败返回原样字节并备注解析失败
                    try
                    {
                        value = parser.Value.Parse(Unpooled.WrappedBuffer(tlv.Value));
                    }
                    catch (Exception ex)
                    {
                        value = tlv.Value.HexDump() + " //tlv parse error, please check log";
                        //Logger.Warn(ex, "TLV Parse Error, Tag=[{0}], Value=[{1}]", parser.Key.HexPadLeft().HexDump(),
                           // tlv.Value.HexDump());
                    }
                }
                if (tlv_map.Count(d => d.Key == tlv.Key) > 1)
                {
                    text += $"{tag} //tlv{tag} 重复的tag {Environment.NewLine}";
                }
                else
                {
                    text += $"{tag} //tlv{tag} {Environment.NewLine}";
                }
                text += $"{length} //len={tlv.Value.Length} {Environment.NewLine}";
                text += $"{value} {Environment.NewLine}{Environment.NewLine}";
                sb.Append(text);
            });
            return sb.ToString();
        }
    }
}
