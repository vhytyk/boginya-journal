using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;
using System.Reflection;
using BLToolkit.Data;
using BLToolkit.DataAccess;
using System.Text.RegularExpressions;
using System.IO;
using BoginyaJournal.Entities;

namespace BoginyaJournal.DbMigration
{
    public class DBMigration
    {
        const string rollBackTemplate = "rollback_ver";
        const string migrateTemplate = "migrate_ver";
        public static void Migrate()
        {
            using (DbManager db = new DbManager())
            {
                DbVersion currentVersion = null;
                int dbVersion = 0;
                try
                {
                    List<DbVersion> list = new SqlQuery<DbVersion>(db).SelectAll();
                    if (list.Count > 0)
                    {
                        currentVersion = list[0];
                        dbVersion = currentVersion.CurrentValue;
                    }
                }
                catch { }

                Assembly currentAssembly = Assembly.GetCallingAssembly();
                var migrateScriptNames = from resourceName in currentAssembly.GetManifestResourceNames()
                                         where resourceName.Contains(migrateTemplate)
                                         select resourceName;
                foreach (string scriptName in migrateScriptNames)
                {
                    Match verMatch = Regex.Match(scriptName, string.Format(@"{0}(\d+)",migrateTemplate));
                    int version = int.Parse(verMatch.Groups[1].Value);
                    if (verMatch.Success && version > dbVersion)
                    {
                        using (StreamReader reader = new StreamReader(currentAssembly.GetManifestResourceStream(scriptName)))
                        {
                            string allScript = reader.ReadToEnd();
                            foreach (string command in allScript.Split(new string[] { "----------" }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                db.SetCommand(command);
                                db.ExecuteNonQuery();
                            }
                            if (null != currentVersion)
                            {
                                currentVersion.CurrentValue = version;
                                new SqlQuery<DbVersion>(db).Update(currentVersion);
                            }
                        }
                    }

                }
            }
        }
        public static void Rollback()
        {
            using (DbManager db = new DbManager())
            {
                DbVersion currentVersion = null;
                int dbVersion = 0;
                try
                {
                    List<DbVersion> list = new SqlQuery<DbVersion>(db).SelectAll();
                    if (list.Count > 0)
                    {
                        currentVersion = list[0];
                        dbVersion = currentVersion.CurrentValue;
                    }
                }
                catch { }

                Assembly currentAssembly = Assembly.GetCallingAssembly();
                var migrateScriptNames = from resourceName in currentAssembly.GetManifestResourceNames()
                                         where resourceName.Contains( rollBackTemplate+dbVersion.ToString() )
                                         select resourceName;
                if (migrateScriptNames.Count() == 0)
                    throw new Exception("can't found rollback script for version " + dbVersion.ToString());
                else
                {
                    string scriptName = migrateScriptNames.ElementAt(0);
                    Match verMatch = Regex.Match(scriptName, string.Format(@"{0}(\d+)",rollBackTemplate));
                    int version = int.Parse(verMatch.Groups[1].Value);
                    if (verMatch.Success && version == dbVersion)
                    {
                        using (StreamReader reader = new StreamReader(currentAssembly.GetManifestResourceStream(scriptName)))
                        {
                            string allScript = reader.ReadToEnd();
                            foreach (string command in allScript.Split(new string[] { "----------" }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                db.SetCommand(command);
                                db.ExecuteNonQuery();
                            }
                            if (null != currentVersion && version > 1)
                            {
                                currentVersion.CurrentValue = version - 1;
                                new SqlQuery<DbVersion>(db).Update(currentVersion);
                            }
                        }
                    }

                }
            }
        }
    }
}
