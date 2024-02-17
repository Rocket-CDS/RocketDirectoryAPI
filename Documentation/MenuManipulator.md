# Menu Manipulator

Categoeries can be injected into the webiste menu to give a seemless navigation from the website to the directory categories.  

*(See RocketTools Page Localization installation instructions)*

## Automatically Add using the AppTheme

The AppTheme dependancy file can be used to define the Menu Provider for each system.
The menu provider is updated when the Admin Settings of the system are saved.

```
<menuprovider>
	<genxml>
		<assembly>RocketDirectoryAPI</assembly>
		<namespaceclass>RocketDirectoryAPI.Components.MenuDirectory</namespaceclass>
		<systemkey>rocketblogapi</systemkey>
	</genxml>
</menuprovider>
```

## Manually Add
Add the Node Manipulatore assembly and namespace to the RocketTools Page Localization settings.  

**Assembly**
```
RocketDirectoryAPI
```
**Namespace.Class**
```
RocketDirectoryAPI.Components.MenuDirectory
```
**SystemKey**
```
SystemKey of the required system
```

# DDR Menu Setup
Because of SEO requirements to only have 1 URL for each detail page a complex setup is sometimes rerquired.  
The URL of the canonical URL is taken from the "System Admin Settings" and it is this defined pag ethat will be used to create canonical and alternate links.  
Complex navigation often needs the detail pages to be access from the website is different ways.  One of which will be the canonical link.  

**The below instructions are an example of how the pages and menus can be setup with the "rocketdirectoryapi" system, but other methods and systems can be used.**  

## Create a List Page
We need a page to display the article list.  
This page should match the naming convension to linked the page to a category structure.  *(See Rocket Tools documentation)*  
Create the page on the root level of the website.  
*The page can be anywhere in the website structure, for this example we are using the root of the website*  

*Name the page*
```
[CATDIR1]
```
*Title of the page  (systemkey)*  
```
rocketdirectoryapi
```
## Create a Detail Page (Optional)
Create a normal page on the root level of the website.
*Name of the page (Any name can be used)*
```
Detail
```
If no detail page is defined the list page can be used by selecting it in the module and System Admin Settings.

## Rename [CATDIR] in the URL

We do not want the page name to be "catdir" in our URL, we can rename the page in the URL.  

 - On the personabar go to Rocket>RocketTools>Page Localzation.
 
 - Select the [CATDIR] page and rename it to "catalog".

## DetailURL()

The detail URL will not normally contain the categoryid parameter.  To avoid adding this to the URL you can pass *null* as the categoryData.
```
@DetailUrl(moduleData.DetailPageTabId(), articleData, null)
```

 