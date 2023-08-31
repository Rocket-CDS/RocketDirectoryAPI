using DNNrocketAPI;
using DNNrocketAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RocketDirectoryAPI.Components
{
    public class DashboardLimpet
    {
        public const string _tableName = "RocketDirectoryAPI";
        private const string _entityTypeCode = "DASHBOARD";
        private const string _systemkey = "rocketdirectoryapi";
        private DNNrocketController _objCtrl;
        private string _guidKey;
        private string _cacheKey;
        private int _portalId;
        public DashboardLimpet(int portalId, string cultureCode)
        {
            Info = new SimplisityInfo();
            if (cultureCode == "") cultureCode = DNNrocketUtils.GetEditCulture();
            CultureCode = cultureCode;
            _portalId = portalId;
            _guidKey =  _entityTypeCode + portalId;
            _objCtrl = new DNNrocketController();

            _cacheKey = portalId + "*" + _entityTypeCode + "*" + _guidKey + "*" + _tableName;

            var uInfo = (SimplisityInfo)CacheUtils.GetCache(_cacheKey, "portal" + portalId);
            if (uInfo == null)
            {
                uInfo = _objCtrl.GetByGuidKey(portalId, -1, _entityTypeCode, _guidKey, "", _tableName);
                if (uInfo != null) Info = _objCtrl.GetInfo(uInfo.ItemID, CultureCode, _tableName);
                if (Info == null || Info.ItemID <= 0)
                {
                    Info = new SimplisityInfo();
                    Info.PortalId = _portalId;
                    Info.ModuleId = -1;
                    Info.TypeCode = _entityTypeCode;
                    Info.GUIDKey = _guidKey;
                    Info.Lang = CultureCode;
                }
                else
                {
                    CacheUtils.SetCache(_cacheKey, Info, "portal" + Info.PortalId);
                }
            }

        }

        private void ReplaceInfoFields(SimplisityInfo postInfo, string xpathListSelect)
        {
            var textList = Info.XMLDoc.SelectNodes(xpathListSelect);
            if (textList != null)
            {
                foreach (XmlNode nod in textList)
                {
                    Info.RemoveXmlNode(xpathListSelect.Replace("*", "") + nod.Name);
                }
            }
            textList = postInfo.XMLDoc.SelectNodes(xpathListSelect);
            if (textList != null)
            {
                foreach (XmlNode nod in textList)
                {
                    Info.SetXmlProperty(xpathListSelect.Replace("*", "") + nod.Name, nod.InnerText);
                }
            }
        }

        public void Save(SimplisityInfo postInfo)
        {
            ReplaceInfoFields(postInfo, "genxml/textbox/*");
            ReplaceInfoFields(postInfo, "genxml/lang/genxml/textbox/*");
            ReplaceInfoFields(postInfo, "genxml/checkbox/*");
            ReplaceInfoFields(postInfo, "genxml/lang/genxml/checkbox/*");
            ReplaceInfoFields(postInfo, "genxml/select/*");
            ReplaceInfoFields(postInfo, "genxml/lang/genxml/select/*");
            ReplaceInfoFields(postInfo, "genxml/radio/*");
            ReplaceInfoFields(postInfo, "genxml/lang/genxml/radio/*");
            Update();
        }
        public void Update()
        {
            _objCtrl.SaveData(Info, _tableName);
            ClearCache();
        }
        public void Delete()
        {
            _objCtrl.Delete(Info.ItemID, _tableName);
            ClearCache();
        }
        public void ClearCache()
        {
            CacheUtils.RemoveCache(_cacheKey, "portal" + _portalId);
        }

        public string EntityTypeCode { get { return _entityTypeCode; } }


        #region "Info - Dashboard Data"
        public SimplisityInfo Info { get; set; }
        public int PortalId { get { return Info.PortalId; } }
        public string CultureCode { get; set; }
        public string SystemKey { get { return "rocketdirectoryapi"; } }
        
        #endregion

    }
}
