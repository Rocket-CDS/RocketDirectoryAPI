using DNNrocketAPI.Components;
using Rocket.AppThemes.Components;
using RocketDirectoryAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace RocketDirectoryAPI.API
{    
    public partial class StartConnect
    {

        public string SaveCatalogSettings()
        {
            _catalogSettings.Save(_postInfo);
            return GetCatalogSettings();
        }

        public string GetCatalogSettings()
        {
            var razorTempl = GetSystemTemplate("CatalogSettings.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _catalogSettings, _dataObjects, _passSettings, _sessionParams, true);
            if (pr.ErrorMsg != "") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public string DeleteCatalogSettings()
        {
            _catalogSettings.Delete();
            return GetCatalogSettings();
        }




    }
}

