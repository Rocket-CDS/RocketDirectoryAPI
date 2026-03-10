using DNNrocketAPI;
using DNNrocketAPI.Components;
using RocketDirectoryAPI.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace RocketDirectoryAPI.Components
{
    public static class ExportUtils
    {
        public static List<string> GetAllRecordTypes(int portalId, string systemKey)
        {
            // get ALL record types.
            var objCtrl = new DNNrocketController();
            var rTypes = new List<string>();
            var l = objCtrl.GetList(portalId, -1, "", "", "", "", 0, 0, 0, 0, "RocketDirectoryAPI");
            foreach (var r in l)
            {
                if (!r.TypeCode.Contains("LANGIDX") && !r.TypeCode.Contains("IDX_") && !r.TypeCode.EndsWith("PortalContent"))  // we don't need the index records.
                {
                    if (r.TypeCode.StartsWith(systemKey)) // we only want the system type.
                    {
                        if (!rTypes.Contains(r.TypeCode)) rTypes.Add(r.TypeCode);
                    }
                }
            }
            return rTypes;
        }
        public static void DoXmlExport(int portalId, string systemKey, string tempFolderMapPath, List<string> rTypes)
        {
            var objCtrl = new DNNrocketController();
            var xrefList = new List<int>();
            foreach (var rType in rTypes)
            {
                var l2 = objCtrl.GetList(portalId, -1, rType, "", "", "", 0, 0, 0, 0, "RocketDirectoryAPI");
                foreach (var r2 in l2)
                {
                    var rtn = r2.ToXmlItem();
                    if (!xrefList.Contains(r2.ParentItemId)) xrefList.Add(r2.ParentItemId);
                    var fullFileName = tempFolderMapPath + "\\" + rType + "_" + portalId + "_" + r2.ItemID + ".xml";
                    FileUtils.SaveFile(fullFileName, rtn);
                }
            }
            foreach (var xrParentId in xrefList)
            {
                var rTypes2 = new List<string>();
                rTypes2.Add("CATXREF");
                rTypes2.Add("PROPXREF");
                foreach (var rType2 in rTypes2)
                {
                    var l2 = objCtrl.GetList(portalId, -1, rType2, " and r1.ParentItemId = " + xrParentId + " ", "", "", 0, 0, 0, 0, "RocketDirectoryAPI");
                    foreach (var r2 in l2)
                    {
                        var rtn = r2.ToXmlItem();
                        var fullFileName = tempFolderMapPath + "\\" + rType2 + "_" + portalId + "_" + r2.ItemID + ".xml";
                        FileUtils.SaveFile(fullFileName, rtn);
                    }
                }
            }
        }
        public static void ExportData(int portalId, string cultureCode, string systemKey = "rocketdirectoryapi")
        {
            var objCtrl = new DNNrocketController();
            var tempRelFolder = PortalUtils.HomeDNNrocketDirectoryRel(portalId).TrimEnd('/') + "/" + systemKey + "/export";
            var tempFolderMapPath = DNNrocketUtils.MapPath(tempRelFolder);
            if (!Directory.Exists(tempFolderMapPath)) Directory.CreateDirectory(tempFolderMapPath);

            // get ALL record types.
            var rTypes = GetAllRecordTypes(portalId, systemKey);

            // Export Data record.
            DoXmlExport(portalId, systemKey, tempFolderMapPath, rTypes);

            var zipRelFolder = PortalUtils.HomeDNNrocketDirectoryRel(portalId).TrimEnd('/') + "/" + systemKey + "/exportzip";
            var zipFolderMapPath = DNNrocketUtils.MapPath(zipRelFolder);
            if (Directory.Exists(zipFolderMapPath)) Directory.Delete(zipFolderMapPath, true);
            Directory.CreateDirectory(zipFolderMapPath);

            ZipFile.CreateFromDirectory(tempFolderMapPath, zipFolderMapPath + "\\exportdata_" + systemKey + ".zip");
        }
        public static void ExportImages(int portalId, string cultureCode, string systemKey = "rocketdirectoryapi")
        {
            var zipRelFolder = PortalUtils.HomeDNNrocketDirectoryRel(portalId).TrimEnd('/') + "/" + systemKey + "/exportzip";
            var zipFolderMapPath = DNNrocketUtils.MapPath(zipRelFolder);
            var PortalContent = new PortalCatalogLimpet(portalId, cultureCode, systemKey);
            ZipFile.CreateFromDirectory(PortalContent.ImageFolderMapPath, zipFolderMapPath + "\\exportimg_" + systemKey + ".zip");
        }
        public static void ExportDocs(int portalId, string cultureCode, string systemKey = "rocketdirectoryapi")
        {
            var zipRelFolder = PortalUtils.HomeDNNrocketDirectoryRel(portalId).TrimEnd('/') + "/" + systemKey + "/exportzip";
            var zipFolderMapPath = DNNrocketUtils.MapPath(zipRelFolder);
            var PortalContent = new PortalCatalogLimpet(portalId, cultureCode, systemKey);
            ZipFile.CreateFromDirectory(PortalContent.DocFolderMapPath, zipFolderMapPath + "\\exportdoc_" + systemKey + ".zip");
        }
        public static string ExportDownloadFile(int portalId, string cultureCode, string systemKey = "rocketdirectoryapi")
        {
            var zipRelFolder = PortalUtils.HomeDNNrocketDirectoryRel(portalId).TrimEnd('/') + "/" + systemKey + "/exportzip";
            var zipFolderMapPath = DNNrocketUtils.MapPath(zipRelFolder);
            var tempRelFolder = PortalUtils.HomeDNNrocketDirectoryRel(portalId).TrimEnd('/') + "/" + systemKey + "/export";
            var tempFolderMapPath = DNNrocketUtils.MapPath(tempRelFolder);

            var exportpath = PortalUtils.HomeDNNrocketDirectoryMapPath() + "\\ImportExportDirectory";
            File.Delete(exportpath);
            ZipFile.CreateFromDirectory(zipFolderMapPath, exportpath);

            Directory.Delete(zipFolderMapPath, true);
            Directory.Delete(tempFolderMapPath, true);
            return exportpath;
        }

    }
}
