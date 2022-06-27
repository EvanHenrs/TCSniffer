﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TCSniffer.common;
using TCSniffer.utils;

namespace TCSniffer.form
{
    [attributes.CustomEvent(nameof(TabTEA))]
    public class TabTEA : ICustomControlEvents
    {
        private static Form1 Frm => Form1.Form;

        public void Register()
        {
            Frm.btn_tea_encrypt.Click += Btn_tea_encrypt_Click;
            Frm.btn_tea_decrypt.Click += Btn_tea_decrypt_Click;
            Frm.btn_tea_copy_decrypt_data.Click += Btn_tea_copy_decrypt_data_Click;
            Frm.btn_tea_key_log_decrypt.Click += Btn_tea_key_log_decrypt_Click;
            Frm.btn_decrypt_byte_by_byte.Click += Btn_decrypt_byte_by_byte_Click;
        }

        #region TAB TEA加解密
        private void Btn_tea_encrypt_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] key = Frm.txt_tea_key.Text.DecodeHex();
                byte[] data = Frm.txt_tea_encrypt_data.Text.DecodeHex();
                byte[] ret = Tea.Encrypt(data, key);
                if (ret != null)
                {
                    Frm.txt_tea_decrypt_data.Text = ret.HexDump();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void Btn_tea_decrypt_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] key = Frm.txt_tea_key.Text.DecodeHex();
                byte[] data = Frm.txt_tea_encrypt_data.Text.DecodeHex();
                byte[] ret = Tea.Decrypt(data, key);
                if (ret != null)
                {
                    Frm.txt_tea_decrypt_data.Text = ret.HexDump();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void Btn_tea_copy_decrypt_data_Click(object sender, EventArgs e)
        {
            string ret = Frm.txt_tea_decrypt_data.Text;
            if (!string.IsNullOrEmpty(ret))
            {
                Clipboard.SetText(ret);
            }
        }

        private void Btn_tea_key_log_decrypt_Click(object sender, EventArgs e)
        {
            string encrypt_data = Frm.txt_tea_encrypt_data.Text;
            if (string.IsNullOrEmpty(encrypt_data)) return;
            byte[] data = encrypt_data.DecodeHex();
            byte[] decrypt_data = Common.TeaKeyLogDecrypt(data, out DecryptionKey decryptionKey);
            if (decrypt_data != null)
            {
                Frm.txt_tea_decrypt_data.Text = decrypt_data.HexDump();
                Frm.txt_tea_key.Text = decryptionKey.Key;
            }
        }

        private void Btn_decrypt_byte_by_byte_Click(object sender, EventArgs e)
        {
            string encrypt_data = Frm.txt_tea_encrypt_data.Text.ClearSpecialSymbols();
            if (string.IsNullOrEmpty(encrypt_data)) return;
            for (int i = 0; i < encrypt_data.Length; i += 2)
            {
                byte[] data = encrypt_data.Substring(i, encrypt_data.Length - i).DecodeHex();
                byte[] decrypt_data = Common.TeaKeyLogDecrypt(data, out DecryptionKey decryptionKey);
                if (decrypt_data != null)
                {
                    Frm.txt_tea_decrypt_data.Text = decrypt_data.HexDump();
                    Frm.txt_tea_key.Text = decryptionKey.Key;
                    Toast.Info($"逐字节KEY日志解密成功, 共读取了[{i}]个字节长度");
                    return;
                }
            }
            Toast.Info("逐字节KEY日志解密失败, 未找到匹配的数据");
        }
        #endregion
    }
}
