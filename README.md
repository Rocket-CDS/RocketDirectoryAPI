# RocketDirectoryAPI

## Standard Razor View Data Objects

    var sessionParams = (SessionParams)Model.SessionParamsData;
    var appThemeSystem = (AppThemeSystemLimpet)Model.GetDataObject("appthemesystem");
    var appThemeDirectory = (AppThemeSystemLimpet)Model.GetDataObject("appthemedirectory");
    var portalData = (PortalLimpet)Model.GetDataObject("portaldata");
    var systemData = (SystemLimpet)Model.GetDataObject("systemdata");
    var portalContent = (PortalCatalogLimpet)Model.GetDataObject("portalcontent");
    var appThemeProjects = (AppThemeProjectLimpet)Model.GetDataObject("appthemeprojects");
    var defaultData = (DefaultsLimpet)Model.GetDataObject("defaultdata");
    var moduleSettings = (ModuleContentLimpet)Model.GetDataObject("modulesettings");
    var globalSettings = (SystemGlobalData)Model.GetDataObject("globalsettings");
    var appThemeDefault = (AppThemeLimpet)Model.GetDataObject("appthemedefault");
    var appThemeView = (AppThemeLimpet)Model.GetDataObject("appthemeview");
    var appThemeAdmin = (AppThemeLimpet)Model.GetDataObject("appthemeadmin");
    var appThemeDataListView = (AppThemeDataList)Model.GetDataObject("appthemedatalistview");
    var appThemeDataListAdmin = (AppThemeDataList)Model.GetDataObject("appthemedatalistadmin");
    var catalogSettings = (CatalogSettingsLimpet)Model.GetDataObject("catalogsettings");
    var catalogList = (CategoryLimpetList)Model.GetDataObject("categorylist");
    var propertyList = (PropertyLimpetList)Model.GetDataObject("propertylist");
    var dashBoard = (DashboardLimpet)Model.GetDataObject("dashboard");

## Injected Razor View Data Objects

    var articleDataList = (ArticleLimpetList)Model.GetDataObject("articlelist");
    var articleData = (ArticleLimpet)Model.GetDataObject("articledata");
    var categoryData = (CategoryLimpet)Model.GetDataObject("categorydata");

