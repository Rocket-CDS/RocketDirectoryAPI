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
            _dataObject.PortalContent.ResetSetting();
            _dataObject.PortalContent.ProjectName = moduleData.ProjectName;
            _dataObject.PortalContent.AppThemeFolder = moduleData.AppThemeAdminFolder;
            _dataObject.PortalContent.AppThemeVersion = moduleData.AppThemeAdminVersion;
            _dataObject.PortalContent.Update();

            return RenderSystemTemplate("ModuleSettings.cshtml");
        }

    }
}

