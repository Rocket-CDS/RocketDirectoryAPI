# Admin Panel Interfaces

An AppTheme for "rocketdirectoryapi" system (or a wrapper system) can select which options are available in the Admin Panel of the system.  

By default options are shown, if you want to hide options then you need to have a "adminpanelinterfacekeys" section in the dependancy file of the AppTheme.  

there are 5 options that can be shown.
- Articel Admin
- Category Admin
- Property Admin
- Settings Admin
- Portal System Admin  (Host only)

The superuser will always see all admin options.

All activated plugins will be shown on the Admin Panel.

Setting the show node to "False" will hide the option.

Example:
```
<adminpanelinterfacekeys list="true">
	<genxml>
		<interfacekey>articleadmin</interfacekey>
		<show>true</show>
	</genxml>
	<genxml>
		<interfacekey>categoryadmin</interfacekey>
		<show>true</show>
	</genxml>
	<genxml>
		<interfacekey>propertyadmin</interfacekey>
		<show>true</show>
	</genxml>
	<genxml>
		<interfacekey>settingsadmin</interfacekey>
		<show>true</show>
	</genxml>
	<genxml>
		<interfacekey>rocketdirectoryadmin</interfacekey>
		<show>true</show>
	</genxml>
</adminpanelinterfacekeys>
```
