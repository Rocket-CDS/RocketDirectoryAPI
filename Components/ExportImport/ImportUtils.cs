using DNNrocketAPI;
using DNNrocketAPI.Components;
using RocketDirectoryAPI.Components;
using Simplisity;
using Simplisity.TemplateEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;

namespace RocketDirectoryAPI.Components
{
    public static class ImportUtils
    {
        private const string _tableName = "RocketDirectoryAPI";

        public static Dictionary<string, int> ImportData(int portalId, string systemKey, string zipFileMapPath, string importSystemKey)
        {
            var itemIdMap = new Dictionary<string, int>();

            if (!File.Exists(zipFileMapPath)) return itemIdMap;

            var objCtrl = new DNNrocketController();
            var tempRelFolder = PortalUtils.HomeDNNrocketDirectoryRel(portalId).TrimEnd('/') + "/" + systemKey + "/import";
            var tempFolderMapPath = DNNrocketUtils.MapPath(tempRelFolder);



            if (Directory.Exists(tempFolderMapPath)) Directory.Delete(tempFolderMapPath, true);
            ZipFile.ExtractToDirectory(zipFileMapPath, tempFolderMapPath);

            if (Directory.Exists(tempFolderMapPath))
            {

                var fList = Directory.GetFiles(tempFolderMapPath, "*.xml");

                // Update DB, Map ItemId to new ItemId
                foreach (var f in fList)
                {
                    var dataXml = FileUtils.ReadFile(f);
                    var sRec = new SimplisityRecord();
                    sRec.FromXmlItem(dataXml);

                    var oldPortalId = sRec.PortalId;
                    sRec.PortalId = portalId;
                    var oldItemId = sRec.ItemID;
                    sRec.ItemID = -1;
                    sRec.UserId = 0; // userid will be corrupt in new Portal, but is not normally used.
                    sRec.SetXmlProperty("genxml/importkey", oldItemId.ToString());
                    if (systemKey != importSystemKey && importSystemKey != "")
                    {
                        if (sRec.TypeCode.StartsWith(importSystemKey))
                        {
                            sRec.TypeCode = sRec.TypeCode.Replace(importSystemKey, systemKey);
                            if (sRec.GetXmlProperty("genxml/systemkey") != "") sRec.SetXmlProperty("genxml/systemkey", systemKey);
                        }
                    }

                    if (sRec.TypeCode == systemKey + "CATALOGSETTINGS" || sRec.TypeCode == systemKey + "PortalCatalog") // do not do languages. (not need)
                    {
                        // Overwrite Settings, if they exist.
                        var sql2 = "select ItemId from {databaseOwner}[{objectQualifier}" + _tableName + "] where portalid = " + portalId + " and Typecode = '" + sRec.TypeCode + "' for xml raw ";
                        var rows = objCtrl.ExecSql(sql2);
                        var rowsRec = new SimplisityRecord();
                        rowsRec.XMLData = "<genxml><rows>" + rows + "</rows></genxml>";
                        var rowsList = rowsRec.GetRecordList("rows");
                        if (rowsList.Count > 1)
                        {
                            // delete duplicates, take first.
                            for (int i = 1; i < rowsList.Count; i++)
                            {
                                objCtrl.Delete(rowsList[i].GetXmlPropertyInt("row/@ItemId"), _tableName);
                            }
                        }
                        if (rowsList.Count > 0)
                        {
                            var newitemid = rowsList[0].GetXmlPropertyInt("row/@ItemId");
                            if (newitemid > 0) sRec.ItemID = newitemid;
                        }
                    }


                    // ----------------------------------------------------------------------------------------
                    // change Portal ID for any paths 
                    // WARNING: ** This path update will only work for DNN **
                    sRec.XMLData = sRec.XMLData.Replace("Portals/" + oldPortalId, "/Portals/" + portalId);
                    sRec.XMLData = sRec.XMLData.Replace("/Portals/" + oldPortalId, "/Portals/" + portalId);
                    sRec.XMLData = sRec.XMLData.Replace("/" + importSystemKey + "/", "/" + systemKey + "/");
                    // ----------------------------------------------------------------------------------------

                    var newItemId = objCtrl.Update(sRec, _tableName);
                    if (!itemIdMap.ContainsKey(oldItemId.ToString())) itemIdMap.Add(oldItemId.ToString(), newItemId);
                }

                // update all perentItemId and XrefItemId
                foreach (var r in itemIdMap)
                {
                    LogUtils.LogSystem("Import Record - OLD: " + r.Key + " NEW: " + r.Value);
                    var sRec = objCtrl.GetRecord(r.Value, _tableName);
                    if (sRec != null)
                    {
                        var newparentId = 0;
                        if (itemIdMap.ContainsKey(sRec.ParentItemId.ToString())) newparentId = itemIdMap[sRec.ParentItemId.ToString()];
                        sRec.ParentItemId = newparentId;

                        var newxrefId = 0;
                        if (itemIdMap.ContainsKey(sRec.XrefItemId.ToString())) newxrefId = itemIdMap[sRec.XrefItemId.ToString()];
                        sRec.XrefItemId = newxrefId;

                        objCtrl.Update(sRec, _tableName);
                    }
                }

                // rebuild index
                foreach (var r in itemIdMap)
                {
                    var sRec = objCtrl.GetRecord(r.Value, _tableName);
                    if (sRec != null)
                    {
                        if (!sRec.TypeCode.EndsWith("LANG")) objCtrl.RebuildLangIndex(sRec.PortalId, sRec.ItemID, _tableName);
                    }
                }

                Directory.Delete(tempFolderMapPath, true);

                // Update List and Detail pages.
                var rocketSettings = DNNrocketUtils.GetTempRecordStorage(portalId + "RocketSettings.xml", true); // Saved in DNNrocket\API\Components\DnnSiteExportImportHelper.cs / ImportWebsiteAndWait()
                var oldListTabPath = rocketSettings.GetXmlProperty("genxml/" + systemKey + "/listtabpath");
                var oldDetailTabPath = rocketSettings.GetXmlProperty("genxml/" + systemKey + "/detailtabpath");

                var portalContent = new PortalCatalogLimpet(portalId, "en-US", systemKey); // culturecode not required for data
                portalContent.Record.SetXmlProperty("genxml/listpage", GetTabIdByTabPath(portalId, oldListTabPath).ToString());
                portalContent.Record.SetXmlProperty("genxml/detailpage", GetTabIdByTabPath(portalId, oldDetailTabPath).ToString());
                portalContent.Update();

            }
            return itemIdMap;
        }
        public static void ImportImgs(int portalId, string cultureCode, string systemKey, string zipFileMapPath, Dictionary<string, int> itemIdMap)
        {
            var PortalContent = new PortalCatalogLimpet(portalId, cultureCode, systemKey);
            if (!Directory.Exists(PortalContent.ImageFolderMapPath)) Directory.CreateDirectory(PortalContent.ImageFolderMapPath);
            var tempRelFolder = PortalUtils.HomeDNNrocketDirectoryRel(portalId).TrimEnd('/') + "/" + systemKey + "/tempextract";
            var tempFolderMapPath = DNNrocketUtils.MapPath(tempRelFolder);
            UnZipOverwriteSubCat(zipFileMapPath, tempFolderMapPath, PortalContent.ImageFolderMapPath, itemIdMap);
        }
        public static void ImportDocs(int portalId, string cultureCode, string systemKey, string zipFileMapPath)
        {
            var PortalContent = new PortalCatalogLimpet(portalId, cultureCode, systemKey);
            if (!Directory.Exists(PortalContent.DocFolderMapPath)) Directory.CreateDirectory(PortalContent.DocFolderMapPath);
            var tempRelFolder = PortalUtils.HomeDNNrocketDirectoryRel(portalId).TrimEnd('/') + "/" + systemKey + "/tempextract";
            var tempFolderMapPath = DNNrocketUtils.MapPath(tempRelFolder);
            UnZipOverwrite(zipFileMapPath, tempFolderMapPath, PortalContent.DocFolderMapPath);
        }
        private static void UnZipOverwrite(string zipPath, string tempPath, string extractPath)
        {
            try
            {
                if (!File.Exists(zipPath)) return;

                if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
                ZipFile.ExtractToDirectory(zipPath, tempPath);

                //build an array of the unzipped files
                string[] files = Directory.GetFiles(tempPath);

                foreach (string file in files)
                {
                    FileInfo f = new FileInfo(file);
                    //Check if the file exists already, if so delete it and then move the new file to the extract folder
                    if (!Directory.Exists(extractPath)) Directory.CreateDirectory(extractPath);
                    if (File.Exists(Path.Combine(extractPath, f.Name)))
                    {
                        File.Delete(Path.Combine(extractPath, f.Name));
                        File.Move(f.FullName, Path.Combine(extractPath, f.Name));
                    }
                    else
                    {
                        File.Move(f.FullName, Path.Combine(extractPath, f.Name));
                    }
                }

                //Delete the temporary directory.
                Directory.Delete(tempPath);
            }
            catch (Exception ex)
            {
                LogUtils.LogException(ex);
            }
        }

        private static void UnZipOverwriteSubCat(string zipPath, string tempPath, string extractPath, Dictionary<string, int> itemIdMap)
        {
            try
            {
                if (!File.Exists(zipPath)) return;

                var objCtrl = new DNNrocketController();

                if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true); 
                ZipFile.ExtractToDirectory(zipPath, tempPath);

                foreach (var d in Directory.GetDirectories(tempPath))
                {
                    //build an array of the unzipped files
                    string[] files = Directory.GetFiles(d);

                    foreach (string file in files)
                    {
                        FileInfo f = new FileInfo(file);
                        var oldItemId = Path.GetFileName(Path.GetDirectoryName(file));
                        if (itemIdMap.ContainsKey(oldItemId))
                        {

                            var newitemId = itemIdMap[oldItemId];

                            var articleRec = objCtrl.GetRecord(newitemId, _tableName);

                            //Check if the file exists already, if so delete it and then move the new file to the extract folder
                            var extractSubCat = extractPath + "\\" + newitemId;
                            if (!Directory.Exists(extractSubCat)) Directory.CreateDirectory(extractSubCat);
                            var newFileMapPath = Path.Combine(extractSubCat, f.Name);

                            if (File.Exists(newFileMapPath))
                            {
                                File.Delete(newFileMapPath);
                                File.Move(f.FullName, newFileMapPath);
                            }
                            else
                            {
                                File.Move(f.FullName, newFileMapPath);
                            }

                            var lp = 1;
                            var imgList = articleRec.GetRecordList("imagelist");
                            foreach (var i in imgList)
                            {
                                if (i.GetXmlProperty("genxml/hidden/imagepatharticleimage").EndsWith("/" + oldItemId + "/" + f.Name))
                                {
                                    articleRec.SetXmlProperty("genxml/imagelist/genxml[" + lp + "]/hidden/imagepatharticleimage", i.GetXmlProperty("genxml/hidden/imagepatharticleimage").Replace("/" + oldItemId + "/" + f.Name, "/" + newitemId + "/" + f.Name));
                                }
                                lp += 1;
                            }
                            objCtrl.Update(articleRec, _tableName);
                        }
                    }

                }

                // Move legacy root level images
                string[] files2 = Directory.GetFiles(tempPath);
                foreach (string file in files2)
                {
                    FileInfo f = new FileInfo(file);
                    //Check if the file exists already, if so delete it and then move the new file to the extract folder
                    var newFileMapPath = Path.Combine(extractPath, f.Name);
                    if (File.Exists(newFileMapPath))
                    {
                        File.Delete(newFileMapPath);
                        File.Move(f.FullName, newFileMapPath);
                    }
                    else
                    {
                        File.Move(f.FullName, newFileMapPath);
                    }
                    // rename of the path is attempted on import. (May not be 100%)
                }
                //Delete the temporary directory.
                Directory.Delete(tempPath, true);
            }
            catch (Exception ex)
            {
                LogUtils.LogException(ex);
            }
        }
        private static int GetTabIdByTabPath(int portalId, string tabpath)
        {
            var l = DNNrocketUtils.GetTabList(portalId);
            foreach (var t in l)
            {
                if (t.GetXmlProperty("genxml/tabpath") == tabpath) return t.GetXmlPropertyInt("genxml/tabid");
            }
            return -1;
        }


    }
}
