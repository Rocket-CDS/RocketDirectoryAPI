using DNNrocketAPI.Components;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace RocketDirectoryAPI.Components
{
    public class ModuleContentLimpet : ModuleBase
    {
        public ModuleContentLimpet(int portalId, string moduleRef, int moduleid = -1, int tabid = -1) : base(portalId, moduleRef, moduleid, tabid)
        {             
        }
        public int DefaultCategoryId { get { return GetSettingInt("defaultcategory"); } }
    }
}
