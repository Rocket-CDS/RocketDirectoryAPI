# Property Filters

Property filters are used to refine search criteria to only articles with those properties attached.  

The templates to deal with property filters can use special razor token to help build a standard filter system.  

**Token to add a filter checkbox.**
```
@FilterCheckBox(p.Key, p.Value, "#rocket-blog", sessionParams.Info.GetXmlPropertyBool("r/" + p.Key))
```
same as using a CheckBox like this..
```
@CheckBox(infoempty, "genxml/checkbox/" + p.Key, "&nbsp;" + p.Value, " class='simplisity_sessionfield'  onchange='simplisity_setSessionField(this.id, this.checked);callArticleList(\"#rocket-blog\");' ", sessionParams.Info.GetXmlPropertyBool("r/" + p.Key))
```

**Token to inject the required JS for the filter system.**
```
@FilterJsApiCall(moduleData.SystemKey, sessionParams)
```

*Example of checkbox filters on the website view*
```
@inherits RocketDirectoryAPI.Components.RocketDirectoryAPITokens<Simplisity.SimplisityRazor>
@using DNNrocketAPI.Components;
@using RocketDirectoryAPI.Components;
@AssigDataModel(Model)
<!--inject-->
<div class="rocket-filters">
    @foreach (var g in moduleData.GetPropertyModuleGroups(catalogSettings))
    {
        <div class="rocket-filtergroup">@g.Value</div>
        foreach (var p in propertyDataList.GetPropertyFilterList(g.Key))
        {
            <div class="rocket-filterlist">
                @FilterCheckBox(p.Key, p.Value, "#rocket-blog", sessionParams.Info.GetXmlPropertyBool("r/" + p.Key))
            </div>
        }
    }
</div>
@FilterJsApiCall(moduleData.SystemKey, sessionParams)
```

Create property group checkbox in module settings. "ThemeSettings.cshtml" to select which property groups should be included on the website view.

```
@FilterGroupCheckBox(groupId, groupName)
```

Example:
```
<div class="w3-row  w3-padding">
    <div class="w3-row">
        <div class='w3-row w3-padding'>
            <div class="w3-large w3-margin-bottom">@ResourceKey("RC.propertygroups")</div>
            @foreach (var p in groupDict)
            {
                <span class="w3-padding">
                    @FilterGroupCheckBox(p.Key, p.Value)
                </span>
            }
        </div>
    </div>
</div>
```

## Module Template Definition
A module template needs to be created on the AppTheme with an entry in the dependancies files.  
This enables the property filters to be selected from the module settings.  

*Exanple of dependancy file*
```
<moduletemplates list="true">
	<genxml>
		<file>Filters.cshtml</file>
		<name>Property Filters</name>
	</genxml>
</moduletemplates>
```
