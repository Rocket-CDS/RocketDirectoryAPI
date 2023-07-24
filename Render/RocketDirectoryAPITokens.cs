using DNNrocketAPI.Components;
using Newtonsoft.Json.Linq;
using RazorEngine.Text;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RocketDirectoryAPI.Components
{
    public class RocketDirectoryAPITokens<T> : DNNrocketAPI.render.DNNrocketTokens<T>
    {

        public IEncodedString TextBoxMoney(string cultureCode, SimplisityInfo info, String xpath, String attributes = "", String defaultValue = "", bool localized = false, int row = 0, string listname = "", string type = "text")
        {
            if (info == null) info = new SimplisityInfo();
            var value = info.GetXmlPropertyInt(xpath);
            if (localized && !xpath.StartsWith("genxml/lang/"))
            {
                value = info.GetXmlPropertyInt("genxml/lang/" + xpath);
            }
            var upd = getUpdateAttr(xpath, attributes, localized);
            var id = getIdFromXpath(xpath, row, listname);

            // [TODO: add encrypt option.]
            //var value = encrypted ? NBrightCore.common.Security.Decrypt(PortalController.Instance.GetCurrentPortalSettings().GUID.ToString(), info.GetXmlProperty(xpath)) : info.GetXmlProperty(xpath);
            if (value == 0 && GeneralUtils.IsNumeric(defaultValue)) value = Convert.ToInt32(defaultValue);

            var typeattr = "type='" + type + "'";
            if (attributes.ToLower().Contains(" type=")) typeattr = "";

            var strOut = "<input value='" + CurrencyUtils.CurrencyEdit(value, cultureCode) + "' id='" + id + "' s-datatype='int' s-xpath='" + xpath + "' " + attributes + " " + upd + " " + typeattr + " />";
            return new RawString(strOut);
        }
        public IEncodedString InterfaceNameResourceKey(RocketInterface rocketInterface, SystemLimpet systemData, String lang = "", string resxFileName = "SideMenu")
        {
            if (lang == "") lang = DNNrocketUtils.GetCurrentCulture();
            var interfaceName = DNNrocketUtils.GetResourceString(systemData.SystemRelPath + "/App_LocalResources", resxFileName + "." + rocketInterface.InterfaceKey, "Text", lang);
            if (interfaceName == "")
            {
                interfaceName = DNNrocketUtils.GetResourceString(rocketInterface.TemplateRelPath + "/App_LocalResources", resxFileName + "." + rocketInterface.InterfaceKey, "Text", lang);
            }
            if (interfaceName == "")
            {
                interfaceName = rocketInterface.InterfaceKey;
            }
            return new RawString(interfaceName);
        }

    }
}
