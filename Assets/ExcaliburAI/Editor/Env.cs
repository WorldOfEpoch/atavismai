using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Env helper for reading/writing Atavism-style DB settings from Config/db.env
/// Supports granular keys per database and an optional MYSQL_DB_LIST.
/// </summary>
public static class Env
{
    public class DbConfig
    {
        public string Host = "127.0.0.1";
        public int Port = 3306;
        public string User = "root";
        public string Pass = "";
        public string DB_Admin = "admin";
        public string DB_Atavism = "atavism";
        public string DB_Master = "master";
        public string DB_World = "world_content";
    }

    static string GetValue(string envPath, string key)
    {
        if (!File.Exists(envPath)) return null;
        foreach (var raw in File.ReadAllLines(envPath))
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            var line = raw.Trim();
            if (line.StartsWith("#")) continue;
            var i = line.IndexOf('=');
            if (i <= 0) continue;
            var k = line.Substring(0, i).Trim();
            if (!string.Equals(k, key, StringComparison.OrdinalIgnoreCase)) continue;
            return line.Substring(i + 1).Trim();
        }
        return null;
    }

    public static DbConfig LoadDb(string envPath = "Config/db.env")
    {
        var cfg = new DbConfig();
        if (!File.Exists(envPath)) return cfg;

        foreach (var raw in File.ReadAllLines(envPath))
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            var line = raw.Trim();
            if (line.StartsWith("#")) continue;
            var i = line.IndexOf('=');
            if (i <= 0) continue;
            var key = line.Substring(0, i).Trim();
            var val = line.Substring(i + 1).Trim();

            switch (key)
            {
                case "MYSQL_HOST": cfg.Host = val; break;
                case "MYSQL_PORT":
                    if (int.TryParse(val, out var p)) cfg.Port = p;
                    break;
                case "MYSQL_USER": cfg.User = val; break;
                case "MYSQL_PASS": cfg.Pass = val; break;
                case "MYSQL_DB": cfg.DB_Atavism = val; break; // legacy single DB
                case "MYSQL_DB_ADMIN": cfg.DB_Admin = val; break;
                case "MYSQL_DB_ATAVISM": cfg.DB_Atavism = val; break;
                case "MYSQL_DB_MASTER": cfg.DB_Master = val; break;
                case "MYSQL_DB_WORLD": cfg.DB_World = val; break;
            }
        }
        return cfg;
    }

    public static void SaveDb(DbConfig cfg, string envPath = "Config/db.env")
    {
        Directory.CreateDirectory(Path.GetDirectoryName(envPath) ?? "Config");
        var lines = new List<string>
        {
            "# ExcaliburAI / Atavism DB settings",
            "MYSQL_HOST=" + cfg.Host,
            "MYSQL_PORT=" + cfg.Port,
            "MYSQL_USER=" + cfg.User,
            "MYSQL_PASS=" + cfg.Pass,
            "MYSQL_DB_ADMIN=" + cfg.DB_Admin,
            "MYSQL_DB_ATAVISM=" + cfg.DB_Atavism,
            "MYSQL_DB_MASTER=" + cfg.DB_Master,
            "MYSQL_DB_WORLD=" + cfg.DB_World,
            "# Optional: MYSQL_DB_LIST=admin,atavism,master,world_content"
        };
        File.WriteAllLines(envPath, lines);
    }

    /// <summary>Returns distinct DB names from explicit fields plus optional MYSQL_DB_LIST.</summary>
    public static string[] GetAllDbNames(string envPath = "Config/db.env")
    {
        var cfg = LoadDb(envPath);
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            cfg.DB_Admin, cfg.DB_Atavism, cfg.DB_Master, cfg.DB_World
        };
        var list = GetValue(envPath, "MYSQL_DB_LIST");
        if (!string.IsNullOrWhiteSpace(list))
            foreach (var s in list.Split(',')) { var v = s.Trim(); if (v.Length > 0) set.Add(v); }
        return set.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
    }

    /// <summary>Connection string with no default database (good for INFORMATION_SCHEMA queries).</summary>
    public static string ConnStrServer(DbConfig cfg = null, string envPath = "Config/db.env")
    {
        cfg ??= LoadDb(envPath);
        return $"Server={cfg.Host};Port={cfg.Port};User ID={cfg.User};Password={cfg.Pass};Allow User Variables=True;SslMode=None;Default Command Timeout=120";
    }

    /// <summary>Connection string to a specific DB name.</summary>
    public static string ConnStr(string dbName, DbConfig cfg = null, string envPath = "Config/db.env")
    {
        cfg ??= LoadDb(envPath);
        return $"Server={cfg.Host};Port={cfg.Port};Database={dbName};User ID={cfg.User};Password={cfg.Pass};Allow User Variables=True;SslMode=None;Default Command Timeout=120";
    }

    public static string GetMySqlConnString(string envPath = "Config/db.env")
    {
        var cfg = LoadDb(envPath);
        return ConnStr(cfg.DB_Atavism, cfg, envPath);
    }
}
