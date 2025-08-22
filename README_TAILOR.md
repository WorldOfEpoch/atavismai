# ExcaliburAI — Tailor Kit for Atavism (Auto-Mapping)

This kit extends the previous skeleton with an **auto-mapper** that connects to your MySQL/MariaDB and writes a precise `Config/content_map.json` based on your actual Atavism tables/columns.

## What it does
- Scans one or more DB schemas (from `Config/db.env`) via INFORMATION_SCHEMA
- Heuristically detects **items, drops/loot, vendors/merchants, skills, quests, mobs**
- Writes:
  - `Config/content_map.json` (tailored mapping)
  - `Assets/ExcaliburAI/Staging/schema_report.md` (tables/columns overview)

## How to run
1. Copy `Assets/` and `Config/` into your Unity project (or on top of the skeleton you already imported).
2. Update `Config/db.env`:
   - `MYSQL_HOST=...`
   - `MYSQL_USER=epochftp`
   - `MYSQL_PASS=Scretnat123!`
   - Either:
     - `MYSQL_DB=atavism` (single DB)
     - or `MYSQL_DB_LIST=admin,atavism,master,world_content` (multiple schemas to scan)
3. Ensure **Newtonsoft.Json** and **MySqlConnector.dll** are available (see README in Plugins).
4. Unity menu: **ExcaliburAI → Atavism → Auto-tailor Mapping**.
5. Check the generated `schema_report.md`. If anything looks off, tweak `content_map.json` manually.

> Safety: The mapper is **read-only**; it does not modify your DB.
