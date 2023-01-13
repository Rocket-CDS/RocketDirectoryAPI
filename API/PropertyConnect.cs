using DNNrocketAPI.Components;
using RocketDirectoryAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RocketDirectoryAPI.API
{    
    public partial class StartConnect
    {
        private PropertyLimpet GetActiveProperty(int propertyid)
        {
            return new PropertyLimpet(_portalCatalog.PortalId, propertyid, _sessionParams.CultureCodeEdit, _systemData.SystemKey);
        }
        public String GetProperty(int propertyId)
        {
            var razorTempl = GetSystemTemplate("propertydetail.cshtml");
            var propertyData = GetActiveProperty(propertyId);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, propertyData, _dataObjects, _passSettings, _sessionParams, true);
            if (pr.ErrorMsg != "") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public string GetPropertyList()
        {
            var propertyDataList = new PropertyLimpetList(PortalUtils.GetCurrentPortalId(), _sessionParams, _sessionParams.CultureCodeEdit, _systemData.SystemKey);
            var razorTempl = GetSystemTemplate("PropertyList.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, propertyDataList, _dataObjects, _passSettings, _sessionParams, true);
            if (pr.ErrorMsg != "") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String AddProperty()
        {
            var parentid = _paramInfo.GetXmlPropertyInt("genxml/hidden/parentid");
            var razorTempl = GetSystemTemplate("PropertyDetail.cshtml");
            var propertyData = GetActiveProperty(-1);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, propertyData, _dataObjects, _passSettings, _sessionParams, true);
            if (pr.ErrorMsg != "") return pr.ErrorMsg;
            return pr.RenderedText;
        }

        public string SaveProperty()
        {
            var propertyId = _paramInfo.GetXmlPropertyInt("genxml/hidden/propertyid");
            _passSettings.Add("saved", "true");
            var propertyData = GetActiveProperty(propertyId);
            var r = propertyData.Save(_postInfo);
            if ( r == -1) _passSettings.Add("duplicateref", "true");
            return GetProperty(r);
        }
        public string DeleteProperty()
        {
            var propertyid = _paramInfo.GetXmlPropertyInt("genxml/hidden/propertyid");
            if (propertyid > 0)
            {
                var propertyData = GetActiveProperty(propertyid);
                propertyData.Delete();
            }
            return GetPropertyList();
        }
        public string RemovePropertyArticle()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var propertyId = _paramInfo.GetXmlPropertyInt("genxml/hidden/propertyid");
            var articleData = GetActiveArticle(articleId);
            articleData.RemoveProperty(propertyId);
            return GetProperty(propertyId);
        }

    }
}

