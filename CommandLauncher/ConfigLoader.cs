﻿/*
 * TeaLauncher. Simple command launcher.
 * Copyright (C) Toshiyuki Hirooka <toshi.hirooka@gmail.com> http://wasabi.in/
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CommandLauncher
{
    // exceptions
    public class ConfigLoaderNotExistsSectionException : Exception { }
    public class ConfigLoaderMultiKeyException : Exception { }
    public class ConfigLoaderNotKeyValueException : Exception { }
    public class ConfigLoaderSameSectionException : Exception { }

    public class ConfigLoader
    {
        Dictionary<string, Dictionary<string, string>> m_Conf = new();

        public void LoadConfigFile(string filename)
        {
            ParseConfig(new StreamReader(filename));
        }

        public void ParseConfig(StreamReader stream)
        {
            string section = null;

            string line = "";
            while (true)
            {
                line = stream.ReadLine();

                if (line == null)
                    break;

                string trimed = line.Trim();

                if (trimed == "")
                    continue;

                // セクションのときの処理
                if (trimed[0] == '[' && trimed[trimed.Length - 1] == ']')
                {
                    section = trimed.Substring(1, trimed.Length - 2);

                    if (m_Conf.ContainsKey(section))
                        throw new ConfigLoaderSameSectionException();
                }
                else // キーと値のときの処理
                {
                    if (section == null)
                        throw new ConfigLoaderNotExistsSectionException();

                    string[] splitted = line.Split(new char[] { '=' });
                    if (splitted.Length == 1)
                        throw new ConfigLoaderNotKeyValueException();
                    if (splitted.Length > 2)
                    {
                        for (int i = 2; i < splitted.Length; i++)
                        {
                            splitted[1] += "=" + splitted[i];
                        }
                    }

                    string key = splitted[0].Trim();
                    string value = splitted[1].Trim();

                    // 無かったら登録
                    if (!m_Conf.ContainsKey(section))
                        m_Conf[section] = new Dictionary<string, string>();

                    // 登録
                    Dictionary<string, string> ht = m_Conf[section];
                    try
                    {
                        ht.Add(key, value);
                    }
                    catch (Exception)
                    {
                        throw new ConfigLoaderMultiKeyException();
                    }
                }
            }

            stream.Close();
        }

        public List<string> GetSections()
        {
            List<string> list = new List<string>();
            foreach (var d in m_Conf)
                list.Add(d.Key);
            return list;
        }

        public Dictionary<string, string> GetConfig(string section)
        {
            return m_Conf[section];
        }
    }
}
