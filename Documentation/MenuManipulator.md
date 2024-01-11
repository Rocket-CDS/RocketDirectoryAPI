# Menu Manipulator

Categoeries can be injected into the webiste menu to give a seemless navigation from the website to the directory categories.  

*(See RocketTools Page Localization installation instructions)*

## Automatically Add
The menu provider the information should be in the "SystemDefaults.rules" file and it will then be automatically added to the page localiztion settings.  
Each sub-system of RocketDirectory should deifne this in the "SystemDefaults.rules" file of the system.  
The menu provider is updated when the Admin Settings of the system are saved.

```
	<menuprovider>
		<assembly>RocketDirectoryAPI</assembly>
		<namespaceclass>RocketDirectoryAPI.Components.MenuDirectory</namespaceclass>
	</menuprovider>
```
*The systemkey is not required becuase it is defined by the DefaultsLimpet class.*


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

