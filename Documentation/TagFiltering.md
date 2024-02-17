# Tag Filters

Tag filters are used to select only articles with those properties attached.  

The templates to deal with tag filters can use special razor token to help build a standard tag filter system.  

**Token to add a tag button.**
```
@TagButton(p.Key, p.Value, "rocket-tagbuttonOff", "rocket-tagbuttonOn",sessionParams)
```

**Token to inject the required JS for the tag system.**
```
@TagJsApiCall(moduleData.SystemKey,"#rocket-blog", "rocket-tagbuttonOff", "rocket-tagbuttonOn", sessionParams)
```

*Example of tag button on the website view*
```
@inherits RocketDirectoryAPI.Components.RocketDirectoryAPITokens<Simplisity.SimplisityRazor>
@using DNNrocketAPI.Components;
@using RocketDirectoryAPI.Components;
@AssigDataModel(Model)
<!--inject-->
@{
    var displayList = new List<int>();
}

<div class="rocket-tags">
    <ul class="">
    @foreach (var g in moduleData.GetPropertyModuleGroups(catalogSettings))
    {
        foreach (var p in propertyDataList.GetPropertyTagList(g.Key))
        {
            if (!displayList.Contains(p.Key))
            {
                <li>
                    @TagButton(p.Key, p.Value ,sessionParams)
                </li>
                displayList.Add(p.Key);
            }
        }
    }
    </ul>
    <div class="">@TagButtonClear("Effacer", sessionParams)</div>
</div>

@TagJsApiCall(moduleData.SystemKey, "#rocketlistcontainer", sessionParams)
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
This enables the tag filters to be selected from the module settings.  

*Exanple of dependancy file*
```
<moduletemplates list="true">
	<genxml>
		<file>Tags.cshtml</file>
		<name>Tag Filters</name>
	</genxml>
</moduletemplates>
```
