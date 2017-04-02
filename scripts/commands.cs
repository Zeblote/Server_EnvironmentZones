// Handles user interaction with the environment zone API.
// -------------------------------------------------------------------

function serverCmdEnvHelp(%client)
{
	messageClient(%client, '', " ");
	messageClient(%client, '', "\c6You can use the following commands with environment zones:");
	messageClient(%client, '', "\c7--------------------------------------------------------------------------------");

	messageClient(%client, '', "<tab:280>\c3/showEnvZones\t\c6 Show all environment zones in the world using dup selection boxes.");
	messageClient(%client, '', "<tab:280>\c3/hideEnvZones\t\c6 Hide all environment zones.");
	messageClient(%client, '', "<font:Arial:8> ");

	messageClient(%client, '', "<tab:280>\c3/createEnvZone\c6 [\c3name\c6]\t\c6 Create a new named environment zone.");
	messageClient(%client, '', "<tab:280>\c3/deleteEnvZone\c6 [\c3name\c6]\t\c6 Delete a named environment zone.");
	messageClient(%client, '', "<tab:280>\c3/deleteEnvZones\t\c6 Delete all environment zones.");
	messageClient(%client, '', "<tab:280>\c3/listEnvZones\t\c6 List all environment zones in the world.");
	messageClient(%client, '', "<font:Arial:8> ");

	messageClient(%client, '', "<tab:280>\c3/editEnvZone\c6 [\c3name\c6]\t\c6 Edit the size of an environment zone.");
	messageClient(%client, '', "<tab:280>\c3/editEnvZone\t\c6 Stop editing the size and apply changes.");
	messageClient(%client, '', "<font:Arial:8> ");

	messageClient(%client, '', "<tab:280>\c3/setEnvZonePersistent\c6 [\c3name\c6]\t\c6 Persistent zones will keep the environment active even after you leave them.");
	messageClient(%client, '', "<tab:280>\c3/setEnvZoneLocal\c6 [\c3name\c6]\t\c6 Remove the persistent flag. This will not update players still seeing the environment.");
	messageClient(%client, '', "<font:Arial:8> ");

	messageClient(%client, '', "<tab:280>\c3/saveEnvZones\c6 [\c3name\c6]\t\c6 Save all environment zones to a file.");
	messageClient(%client, '', "<tab:280>\c3/loadEnvZones\c6 [\c3name\c6]\t\c6 Replace all environment zones with saved ones.");
	messageClient(%client, '', "<tab:280>\c3/deleteEnvZoneSave\c6 [\c3name\c6]\t\c6 Permanently delete an environment zone save.");
	messageClient(%client, '', "<tab:280>\c3/listEnvZoneSaves\t\c6 List all the available saves, in case you forgot them.");

	messageClient(%client, '', "\c7--------------------------------------------------------------------------------");
	messageClient(%client, '', "\c6You might have to use \c3PageUp\c6/\c3PageDown\c6 to see all of them.");
	messageClient(%client, '', " ");
}

$EZ::AdminFailMsg = "\c6Modifying environment zones is admin only. Ask an admin for help.";

function serverCmdShowEnvZones(%client)
{
	if(!%client.isAdmin){messageClient(%client, '', $EZ::AdminFailMsg); return;}

	messageClient(%client, '', "\c6Environment zones are now visible.");
	showEnvironmentZones(1);
}

function serverCmdHideEnvZones(%client)
{
	if(!%client.isAdmin){messageClient(%client, '', $EZ::AdminFailMsg); return;}

	messageClient(%client, '', "\c6Environment zones are now hidden.");
	showEnvironmentZones(0);
}

function serverCmdCreateEnvZone(%client, %n1, %n2, %n3, %n4, %n5)
{
	if(!%client.isAdmin){messageClient(%client, '', $EZ::AdminFailMsg); return;}

	if(!$ShowEnvironmentZones)
		showEnvironmentZones(1);

	%name = trim(%n1 SPC %n2 SPC %n3 SPC %n4 SPC %n5);
	if(!strLen(%name))
	{
		messageClient(%client, '', "\c0You have to specify a name for the new zone!");
		return;
	}

	if(isObject(findEnvZoneByName(%name)))
	{
		messageClient(%client, '', "\c0A zone already exists with that name!");
		return;
	}

	if(!isObject(%client.player))
	{
		messageClient(%client, '', "\c0You need to spawn before creating zones!");
		return;
	}

	%pos = %client.player.getPosition();
	%pos = (getWord(%pos, 0) | 0) SPC (getWord(%pos, 1) | 0) SPC (getWord(%pos, 2) | 0);

	%zone = EnvironmentZone(%name);
	%zone.setSize(vectorAdd(%pos, "-5 -5 0"), vectorAdd(%pos, "5 5 10"));
	messageClient(%client, '', "\c6A new zone has been created somewhere around your player.");
	messageClient(%client, '', "\c6You can edit its environment using the standard gui, while standing inside the zone.");
	%client.player.setTransform(%client.player.getTransform());
}

function serverCmdDeleteEnvZone(%client, %n1, %n2, %n3, %n4, %n5)
{
	if(!%client.isAdmin){messageClient(%client, '', $EZ::AdminFailMsg); return;}

	if(!$ShowEnvironmentZones)
		showEnvironmentZones(1);

	%name = trim(%n1 SPC %n2 SPC %n3 SPC %n4 SPC %n5);	
	if(!strLen(%name))
	{
		messageClient(%client, '', "\c0You have to specify a name for the zone to delete!");
		return;
	}

	if(!isObject(findEnvZoneByName(%name)))
	{
		messageClient(%client, '', "\c0There is no zone with that name!");
		return;
	}

	findEnvZoneByName(%name).delete();
	messageClient(%client, '', "\c6The zone has been deleted.");
}

function serverCmdDeleteEnvZones(%client)
{
	if(!%client.isAdmin){messageClient(%client, '', $EZ::AdminFailMsg); return;}

	if(!EnvironmentZoneGroup.getCount())
	{
		messageClient(%client, '', "\c0There are no zones to delete!");
		return;
	}

	deleteAllEnvZones();
	
	messageClient(%client, '', "\c6All zones have been deleted.");
}

function serverCmdListEnvZones(%client)
{
	if(!%client.isAdmin){messageClient(%client, '', $EZ::AdminFailMsg); return;}

	if(!EnvironmentZoneGroup.getCount())
	{
		messageClient(%client, '', "\c0There are no zones to list!");
		return;
	}
	
	messageClient(%client, '', "\c6The following zones exist in the world:");

	for(%i = 0; %i < EnvironmentZoneGroup.getCount(); %i++)
	{
		%zone = EnvironmentZoneGroup.getObject(%i);
		messageClient(%client, '', "\c6" @ %zone.zoneName);
	}
}

function serverCmdEditEnvZone(%client, %n1, %n2, %n3, %n4, %n5)
{
	if(!%client.isAdmin){messageClient(%client, '', $EZ::AdminFailMsg); return;}

	if(!$ShowEnvironmentZones)
		showEnvironmentZones(1);

	%name = trim(%n1 SPC %n2 SPC %n3 SPC %n4 SPC %n5);	
	if(!strLen(%name))
	{
		if(isObject(%client.envEditZone))
		{
			%client.envEditZone.stopEdit();
			messageClient(%client, '', "\c6Stopped editing the zone. Changes have been applied.");
		}
		else
		{
			messageClient(%client, '', "\c0You aren't editing a zone yet. Specify a name to start!");
		}
		return;
	}

	if(!isObject(findEnvZoneByName(%name)))
	{
		messageClient(%client, '', "\c0There is no zone with that name!");
		return;
	}

	if(isObject(%client.envEditZone))
	{
		%client.envEditZone.stopEdit();
	}	

	findEnvZoneByName(%name).startEdit(%client);
	messageClient(%client, '', "\c6Started editing the zone. Use ghost brick controls, like with a new duplicator selection box.");
}

function serverCmdSetEnvZonePersistent(%client, %n1, %n2, %n3, %n4, %n5)
{
	if(!%client.isAdmin){messageClient(%client, '', $EZ::AdminFailMsg); return;}

	if(!$ShowEnvironmentZones)
		showEnvironmentZones(1);

	%name = trim(%n1 SPC %n2 SPC %n3 SPC %n4 SPC %n5);	
	if(!strLen(%name))
	{
		messageClient(%client, '', "\c0You have to specify a name for the zone to make persistent!");
		return;
	}

	if(!isObject(findEnvZoneByName(%name)))
	{
		messageClient(%client, '', "\c0There is no zone with that name!");
		return;
	}

	%zone = findEnvZoneByName(%name);
	%zone.persistent = true;
	%zone.updateShapeName();
	messageClient(%client, '', "\c6The zone is now persistent.");
}

function serverCmdSetEnvZoneLocal(%client, %n1, %n2, %n3, %n4, %n5)
{
	if(!%client.isAdmin){messageClient(%client, '', $EZ::AdminFailMsg); return;}

	if(!$ShowEnvironmentZones)
		showEnvironmentZones(1);

	%name = trim(%n1 SPC %n2 SPC %n3 SPC %n4 SPC %n5);	
	if(!strLen(%name))
	{
		messageClient(%client, '', "\c0You have to specify a name for the zone to make local!");
		return;
	}

	if(!isObject(findEnvZoneByName(%name)))
	{
		messageClient(%client, '', "\c0There is no zone with that name!");
		return;
	}

	%zone = findEnvZoneByName(%name);
	%zone.persistent = false;
	%zone.updateShapeName();
	messageClient(%client, '', "\c6The zone is no longer persistent.");
}

function serverCmdSaveEnvZones(%client, %f0, %f1, %f2, %f3, %f4, %f5, %f6, %f7)
{
	if(!%client.isAdmin){messageClient(%client, '', $EZ::AdminFailMsg); return;}

	if(!$ShowEnvironmentZones)
		showEnvironmentZones(1);

	if(!EnvironmentZoneGroup.getCount())
	{
		messageClient(%client, '', "\c0There are no zones to save!");
		return;
	}

	%fileName = trim(%f0 SPC %f1 SPC %f2 SPC %f3 SPC %f4 SPC %f5 SPC %f6 SPC %f7);
	if(!strLen(%fileName))
	{
		messageClient(%client, '', "\c0You have to specify a name for the save file!");
		return;
	}

	%allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ._-()";
	%filePath = "config/server/EnvironmentZoneSaves/" @ %fileName @ ".ez";
	%filePath = strReplace(%filePath, ".ez.ez", ".ez");

	for(%i = 0; %i < strLen(%fileName); %i++)
	{
		if(strStr(%allowed, getSubStr(%fileName, %i, 1)) == -1)
		{
			%forbidden = true;
			break;
		}
	}

	if(%forbidden || !strLen(%fileName) || strLen(%fileName) > 50)
	{
		messageClient(%client, '', "\c0The file name contains invalid characters, try again!");
		return;
	}

	if(isFile(%filePath))
	{
		fileDelete(%filePath);
	}

	exportEnvironmentZones(%filePath);
	messageClient(%client, '', "\c6All zones have been saved.");
}

function serverCmdLoadEnvZones(%client, %f0, %f1, %f2, %f3, %f4, %f5, %f6, %f7)
{
	if(!%client.isAdmin){messageClient(%client, '', $EZ::AdminFailMsg); return;}

	if(!$ShowEnvironmentZones)
		showEnvironmentZones(1);

	%fileName = trim(%f0 SPC %f1 SPC %f2 SPC %f3 SPC %f4 SPC %f5 SPC %f6 SPC %f7);
	if(!strLen(%fileName))
	{
		messageClient(%client, '', "\c0You have to specify a name for the save file to load!");
		return;
	}

	%allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ._-()";
	%filePath = "config/server/EnvironmentZoneSaves/" @ %fileName @ ".ez";
	%filePath = strReplace(%filePath, ".ez.ez", ".ez");

	for(%i = 0; %i < strLen(%fileName); %i++)
	{
		if(strStr(%allowed, getSubStr(%fileName, %i, 1)) == -1)
		{
			%forbidden = true;
			break;
		}
	}

	if(%forbidden || !strLen(%fileName) || strLen(%fileName) > 50)
	{
		messageClient(%client, '', "\c0The file name contains invalid characters, try again!");
		return;
	}

	if(!isFile(%filePath))
	{
		messageClient(%client, '', "\c0There is no save file with that name!");
		return;
	}

	loadEnvironmentZones(%filePath);
	messageClient(%client, '', "\c6All zones have been loaded.");
}

function serverCmdDeleteEnvZoneFile(%client, %f0, %f1, %f2, %f3, %f4, %f5, %f6, %f7)
{
	if(!%client.isAdmin){messageClient(%client, '', $EZ::AdminFailMsg); return;}

	%fileName = trim(%f0 SPC %f1 SPC %f2 SPC %f3 SPC %f4 SPC %f5 SPC %f6 SPC %f7);
	if(!strLen(%fileName))
	{
		messageClient(%client, '', "\c0You have to specify a name for the save file to delete!");
		return;
	}

	%allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ._-()";
	%filePath = "config/server/EnvironmentZoneSaves/" @ %fileName @ ".ez";
	%filePath = strReplace(%filePath, ".ez.ez", ".ez");

	for(%i = 0; %i < strLen(%fileName); %i++)
	{
		if(strStr(%allowed, getSubStr(%fileName, %i, 1)) == -1)
		{
			%forbidden = true;
			break;
		}
	}

	if(%forbidden || !strLen(%fileName) || strLen(%fileName) > 50)
	{
		messageClient(%client, '', "\c0The file name contains invalid characters, try again!");
		return;
	}

	if(!isFile(%filePath))
	{
		messageClient(%client, '', "\c0There is no save file with that name!");
		return;
	}

	fileDelete(%filePath);
	messageClient(%client, '', "\c6The save file has been permanently deleted.");
}

function serverCmdListEnvZoneSaves(%client)
{
	if(!%client.isAdmin){messageClient(%client, '', $EZ::AdminFailMsg); return;}

	messageClient(%client, '', "\c6Available files:");
	%pattern = "config/server/EnvironmentZoneSaves/*.ez";
	for(%i = findFirstFile(%pattern); isFile(%i); %i = findNextFile(%pattern))
	{
		messageClient(%client, '', "\c6" @ fileBase(%i));
	}
}

function findEnvZoneByName(%name)
{
	for(%i = 0; %i < EnvironmentZoneGroup.getCount(); %i++)
	{
		%zone = EnvironmentZoneGroup.getObject(%i);
		if(%zone.zoneName $= %name)
			return %zone.getId();
	}
}
