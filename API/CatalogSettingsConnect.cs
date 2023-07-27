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

        public string SaveCatalogSettings()
        {
            _dataObject.CatalogSettings.Save(_postInfo);
            return GetCatalogSettings();
        }
        public string AddPropertyGroup()
        {
            _dataObject.CatalogSettings.Save(_postInfo);
            _dataObject.CatalogSettings.AddGroup(new SimplisityInfo());
            return GetCatalogSettings();
        }

        public string GetCatalogSettings()
        {
            var razorTempl = GetSystemTemplate("CatalogSettings.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.CatalogSettings, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.ErrorMsg != "") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public string DeleteCatalogSettings()
        {
            _dataObject.CatalogSettings.Delete();
            return GetCatalogSettings();
        }




    }
}

