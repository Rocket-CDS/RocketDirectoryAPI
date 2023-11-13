# Property Filters





```
@FilterCheckBox(p.Key, p.Value, "#rocket-blog", sessionParams.Info.GetXmlPropertyBool("r/" + p.Key))
```
same as using a CheckBox like this..
```
@CheckBox(infoempty, "genxml/checkbox/" + p.Key, "&nbsp;" + p.Value, " class='simplisity_sessionfield'  onchange='simplisity_setSessionField(this.id, this.checked);callArticleList(\"#rocket-blog\");' ", sessionParams.Info.GetXmlPropertyBool("r/" + p.Key))
```

Token to build the required JS for the filter system.
```
@FilterJsApiCall(moduleData.SystemKey, sessionParams)
```

Example of checkbox filters
```
@inherits RocketDirectoryAPI.Components.RocketDirectoryAPITokens<Simplisity.SimplisityRazor>
@using DNNrocketAPI.Components;
@using RocketDirectoryAPI.Components;
@AssigDataModel(Model)
@AddProcessDataResx(appTheme, true)
@AddProcessData("resourcepath", "/DesktopModules/DNNrocketModules/RocketDirectoryAPI/App_LocalResources/")
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

Create property checkbox in module settings. "ThemeSettings.cshtml".

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