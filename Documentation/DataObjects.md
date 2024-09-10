# Razor View Data Objects
The razor templates can use a defined set of data objects.  Some are auomatically add on every call, some are only injected if required.  

## Standard Razor View Data Objects

    appTheme = (AppThemeLimpet)sModel.GetDataObject("apptheme");
    appThemeDefault = (AppThemeLimpet)sModel.GetDataObject("appthemedefault");
    appThemeSystem = (AppThemeSystemLimpet)sModel.GetDataObject("appthemesystem");
    appThemeDirectory = (AppThemeSystemLimpet)sModel.GetDataObject("appthemedirectory");
    appThemeDirectoryDefault = (AppThemeLimpet)sModel.GetDataObject("appthemedirectorydefault");
    appThemeProjects = (AppThemeProjectLimpet)sModel.GetDataObject("appthemeprojects");
    appThemeDataList = (AppThemeDataList)sModel.GetDataObject("appthemedatalist");
    portalContent = (PortalCatalogLimpet)sModel.GetDataObject("portalcontent");
    systemData = (SystemLimpet)sModel.GetDataObject("systemdata");
    systemDirectoryData = (SystemLimpet)sModel.GetDataObject("systemdirectorydata");
    portalData = (PortalLimpet)sModel.GetDataObject("portaldata");
    catalogSettings = (CatalogSettingsLimpet)sModel.GetDataObject("catalogsettings");
    moduleData = (ModuleContentLimpet)sModel.GetDataObject("modulesettings");
    moduleDataInfo = new SimplisityInfo(moduleData.Record);
    categoryDataList = (CategoryLimpetList)sModel.GetDataObject("categorylist");
    propertyDataList = (PropertyLimpetList)sModel.GetDataObject("propertylist");
    propertyData = (PropertyLimpet)sModel.GetDataObject("propertydata");
    defaultData = (DefaultsLimpet)sModel.GetDataObject("defaultdata");
    globalSettings = (SystemGlobalData)sModel.GetDataObject("globalsettings");
    dashBoard = (DashboardLimpet)sModel.GetDataObject("dashboard");
    articleData = (ArticleLimpet)sModel.GetDataObject("articledata");
    articleDataList = (ArticleLimpetList)sModel.GetDataObject("articlelist");
    sessionParams = sModel.SessionParamsData;
    userParams = (UserParams)sModel.GetDataObject("userparams");

    info = articleData.Info;
    infoempty = new SimplisityInfo();

## Injected Razor View Data Objects

    articleDataList = (ArticleLimpetList)Model.GetDataObject("articlelist");
    articleData = (ArticleLimpet)Model.GetDataObject("articledata");
    articleCategoryData = (CategoryLimpet)sModel.GetDataObject("articlecategorydata");
    categoryData = (CategoryLimpet)Model.GetDataObject("categorydata");
