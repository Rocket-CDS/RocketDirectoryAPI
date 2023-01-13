## Handlebars Helpers for RocketDirectoryAPI System

The RocketDirectoryAPI system implements some special handlebars helpers to make creating templates easier.

### general session

**{{displayData this}}** - This helper is used to display json string, this is helpful in understanding what data you are using.  
```
{{engineurl @root}}
{{culturecode @root}}
{{culturecodeedit @root}}
```

### article

The "article" helper will read data from the article being rendered.  
```
{{article @root "<xpath>" "<displaytype>"}}  
{{article this "detailurl"}}
```


Example:
```
{{article this "ref"}}
{{article this "name"}}
{{article this "summary"}}
{{article this "defaultproperty"}}
{{article this "defaultcategory"}}
{{{article @root "genxml/textbox/articlesummary2" "breakof"}}}
{{article @root "genxml/textbox/price" "money" "fr-FR"}}
``` 

**Display Type \<displaytype>**  
The display type parameter formats the output.   
**"breakof"** =  NewLine to html \<br/>. (Use triple bracket to format html {{{....}}} )  
**"money"** = display format for money.  (Optional: Use extra param of "culturecode")

### article test

```
{{#articletest @root "haslinks"}}
        <h1>Has Links</h1>
{{/articletest}}
{{#articletest @root "hasdocs"}}
        <h1>Has Docs</h1>
{{/articletest}}
{{#articletest @root "hasimages"}}
        <h1>Has Images</h1>
{{/articletest}}
{{#articletest @root "ishidden"}}
        <h1>Is Hidden</h1>
{{/articletest}}
{{#articletest @root "haslist" "listname"}}
        <h1>Has List</h1>
{{/articletest}}
```


### image
**Images in a Loop**  
Each article can have a list of images.  To access those images from handlebars you can use a #each loop.  

```
{{#each genxml.data.genxml.imagelist.genxml}}

    {{#imagetest @root "ishidden" @index}}
        <h1>Is Hidden</h1>
    {{/imagetest}}
    {{#imagetest @root "isshown" @index}}
        <h1>Is Shown</h1>
    {{/imagetest}}
    {{#imagetest @root "hasheading" @index}}
        <h1>hasheading</h1>
    {{/imagetest}}
    {{#imagetest @root "hasimage" @index}}
        <h1>hasimage</h1>
    {{/imagetest}}
    {{#imagetest @root "haslink" @index}}
        <h1>haslink</h1>
    {{/imagetest}}
    {{#imagetest @root "hassummary" @index}}
        <h1>hassummary</h1>
    {{/imagetest}}


    {{image @root "thumburl" 0 640 200}}
    {{image @root "alt" @index}}
    {{image @root "summary" @index}}
    {{image @root "relpath" @index}}
    {{image @root "height" @index}}
    {{image @root "width" @index}}
    {{image @root "url" @index}}
    {{image @root "urltext" @index}}
    {{image @root "fieldid" @index}}
    {{image @root "count"}}
    {{image @root "genxml/textbox/mytextbox" @index}}

{{/each}}
```
NOTE: There is a bug in handlebars.js.  If the "@../index" is used within a loop it only works for the @first, after that the @index seems to be passed.
### doc
```
{{#each genxml.data.genxml.documentlist.genxml}}

    {{#doctest @root "ishidden" @index}}
        <h1>Is Hidden</h1>
    {{/doctest}}
    {{#doctest @root "isshown" @index}}
        <h1>Is Shown</h1>
    {{/doctest}}

    {{document @root "key" @index}}
    {{document @root "name" @index}}
    {{document @root "hidden" @index}}
    {{document @root "url" @index}}
    {{document @root "relpath" @index}}
    {{document @root "fieldid" @index}}
    {{document @root "count"}}
    {{document @root "genxml/textbox/mytextbox" @index}}

{{/each}}
```
### link
```
{{#each genxml.data.genxml.linklist.genxml}}

    {{#linktest @root "ishidden" @index}}
        <h1>Is Hidden</h1>
    {{/linktest}}
    {{#linktest @root "isshown" @index}}
        <h1>Is Shown</h1>
    {{/linktest}}

    {{link @root "key" @index 640 200}}
    {{link @root "name" @index 640 200}}
    {{link @root "hidden" @index 640 200}}
    {{link @root "fieldid" @index}}
    {{link @root "count"}}
    {{link @root "ref" @index}}
    {{link @root "type" @index}}
    {{link @root "target" @index}}
    {{link @root "anchor" @index}}
    {{link @root "url" @index}}
    {{link @root "genxml/textbox/mytextbox" @index}}

{{/each}}
```

## Settings

Settings that are added to the module via the "RemoteSettings.cshtml" can be accessed by using root helper.
```
<div class="w3-third w3-padding">
    <label>@ResourceKey("DNNrocket.height") (px)</label>
    @TextBox(info, "genxml/settings/height", " class='w3-input w3-border'", "200")
</div>
```
Is accesed in handlebars by:
```
{{@root.genxml.remotemodule.genxml.settings.height}}
```

```
<img class="sliderind{{moduleref @root}}" src="{{image @root "thumburl" @index @root.genxml.remotemodule.genxml.settings.width @root.genxml.remotemodule.genxml.settings.height}}" style="width: 100%;">
```

#### imageresize
The resize of any image is a setting.  This can be in the razor template as a s-field called "imageresize".
```
<input id="imagefileupload" class="simplisity_base64upload" s-reload="false" s-cmd="remote_addimage" s-post="#a-articledata" s-list=".@articleData.ImageListName,.@articleData.DocumentListName,.@articleData.LinkListName" s-fields='{"imageresize":"640","dataref":"@(articleData.DataRef)","moduleref":"@remoteModule.ModuleRef"}' type="file" name="file[]" multiple style="display:none;">
```
If no imageresize value exists on the upload filed the module settings field called "imageresize" is used.
```
<div class="w3-third w3-padding">
    <label>@ResourceKey("DNNrocket.imageresize") (px)</label>
    @TextBox(info, "genxml/settings/imageresize", " class='w3-input w3-border'", "1400")
</div>
```

The default resize is 640px.

## when (test) helper

```
Test 2 values with select operator.

{{#when <operand1> 'eq' <operand2>}}
    do something here
{{/when}}


eq:     ==
noteq:  !=
gt:     >
gteq:   >=
lt:     <
lteq:   <=
```

## resourcekey helper
```
{{resourcekey this, file.key, language = '', extension = 'Text'}}
```
In the razor template that calls the handlebars templates (Usually view.cshtml) you must have defined the rexlist.  The resxlist passes the paths for resx files to handlebars. 
```
AddProcessData("resourcepath", "/DesktopModules/DNNrocket/api/App_LocalResources/");
AddProcessData("resourcepath", "/DesktopModules/DNNrocketModules/RocketContent/App_LocalResources/");
AddProcessData("resourcepath", "/DesktopModules/RocketThemes/" + articleData.ProjectName + "/" + articleData.AdminAppThemeFolder+ "/" + articleData.AdminAppThemeFolderVersion  + "/resx");
    
hbsDict.Add("resxlist", RenderRazorUtils.GetResxPaths(Processdata));
```

Example:
```
{{resourcekey @root "Theme.tel" "" "Text"}}:{{textbox.tel}}
```

#### IsInCategory \<articleId> \<categoryRef>  
*Warning: This can be slow, try and avoid multiple calls.*
```
{{#IsInCategory genxml.data.genxml.column.itemid "ref1"}}
    <h1>You're in it.</h1>
{{else}}
    <h1>You're NOT in it.</h1>
{{/IsInCategory}}
```
#### HasProperty \<articleId> \<propertyRef>  
*Warning: This can be slow, try and avoid multiple calls.*
```
{{#HasProperty genxml.data.genxml.column.itemid "111"}}
    <h1>You're in Property. </h1>
{{else}}
    <h1>You're NOT in Property.</h1>
{{/HasProperty}}
```
