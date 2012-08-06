steamlauncher
=============
Simple steam launcher for those, who are tired of entering passwords endlessly.
Use XMLCreator to create XML, which would hold all the settings.
Use steamlauncher - to launch steam with following parameter - application name from xml (steam.apps.app[name]) and user name (steam.computername).
Example - steamlauncher.exe application_name username

Login and password would be stored encrypted in XML.
Shared secret and password is embedded in sources (so, to ensure security - you have to change it and recompile application (though, I do realize, that this is pretty weak protection - but still)...