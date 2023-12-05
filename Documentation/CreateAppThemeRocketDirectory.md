**Create a Folders**

```plaintext
/DesktopModules/RocketThemes/AppThemes-W3-CSS/rocketdirectoryapi.example1
```

And create version sub-folder.

```plaintext
/DesktopModules/RocketThemes/AppThemes-W3-CSS/rocketdirectoryapi.example1/1.0
```

Create a "Default" sub-folder

```plaintext
/DesktopModules/RocketThemes/AppThemes-W3-CSS/rocketdirectoryapi.example1/1.0/default
```
There are a number of razor templates required for an AppTheme.    
The AppTheme included both Admin templates and the view (website display) templates.  Standard names and structures are required.

NOTE: All admin templates use the w3.css framework, which is automatically added to the page by the rocketdirectoryapi system.  
[https://www.w3schools.com/w3css/](https://www.w3schools.com/w3css/)  

#### RocketDirectory is what we call a base system, that means that other systems can be built using the RocketDirectory
Systems that use or "inherit" the RocketDirectory system are called wrapper systems and will have a different name to the RocketDirectory system. 
The different name allows mulitple RocketDirectory systems to run on the same website.  You can build an AppTheme for any wrapper system in the same way, although some wrapper systems may have extra functionlaity.   

#### **Step 2 -  Default Razor Templates**

A RocketDirectory AppTheme is a "List and Detail" structure.  Which means that you have a page for a list of "articles" and also a possiible detail page for each article.  

The easiest way to build an AppTheme is to copy an existing one that does something close to what you need.  Here we will build a simple list and detail AppTheme to show how it works.  


Create a file called "**AdminList.cshtml**" with this content...

```
@inherits RocketDirectoryAPI.Components.RocketDirectoryAPITokens<Simplisity.SimplisityRazor>
@using DNNrocketAPI.Components;
@using RocketDirectoryAPI.Components;
@AssigDataModel(Model)
@AddProcessDataResx(appTheme, true)
<!--inject-->

@RenderLanguageSelector("articleadmin_editlist", new Dictionary<string, string>(), appThemeDirectory, Model)

<div class='w3-animate-opacity'>

    [INJECT:appthemedirectory,AdminSearch.cshtml]

    <div id="datalistsection" class="w3-padding">

        <table class="w3-table w3-bordered w3-hoverable">
            <thead>
                <tr>
                    <th></th>
                    <th>@ResourceKey("RE.name")</th>
                </tr>
            </thead>
            @foreach (ArticleLimpet articleData in articleDataList.GetArticleList())
            {
                    <tr class=" simplisity_click" s-cmd="articleadmin_editarticle" s-fields='{"articleid":"@articleData.ArticleId","track":"true"}' style="cursor:pointer;">
                        <td style="width:54px;">
                            <img src="@ImageUrl(articleData.GetImage(0).RelPath, 48, 48)" style="height:48px;width:48px;" class="w3-round" />
                        </td>
                        <td><div class="w3-tiny">@articleData.Ref</div><b>@articleData.Name</b></td>
                    </tr>
            }
        </table>

        @RenderTemplate("AdminPaging.cshtml", appThemeDirectory, Model, true)

    </div>

</div>

```

The "AdminList.cshtml" template is the default Admin List template.  
This example show a list with a language selector at the top, a search banner, rows with an image at the front and paging options.  


Create a file called "**AdminDetail.cshtml**" with this content...

```
@inherits RocketDirectoryAPI.Components.RocketDirectoryAPITokens<Simplisity.SimplisityRazor>
@using DNNrocketAPI.Components;
@using RocketDirectoryAPI.Components;
@AssigDataModel(Model)
@AddProcessDataResx(appTheme, true)
<!--inject-->
@{
    var sfieldDict = new Dictionary<string, string>();
    sfieldDict.Add("articleid", articleData.ArticleId.ToString());
    sfieldDict.Add("moduleedit", sessionParams.GetBool("moduleedit").ToString());
    sfieldDict.Add("tabid", sessionParams.TabId.ToString());
}
@RenderLanguageSelector("articleadmin_editarticle", sfieldDict, appThemeDirectory, Model)

<div id="articleedit">

    <div id="articleeditcontent" class='w3-animate-opacity '>

        [INJECT:appthemedirectory,AdminDetailButtons.cshtml]

        <div id="articledatasection" class="">
            <div class='w3-row'>
                <div class="w3-twothird w3-padding-small">
                    <div id="generaltab" class='w3-row sectionname w3-border'>
                        <div class='w3-row'>
                            <div class="w3-third">
                                <div class='w3-col w3-padding'>
                                    <label>@ResourceKey("RC.ref")</label>
                                    @TextBox(info, articleData.RefXPath, " class='w3-input w3-border' autocomplete='off' ", "", false, 0)
                                </div>
                            </div>
                            <div class="w3-third">
                                <div class='w3-col w3-padding'>
                                    <label>Statut</label>
                                    <div>
                                        @CheckBox(info, articleData.HiddenXPath, "&nbsp;" + ResourceKey("DNNrocket.hidden"), " class='w3-check' ")
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class=" w3-row w3-padding">
                            <label>@ResourceKey("RC.name")</label>&nbsp;@EditFlag(sessionParams)
                            @TextBox(info, articleData.NameXPath, " class='w3-input w3-border' autocomplete='off' ", "", true, 0)
                        </div>
                        <div class=" w3-row w3-padding">
                            <label>Résumé</label>&nbsp;@EditFlag(sessionParams)
                            @TextArea(info, articleData.SummaryXPath, " class='w3-input w3-border' rows='8' autocomplete='off' ", "", true, 0)
                        </div>
                    </div>

                </div>
                <div class="w3-third w3-padding-small">

                    <div onclick="$('#imgs').toggle();$('#imgsexpand').toggle();" class="w3-button w3-block w3-theme-l3 w3-left-align w3-margin-top">
                        @ResourceKey("DNNrocket.images")
                        <span id="imgsexpand" class="material-icons w3-right" style="display:none;">
                            unfold_more
                        </span>
                    </div>
                    <div id="imgs" class='w3-row sectionname w3-border w3-padding' style="">
                        @RenderTemplate("Articleimages.cshtml", appThemeDirectory, Model, true)
                    </div>
                    @RenderTemplate("ArticleimageSelect.cshtml", appThemeDirectory, Model, true)
                </div>
            </div>
        </div>

        <div class="w3-padding">
            <i>ID: </i> @info.ItemID <i> GuidKey:</i> @info.GUIDKey
        </div>

    </div>

</div>


```

The "AdminDetail.cshtml" template is the default admin detail template.  
We have added a selection of standard fields and an option to upload images.

Create a file called "**AdminHeader.cshtml**" with this content...

```
@inherits RocketDirectoryAPI.Components.RocketDirectoryAPITokens<Simplisity.SimplisityRazor>
<!--inject-->
```
The Admin Header template allows you to add extra functionality.  *This is a placeholder template, so we do not get a error display on admin.*  

Create a file called "**View.cshtml**" with this content...
```
@inherits RocketDirectoryAPI.Components.RocketDirectoryAPITokens<Simplisity.SimplisityRazor>
@using DNNrocketAPI.Components;
@using RocketDirectoryAPI.Components;
@AssigDataModel(Model)
@AddProcessDataResx(appTheme, true)
@AddProcessData("resourcepath", "/DesktopModules/DNNrocketModules/RocketDirectoryAPI/App_LocalResources/")
<!--inject-->

<div id="rocket-example1" class="rocket-example1-container">
    @{
        Model.SetSetting("searchview", "false");
        if (articleData != null)
        {
            @RenderTemplate("ArticleDetail.cshtml", appTheme, Model, false)
        }
        else
        {
            @RenderTemplate("ArticleList.cshtml", appTheme, Model, false)
        }
    }
</div>

<div id="simplisity_loader" class="loader-wrapper" style="display:none;">
    <div class="loader is-loading"></div>
</div>

```
The "view.cshtml" is the default template to display on the website, although the AppTheme settings can change this.  
This view tests to see if we want to display a list or the detail page, it then renders the required template.  "ArticleList.cshtml" or "ArticleDetail.cshtml".


Create a file called "**ArticleList.cshtml**" with this content...

```
@inherits RocketDirectoryAPI.Components.RocketDirectoryAPITokens<Simplisity.SimplisityRazor>
@using DNNrocketAPI.Components;
@using RocketDirectoryAPI.Components;
@AssigDataModel(Model)
@AddProcessDataResx(appTheme, true)
@AddProcessData("resourcepath", "/DesktopModules/DNNrocketModules/RocketDirectoryAPI/App_LocalResources/")
<!--inject-->
@{
    var articleList = articleDataList.GetArticleList();
}
<div class="articlewrapper">

[INJECT:appthemedirectorydefault,SearchBanner.cshtml]

<h1>Name: @categoryData.Name</h1>
    <div class="w3-row w3-padding">

        @foreach (ArticleLimpet articleData in articleList)
        {
            <div class="w3-col m3 w3-center">
                <a href="@DetailUrl(moduleData.DetailPageTabId(), articleData, categoryData)" title="@articleData.Name" onclick="$('#simplisity_loader').show();">
                    <img src="@ImageUrl(articleData.GetImage(0).RelPathWebp,120,120)" alt="@articleData.GetImage(0).Alt" />
                    <div class="w3-container">
                    <b>@articleData.Name</b>
                    </div>
                </a>
            </div>
        }

    </div>

    <div class="w3-row w3-padding-16 w3-center">
            @RenderTemplate("articlePaging.cshtml", appThemeDirectoryDefault, Model, false)
    </div>

</div>
```

Create a file called "**ArticleDetail.cshtml**" with this content...
```
@inherits RocketDirectoryAPI.Components.RocketDirectoryAPITokens<Simplisity.SimplisityRazor>
@using DNNrocketAPI.Components;
@using RocketDirectoryAPI.Components;
@AssigDataModel(Model)
@AddProcessDataResx(appTheme, true)
@AddProcessData("resourcepath", "/DesktopModules/DNNrocketModules/RocketDirectoryAPI/App_LocalResources/")
<!--inject-->
<div class="w3-container">

    <div class="w3-container">
        <a href="@ListUrl(moduleData.ListPageTabId(), categoryData)" class="w3-button w3-green">@ButtonIconText(ButtonTypes.back)</a>
    </div>

    <div class="articledetail-left">

        <h1>@articleData.Name</h1>

        <div>@BreakOf(articleData.Summary)</div>

        <div>
            <img src="@ImageUrl(articleData.GetImage(0).RelPathWebp,300,300)" alt="@articleData.GetImage(0).Alt" />
        </div>

    </div>

</div>
```

#### Other possible templates

**Categories.cshtml**  
See "Category Menu" in documentation

**Filters.cshtml**  
See "Property Filters" in documentation

**ThemeSettings.cshtml**  
This template is used to get user settings for the AppTheme.

#### AppTheme Dependancies

**Create a "dep" sub-folder**

```
/DesktopModules/RocketThemes/AppThemes-W3-CSS/rocketdirectoryapi.example1/1.0/dep
```

For this AppTheme we will have a startup templates, inject css dependancies, only have articles showing on the Admin Panel and a URL query paramters for the detail page.

Create a file called "example1.dep" in the "dep" folder.
```
<genxml>
	<deps list="true">
		<genxml>
			<ctrltype><![CDATA[css]]></ctrltype>
			<url><![CDATA[/DesktopModules/DNNrocket/css/w3.css]]></url>
		</genxml>
		<genxml>
			<ctrltype><![CDATA[css]]></ctrltype>
			<url><![CDATA[https://fonts.googleapis.com/icon?family=Material+Icons]]></url>
		</genxml>
	</deps>
	<moduletemplates list="true">
		<genxml>
			<file><![CDATA[view.cshtml]]></file>
			<name><![CDATA[List View]]></name>
		</genxml>
	</moduletemplates>
	<adminpanelinterfacekeys list="true">
		<genxml>
			<interfacekey>articleadmin</interfacekey>
			<show>true</show>
		</genxml>
		<genxml>
			<interfacekey>categoryadmin</interfacekey>
			<show>false</show>
		</genxml>
		<genxml>
			<interfacekey>propertyadmin</interfacekey>
			<show>false</show>
		</genxml>
		<genxml>
			<interfacekey>settingsadmin</interfacekey>
			<show>false</show>
		</genxml>
		<genxml>
			<interfacekey>rocketdirectoryadmin</interfacekey>
			<show>false</show>
		</genxml>
	</adminpanelinterfacekeys>
	<queryparams list="true">
		<genxml>
			<queryparam>articleid</queryparam>
			<tablename>rocketdirectoryapi</tablename>
		</genxml>
	</queryparams>
</genxml>
```

### QueryParams
With the directory system you may have a list and detail structure.  
The detail should contain SEO data in the header.  The SEO data is read by using a URL parameter, this paramater is defined in the dependacies file.  Saving the directory settings will also update the Page data so the Meta.ascx can capture the detail data with an ItemId.  


