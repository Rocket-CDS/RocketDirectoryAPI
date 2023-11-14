# Category Menu
A RocketDirectory module can display a category menu.  The correct module templates need to be setup and selected from the module settings.  

A URL query parameter of "catid" is used to identify the category that shoudl be displayed.  

## Module Template Definition
A module template needs to be created on the AppTheme with an entry in the dependancies files.  
This enables the category menu to be selected from the module settings.  

*Exanple of dependancy file*
```
<moduletemplates list="true">
	<genxml>
		<file><![CDATA[Categories.cshtml]]></file>
		<name><![CDATA[Category Menu]]></name>
	</genxml>
</moduletemplates>
```

## Category Menu Template
In the "default" sub-folder of the AppTheme the above template needs to be created.  
*Example: Categories.cshtml*
```
@inherits RocketDirectoryAPI.Components.RocketDirectoryAPITokens<Simplisity.SimplisityRazor>
@using DNNrocketAPI.Components;
@using RocketDirectoryAPI.Components;
@AssigDataModel(Model)
@AddProcessDataResx(appTheme, true)
@AddProcessData("resourcepath", "/DesktopModules/DNNrocketModules/RocketDirectoryAPI/App_LocalResources/")
<!--inject-->

<div class="categories">

    <h4>@ResourceKey("DNNrocket.categories") :</h4>

    <div class="w3-bar-block w3-light-grey">
        <a href="@DNNrocketUtils.NavigateURL(sessionParams.TabId)" class="w3-bar-item w3-button">@ResourceKey("DNNrocket.all")</a>

    @foreach (var c in categoryDataList.GetCategoryTree())
    {
        if (!c.Hidden && !c.HiddenByCulture && c.Level == 0)
        {
            if (c.Level == 0)
            {
                var l = c.GetDirectChildren();
                var catDict = new Dictionary<string, string>();
                catDict.Add("catid", c.CategoryId.ToString());
                if (l.Count == 0)
                {
                    <a href="@PagesUtils.NavigateURL(sessionParams.TabId, catDict, c.Name)" class="w3-bar-item w3-button" onclick="$('.simplisity_loader').show()">@c.Name</a>
                }
                else
                {
                    <a href="@PagesUtils.NavigateURL(sessionParams.TabId, catDict, c.Name)" class="w3-bar-item w3-button" onclick="$('.simplisity_loader').show()">@c.Name</a>
                    <div class="w3-margin-left">
                        @foreach (var child in l)
                        {
                            var catDict2 = new Dictionary<string, string>();
                            catDict2.Add("catid", child.CategoryId.ToString());
                                <a href="@PagesUtils.NavigateURL(sessionParams.TabId, catDict2, child.Name)" class="w3-bar-item w3-button" onclick="$('.simplisity_loader').show()">@child.Name</a>
                        }
                    </div>
                }
            }
        }
    }
    </div>
</div>
```




