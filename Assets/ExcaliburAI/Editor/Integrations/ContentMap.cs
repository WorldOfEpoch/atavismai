using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

public class ContentMap
{
    public TableMap Items;
    public TableMap Drops;
    public TableMap Vendors;
    public TableMap Skills;

    public static ContentMap Load(string path)
    {
        var json = JObject.Parse(File.ReadAllText(path));
        var m = new ContentMap();
        if (json["items"] is JObject it) m.Items = TableMap.FromJson(it);
        if (json["drops"] is JObject dr) m.Drops = TableMap.FromJson(dr);
        if (json["vendors"] is JObject ve) m.Vendors = TableMap.FromJson(ve);
        if (json["skills"] is JObject sk) m.Skills = TableMap.FromJson(sk);
        return m;
    }
}

public class TableMap
{
    public string Table;
    public List<string> Keys = new();
    public Dictionary<string, string> Columns = new();

    public static TableMap FromJson(JObject j)
    {
        var tm = new TableMap();
        tm.Table = j.Value<string>("table");
        if (j["keys"] is JArray arr)
            foreach (var k in arr) tm.Keys.Add(k.ToString());
        var cols = j["columns"] as JObject;
        if (cols != null)
            foreach (var kv in cols) tm.Columns[kv.Key] = kv.Value.ToString();
        return tm;
    }
}
