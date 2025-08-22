#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ExcaliburAI.AI
{
    public static class CsvUtil
    {
        public static List<Dictionary<string,string>> Read(string path)
        {
            var list = new List<Dictionary<string,string>>();
            if (!File.Exists(path)) return list;
            var lines = File.ReadAllLines(path);
            if (lines.Length == 0) return list;
            var headers = lines[0].Split(',');
            for (int i=1;i<lines.Length;i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                var vals = SplitCsv(lines[i]);
                var row = new Dictionary<string,string>();
                for (int c=0;c<headers.Length && c<vals.Count;c++) row[headers[c]] = vals[c];
                list.Add(row);
            }
            return list;
        }

        public static void Write(string path, List<Dictionary<string,string>> rows)
        {
            if (rows == null) rows = new List<Dictionary<string,string>>();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var headers = new List<string>(rows.Count>0 ? rows[0].Keys : new string[]{"id","name"});
            using (var sw = new StreamWriter(path))
            {
                sw.WriteLine(string.Join(",", headers.ToArray()));
                foreach (var r in rows)
                {
                    var vals = new List<string>();
                    foreach (var h in headers) vals.Add(Escape(r.ContainsKey(h)? r[h] : ""));
                    sw.WriteLine(string.Join(",", vals.ToArray()));
                }
            }
        }

        static string Escape(string s) { if (s==null) s=""; if (s.Contains(",") || s.Contains("\"")) return "\""+ s.Replace("\"","\"\"") +"\""; return s; }
        static List<string> SplitCsv(string line)
        {
            var res = new List<string>(); bool inQ=false; System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i=0;i<line.Length;i++)
            {
                char ch = line[i];
                if (inQ) { if (ch=='"') { if (i+1<line.Length && line[i+1]=='"'){sb.Append('"'); i++;} else inQ=false; } else sb.Append(ch); }
                else { if (ch=='"') inQ=true; else if (ch==','){res.Add(sb.ToString()); sb.Length=0;} else sb.Append(ch); }
            }
            res.Add(sb.ToString());
            return res;
        }
    }
}
#endif
