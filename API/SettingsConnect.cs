using DNNrocketAPI;
using DNNrocketAPI.Components;
using Rocket.AppThemes.Components;
using RocketDirectoryAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Xml;

namespace RocketDirectoryAPI.API
{
    public partial class StartConnect
    {
        private string SaveSettings()
        {
            var moduleData = _dataObject.ModuleSettings;
            moduleData.Save(_postInfo);
            moduleData.Update();
            _dataObject.SetDataObject("modulesettings", moduleData);
            return RenderSystemTemplate("ModuleSettings.cshtml");
        }
        private string DisplaySettings()
        {
            var moduleData = _dataObject.ModuleSettings;

            // if we have no appThemes download the default
            var appThemeProjectData = new AppThemeProjectLimpet();
            var appThemeList = new AppThemeDataList(_dataObject.PortalId, appThemeProjectData.DefaultProjectName());
            if (appThemeList != null && appThemeList.List.Count == 0)
            {
                appThemeProjectData.DownloadGitHubProject(appThemeProjectData.DefaultProjectName());
            }
            if (_dataObject.PortalContent.ProjectName == "")
            {
                if (!moduleData.HasProject) return RenderSystemTemplate("ModuleSelectProject.cshtml");
                if (!moduleData.HasAppThemeAdmin) return RenderSystemTemplate("ModuleSelectAppTheme.cshtml");
                if (!moduleData.HasAppThemeAdminVersion) return RenderSystemTemplate("ModuleSelectAppThemeVersion.cshtml");
            }
            return RenderSystemTemplate("ModuleSettings.cshtml");
        }
        private string SelectAppThemeProject()
        {
            var moduleData = _dataObject.ModuleSettings;
            moduleData.ProjectName = _paramInfo.GetXmlProperty("genxml/hidden/projectname");
            _dataObject.SetDataObject("modulesettings", moduleData);
            moduleData.Update();
            return RenderSystemTemplate("ModuleSelectAppTheme.cshtml");
        }
        private string SelectAppTheme()
        {
            var moduleData = _dataObject.ModuleSettings;
            moduleData.AppThemeAdminFolder = _paramInfo.GetXmlProperty("genxml/hidden/appthemefolder");
            _dataObject.SetDataObject("modulesettings", moduleData);
            moduleData.Update();
            return RenderSystemTemplate("ModuleSelectAppThemeVersion.cshtml");
        }
        private string SelectAppThemeVersion()
        {
            var moduleData = _dataObject.ModuleSettings;
            moduleData.AppThemeAdminVersion = _paramInfo.GetXmlProperty("genxml/hidden/appthemefolderversion");
            _dataObject.SetDataObject("modulesettings", moduleData);
            moduleData.Update();
            _dataObject.PortalContent.ProjectName = moduleData.ProjectName;
            _dataObject.PortalContent.AppThemeFolder = moduleData.AppThemeAdminFolder;
            _dataObject.PortalContent.AppThemeVersion = moduleData.AppThemeAdminVersion;
            _dataObject.PortalContent.Update();

            return RenderSystemTemplate("ModuleSettings.cshtml");
        }

        private string ExportData()
        {
            // check the scheduler initiated the call.
            var rtn = "";
            var securityKey = DNNrocketUtils.GetTempStorage(_paramInfo.GetXmlProperty("genxml/hidden/securitykey"), true);
            if (securityKey != null) // if it exists in the temp table, it was created by the scheduler.
            {
                var moduleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/moduleid");
                var systemKey = _paramInfo.GetXmlProperty("genxml/hidden/systemkey");
                var portalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid");
                var databasetable = _paramInfo.GetXmlProperty("genxml/hidden/databasetable");
                var moduleRef = portalId + "_ModuleID_" + moduleId;
                var moduleSettings = new ModuleContentLimpet(portalId, moduleRef, systemKey, moduleId, -1);
                if (moduleSettings.HasProject)
                {
                    rtn = "<export>";
                    rtn += "<systemkey>" + systemKey + "</systemkey>";
                    rtn += "<databasetable>RocketDirectoryAPI</databasetable>";
                    rtn += "<modulesettings>";
                    rtn += moduleSettings.Record.ToXmlItem();
                    rtn += "</modulesettings>";
                    rtn += "</export>";
                }
            }

            return rtn;
        }
        private void ImportData()
        {
            // check the scheduler initiated the call.
            var securityKey = DNNrocketUtils.GetTempStorage(_paramInfo.GetXmlProperty("genxml/hidden/securitykey"), true);
            if (securityKey != null) // if it exists in the temp table, it was created by the scheduler.
            {

                var moduleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/moduleid");
                var tabId = _paramInfo.GetXmlPropertyInt("genxml/hidden/tabid");
                var systemKey = _paramInfo.GetXmlProperty("genxml/hidden/systemkey");
                var portalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid");
                var databasetable = _paramInfo.GetXmlProperty("genxml/hidden/databasetable");
                var moduleRef = portalId + "_ModuleID_" + moduleId;

                var objCtrl = new DNNrocketController();

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(_postInfo.XMLData);

                LogUtils.LogSystem("IMPORT XML _paramInfo: " + _paramInfo.XMLData);
                LogUtils.LogSystem("IMPORT XML _postInfo: " + _postInfo.XMLData);

                //import Settings (Saved in DNNrocket table)
                var settingsNod = xmlDoc.SelectSingleNode("export/modulesettings");
                if (settingsNod != null)
                {
                    var legacymoduleref = "";
                    var legacymoduleid = "";
                    var ms = new SimplisityRecord();
                    ms.FromXmlItem(settingsNod.InnerXml);
                    var rec = objCtrl.GetRecordByGuidKey(portalId, moduleId, "MODSETTINGS", moduleRef, "");
                    if (rec != null)
                    {
                        var storeId = rec.ItemID;
                        ms = rec;
                        ms.FromXmlItem(settingsNod.InnerXml);
                        ms.ItemID = storeId;
                    }
                    else
                    {
                        ms.ItemID = -1;
                    }
                    legacymoduleref = ms.GUIDKey;
                    ms.SetXmlProperty("genxml/legacymoduleref", legacymoduleref); // used to link DataRef on Satellite modules.
                    legacymoduleid = ms.ModuleId.ToString();
                    ms.SetXmlProperty("genxml/legacymoduleid", legacymoduleid);
                    if (ms.GetXmlProperty("genxml/settings/name") != "" && legacymoduleid != "")
                    {
                        ms.SetXmlProperty("genxml/settings/name", ms.GetXmlProperty("genxml/settings/name").Replace(legacymoduleid, moduleId.ToString()));
                    }
                    else
                    {
                        LogUtils.LogSystem("ERROR IMPORTDATA: ms.GetXmlProperty(\"genxml/settings/name\"):" + ms.GetXmlProperty("genxml/settings/name") + " legacymoduleid:" + legacymoduleid);
                    }
                    ms.PortalId = portalId;
                    ms.ModuleId = moduleId;
                    ms.GUIDKey = moduleRef;
                    ms.SetXmlProperty("genxml/data/tabid", tabId.ToString());
                    objCtrl.Update(ms);
                }
            }

        }


    }
}

