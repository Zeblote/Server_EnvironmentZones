function GameConnection::setEnvironment(%this, %env)
{
	if(%this.currentEnvironment == %env)
		return;

	if(isObject(%this.currentEnvironment))
		%this.currentEnvironment.clearScopeToClient(%this);

	%this.currentEnvironment = %env;
	%env.scopeToClient(%this);
	EnvGuiServer::sendVignette(%this);
}

function setupDefaultEnvironment()
{
	if(!isObject($DefaultEnvironment))
	{
		%env = Environment();
		%env.sky = nameToId("Sky");
		%env.sun = nameToId("Sun");
		%env.sunLight = nameToId("SunLight");
		%env.dayCycle = nameToId("DayCycle");
		%env.groundPlane = nameToId("GroundPlane");
		%env.waterPlane = nameToId("WaterPlane");
		%env.waterZone = nameToId("WaterZone");
		%env.rain = nameToId("Rain");
		%env.copyVariables();
		%env.setupNetFlags();

		$CurrentEnvironment = %env.getId();
		$DefaultEnvironment = %env.getId();
	}
}

function exportEnvironmentZones(%filename)
{
	%file = new FileObject();
	%file.openForWrite(%filename);

	%file.writeLine(EnvironmentZoneGroup.getCount());
	for(%i = 0; %i < EnvironmentZoneGroup.getCount(); %i++)
	{
		%zone = EnvironmentZoneGroup.getObject(%i);
		%env = %zone.environment;
		%file.writeLine("ZONE: " @ %zone.zoneName);
		%file.writeLine(!!%zone.persistent);
		%file.writeLine(%zone.point1 TAB %zone.point2);

		%file.writeLine("File_DayCycle" TAB $EnvGuiServer::DayCycle[%env.var_DayCycleIdx]);
		%file.writeLine("File_Ground" TAB $EnvGuiServer::Ground[%env.var_GroundIdx]);
		%file.writeLine("File_Sky" TAB $EnvGuiServer::Sky[%env.var_SkyIdx]);
		%file.writeLine("File_SunFlareBottom" TAB $EnvGuiServer::SunFlare[%env.var_SunFlareBottomIdx]);
		%file.writeLine("File_SunFlareTop" TAB $EnvGuiServer::SunFlare[%env.var_SunFlareTopIdx]);
		%file.writeLine("File_Water" TAB $EnvGuiServer::Water[%env.var_WaterIdx]);

		%file.writeLine("SimpleMode" TAB %env.var_SimpleMode);

		%file.writeLine("AmbientLightColor" TAB %env.var_AmbientLightColor);
		%file.writeLine("DayCycleEnabled" TAB %env.var_DayCycleEnabled);
		%file.writeLine("DayLength" TAB %env.var_DayLength);
		%file.writeLine("DirectLightColor" TAB %env.var_DirectLightColor);
		%file.writeLine("FogColor" TAB %env.var_FogColor);
		%file.writeLine("FogDistance" TAB %env.var_FogDistance);
		%file.writeLine("FogHeight" TAB %env.var_FogHeight);
		%file.writeLine("GroundColor" TAB %env.var_GroundColor);
		%file.writeLine("GroundScrollX" TAB %env.var_GroundScrollX);
		%file.writeLine("GroundScrollY" TAB %env.var_GroundScrollY);
		%file.writeLine("HasSetAdvancedOnce" TAB %env.var_HasSetAdvancedOnce);
		%file.writeLine("ShadowColor" TAB %env.var_ShadowColor);
		%file.writeLine("SkyColor" TAB %env.var_SkyColor);
		%file.writeLine("SunAzimuth" TAB %env.var_SunAzimuth);
		%file.writeLine("SunElevation" TAB %env.var_SunElevation);
		%file.writeLine("SunFlareColor" TAB %env.var_SunFlareColor);
		%file.writeLine("SunFlareSize" TAB %env.var_SunFlareSize);
		%file.writeLine("UnderWaterColor" TAB %env.var_UnderWaterColor);
		%file.writeLine("VignetteColor" TAB %env.var_VignetteColor);
		%file.writeLine("VignetteMultiply" TAB %env.var_VignetteMultiply);
		%file.writeLine("VisibleDistance" TAB %env.var_VisibleDistance);
		%file.writeLine("WaterColor" TAB %env.var_WaterColor);
		%file.writeLine("WaterHeight" TAB %env.var_WaterHeight);
		%file.writeLine("WaterScrollX" TAB %env.var_WaterScrollX);
		%file.writeLine("WaterScrollY" TAB %env.var_WaterScrollY);
		%file.writeLine("WindEffectPrecipitation" TAB %env.var_WindEffectPrecipitation);
		%file.writeLine("WindVelocity" TAB %env.var_WindVelocity);
		%file.writeLine("SimpleVignetteColor" TAB %env.simple_VignetteColor);
		%file.writeLine("SimpleVignetteMultiply" TAB %env.simple_VignetteMultiply);
		%file.writeLine("");
	}

	%file.delete();
}

function loadEnvironmentZones(%filename)
{
	deleteAllEnvZones();

	%file = new FileObject();
	%file.openForRead(%filename);

	%num = %file.readLine();
	if(!%num || %num > 1000)
	{
		messageAll('', "\c0ERROR: Broken environment zone file.");
		return;
	}

	messageAll('', "\c6Loading " @ %num @ " environment zones...");
	
	while(!%file.isEOF())
	{
		%line = %file.readLine();
		if(firstWord(%line) !$= "ZONE:")
		{
			messageAll('', "\c0ERROR: Broken environment zone file.");
			deleteAllEnvZones();
			return;
		}
		%name = getSubStr(%line, 6, 999);
		if(!strLen(%name))
		{
			messageAll('', "\c0ERROR: Broken environment zone file.");
			deleteAllEnvZones();
			return;
		}

		%persistent = !!%file.readLine();
		%line = %file.readLine();
		%point1 = getField(%line, 0);
		%point2 = getField(%line, 1);

		messageAll('', "\c6Loading zone \"" @ %name @ "\".");
		%zone = EnvironmentZone(%name);
		%zone.setSize(%point1, %point2);
		%zone.persistent = %persistent;
		%zone.updateShapeName();

		while(!%file.isEOF() && strLen(%line = %file.readLine()))
		{
			%var = getField(%line, 0);
			%value = getField(%line, 1);

			if(getSubStr(%var, 0, 5) $= "File_")
			{
				%type = getSubStr(%var, 5, 999);
				%value = getEnvFileIdx(%type, %value);
				%var = %type @ "Idx";
			}

			setEnvironment(%var, %value);
		}

		%zone.environment.postEditCheck();
	}

	%file.delete();
}

function getEnvFileIdx(%type, %file)
{
	%count = $EnvGuiServer["::" @ %type @ "Count"];

	for(%i = 0; %i < %count; %i++)
	{
		%filex = $EnvGuiServer["::" @ %type @ %i];
		if(%filex $= %file)
			return %i;
	}

	return -1;
}

function deleteAllEnvZones()
{
	while(EnvironmentZoneGroup.getCount())
	{
		EnvironmentZoneGroup.getObject(0).delete();
	}
}

package EnvironmentZones
{
	function GameConnection::onClientEnterGame(%this)
	{
		parent::onClientEnterGame(%this);

		setupDefaultEnvironment();
		%this.setEnvironment($DefaultEnvironment);
	}

	function GameConnection::spawnPlayer(%this)
	{
		setupDefaultEnvironment();
		%this.setEnvironment($DefaultEnvironment);

		return parent::spawnPlayer(%this);
	}

	function GameConnection::onClientLeaveGame(%this)
	{
		if(isObject(%this.envEditZone))
			%this.envEditZone.stopEdit();

		parent::onClientLeaveGame(%this);
	}

	function serverCmdEnvGui_RequestCurrent(%client)
	{
		if(%client.isAdmin)
			%client.currentEnvironment.setCurrent();

		parent::serverCmdEnvGui_RequestCurrent(%client);
	}

	function serverCmdEnvGui_RequestLists(%client)
	{
		if(%client.isAdmin)
			%client.currentEnvironment.setCurrent();

		parent::serverCmdEnvGui_RequestLists(%client);
	}

	function serverCmdEnvGui_RequestCurrentVars(%client)
	{
		if(%client.isAdmin)
			%client.currentEnvironment.setCurrent();

		parent::serverCmdEnvGui_RequestCurrentVars(%client);
	}

	function serverCmdEnvGui_ClickDefaults(%client)
	{
		if(%client.isAdmin)
			%client.currentEnvironment.setCurrent();

		parent::serverCmdEnvGui_ClickDefaults(%client);

		if(%client.isAdmin)
			%client.currentEnvironment.postEditCheck();
	}

	function serverCmdEnvGui_SetVar(%client, %varName, %value)
	{
		if(%client.isAdmin)
			%client.currentEnvironment.setCurrent();

		parent::serverCmdEnvGui_SetVar(%client, %varName, %value);

		if(%client.isAdmin)
			%client.currentEnvironment.postEditCheck();
	}

	function EnvGuiServer::SendVignetteAll()
	{
		parent::SendVignetteAll();
	}

	function EnvGuiServer::sendVignette(%client)
	{
		if(!isObject(%client))
			return;

		if(!isObject(%client.currentEnvironment) || %client.currentEnvironment.getId() == $CurrentEnvironment)
		{
			if($EnvGuiServer::SimpleMode)
				commandToClient(%client, 'setVignette', $Sky::VignetteMultiply, $Sky::VignetteColor);
			else
				commandToClient(%client, 'setVignette', $EnvGuiServer::VignetteMultiply, $EnvGuiServer::VignetteColor);
		}
		else
		{
			if(%client.currentEnvironment.var_SimpleMode)
				commandToClient(%client, 'setVignette', %client.currentEnvironment.simple_VignetteMultiply, %client.currentEnvironment.simple_VignetteColor);
			else
				commandToClient(%client, 'setVignette', %client.currentEnvironment.var_VignetteMultiply, %client.currentEnvironment.var_VignetteColor);
		}
	}
};

activatePackage(EnvironmentZones);

function serverCmdEnvHelp(%client)
{
	messageClient(%client, '', " ");
	messageClient(%client, '', "\c6You can use the following commands with environment zones:");
	messageClient(%client, '', "\c7--------------------------------------------------------------------------------");

	messageClient(%client, '', "<tab:220>\c3/showEnvZones\t\c6 Visualize environment zones using dup selection boxes.");
	messageClient(%client, '', "<tab:220>\c3/hideEnvZones\t\c6 Hide the zone visualizations.");
	messageClient(%client, '', "<font:Arial:8> ");

	messageClient(%client, '', "<tab:220>\c3/createEnvZone\c6 [\c3name\c6]\t\c6 Create an environment zone with specified name.");
	messageClient(%client, '', "<tab:220>\c3/deleteEnvZone\c6 [\c3name\c6]\t\c6 Delete an environment zone.");
	messageClient(%client, '', "<tab:220>\c3/deleteEnvZones\t\c6 Delete all environment zones.");
	messageClient(%client, '', "<font:Arial:8> ");

	messageClient(%client, '', "<tab:220>\c3/editEnvZone\c6 [\c3name\c6]\t\c6 Modify the zone using the dup selection box.");
	messageClient(%client, '', "<tab:220>\c3/editEnvZone\t\c6 Stop editing the zone.");
	messageClient(%client, '', "<font:Arial:8> ");

	messageClient(%client, '', "<tab:220>\c3/setEnvZonePersistent\c6 [\c3name\c6]\t\c6 Persistent zones will keep the environment active even when you leave them.");
	messageClient(%client, '', "<tab:220>\c3/setEnvZoneLocal\c6 [\c3name\c6]\t\c6 Remove the persistent flag. This will not update players still seeing the environment.");
	messageClient(%client, '', "<font:Arial:8> ");

	messageClient(%client, '', "<tab:220>\c3/saveEnvZones\c6 [\c3name\c6]\t\c6 Save the current environment zones to a file.");
	messageClient(%client, '', "<tab:220>\c3/loadEnvZones\c6 [\c3name\c6]\t\c6 Load the environment zone setup from a file.");
	messageClient(%client, '', "<tab:220>\c3/deleteEnvZoneFile\c6 [\c3name\c6]\t\c6 Permanently delete an environment zone setup.");
	messageClient(%client, '', "<tab:220>\c3/listEnvZoneFiles\t\c6 List the available saves, in case you forgot them.");

	messageClient(%client, '', "\c7--------------------------------------------------------------------------------");
	messageClient(%client, '', "\c6You might have to use \c3PageUp\c6/\c3PageDown\c6 to see all of them.");
	messageClient(%client, '', " ");
}

function serverCmdShowEnvZones(%client)
{
	if(!%client.isAdmin)
	{
		messageClient(%client, '', "\c6This command is admin-only.");
		return;
	}

	messageClient(%client, '', "\c6Environment zones are now visible.");
	showEnvironmentZones(1);
}

function serverCmdHideEnvZones(%client)
{
	if(!%client.isAdmin)
	{
		messageClient(%client, '', "\c6This command is admin-only.");
		return;
	}

	messageClient(%client, '', "\c6Environment zones are now hidden.");
	showEnvironmentZones(0);
}

function serverCmdCreateEnvZone(%client, %n1, %n2, %n3, %n4, %n5)
{
	if(!%client.isAdmin)
	{
		messageClient(%client, '', "\c6This command is admin-only.");
		return;
	}

	if(!$ShowEnvironmentZones)
		showEnvironmentZones(1);

	%name = trim(%n1 SPC %n2 SPC %n3 SPC %n4 SPC %n5);
	if(!strLen(%name))
	{
		messageClient(%client, '', "\c6Invalid name.");
		return;
	}

	if(isObject(findEnvZoneByName(%name)))
	{
		messageClient(%client, '', "\c6Zone already exists.");
		return;
	}

	if(!isObject(%client.player))
	{
		messageClient(%client, '', "\c6You need to spawn first.");
		return;
	}

	%pos = %client.player.getPosition();
	%pos = (getWord(%pos, 0) | 0) SPC (getWord(%pos, 1) | 0) SPC (getWord(%pos, 2) | 0);

	%zone = EnvironmentZone(%name);
	%zone.setSize(vectorAdd(%pos, "-5 -5 0"), vectorAdd(%pos, "5 5 10"));
	messageClient(%client, '', "\c6Zone created.");
}

function serverCmdDeleteEnvZone(%client, %n1, %n2, %n3, %n4, %n5)
{
	if(!%client.isAdmin)
	{
		messageClient(%client, '', "\c6This command is admin-only.");
		return;
	}

	if(!$ShowEnvironmentZones)
		showEnvironmentZones(1);

	%name = trim(%n1 SPC %n2 SPC %n3 SPC %n4 SPC %n5);	
	if(!strLen(%name))
	{
		messageClient(%client, '', "\c6Invalid name.");
		return;
	}

	if(!isObject(findEnvZoneByName(%name)))
	{
		messageClient(%client, '', "\c6Zone doesn't exist.");
		return;
	}

	findEnvZoneByName(%name).delete();
	messageClient(%client, '', "\c6Zone deleted.");
}

function serverCmdDeleteEnvZones(%client)
{
	if(!%client.isAdmin)
	{
		messageClient(%client, '', "\c6This command is admin-only.");
		return;
	}

	if(!EnvironmentZoneGroup.getCount())
	{
		messageClient(%client, '', "\c6There are no zones to delete.");
		return;
	}

	deleteAllEnvZones();
	
	messageClient(%client, '', "\c6All zones deleted.");
}

function serverCmdEditEnvZone(%client, %n1, %n2, %n3, %n4, %n5)
{
	if(!%client.isAdmin)
	{
		messageClient(%client, '', "\c6This command is admin-only.");
		return;
	}

	if(!$ShowEnvironmentZones)
		showEnvironmentZones(1);

	%name = trim(%n1 SPC %n2 SPC %n3 SPC %n4 SPC %n5);	
	if(!strLen(%name))
	{
		if(isObject(%client.envEditZone))
		{
			%client.envEditZone.stopEdit();
			messageClient(%client, '', "\c6Stopped editing zone.");
		}
		else
		{
			messageClient(%client, '', "\c6You aren't editing a zone yet.");
		}
		return;
	}

	if(!isObject(findEnvZoneByName(%name)))
	{
		messageClient(%client, '', "\c6Zone doesn't exist.");
		return;
	}

	if(isObject(%client.envEditZone))
	{
		%client.envEditZone.stopEdit();
	}	

	findEnvZoneByName(%name).startEdit(%client);
	messageClient(%client, '', "\c6Started editing zone. Use ghost brick controls.");
}

function serverCmdSetEnvZonePersistent(%client, %n1, %n2, %n3, %n4, %n5)
{
	if(!%client.isAdmin)
	{
		messageClient(%client, '', "\c6This command is admin-only.");
		return;
	}

	if(!$ShowEnvironmentZones)
		showEnvironmentZones(1);

	%name = trim(%n1 SPC %n2 SPC %n3 SPC %n4 SPC %n5);	
	if(!strLen(%name))
	{
		messageClient(%client, '', "\c6Invalid name.");
		return;
	}

	if(!isObject(findEnvZoneByName(%name)))
	{
		messageClient(%client, '', "\c6Zone doesn't exist.");
		return;
	}

	%zone = findEnvZoneByName(%name);
	%zone.persistent = true;
	%zone.updateShapeName();
	messageClient(%client, '', "\c6Zone is now persistent.");
}

function serverCmdSetEnvZoneLocal(%client, %n1, %n2, %n3, %n4, %n5)
{
	if(!%client.isAdmin)
	{
		messageClient(%client, '', "\c6This command is admin-only.");
		return;
	}

	if(!$ShowEnvironmentZones)
		showEnvironmentZones(1);

	%name = trim(%n1 SPC %n2 SPC %n3 SPC %n4 SPC %n5);	
	if(!strLen(%name))
	{
		messageClient(%client, '', "\c6Invalid name.");
		return;
	}

	if(!isObject(findEnvZoneByName(%name)))
	{
		messageClient(%client, '', "\c6Zone doesn't exist.");
		return;
	}

	%zone = findEnvZoneByName(%name);
	%zone.persistent = false;
	%zone.updateShapeName();
	messageClient(%client, '', "\c6Zone is no longer persistent.");
}

function serverCmdSaveEnvZones(%client, %f0, %f1, %f2, %f3, %f4, %f5, %f6, %f7)
{
	if(!%client.isAdmin)
	{
		messageClient(%client, '', "\c6This command is admin-only.");
		return;
	}

	if(!$ShowEnvironmentZones)
		showEnvironmentZones(1);

	if(!EnvironmentZoneGroup.getCount())
	{
		messageClient(%client, '', "\c6There are no zones to save.");
		return;
	}

	%fileName = trim(%f0 SPC %f1 SPC %f2 SPC %f3 SPC %f4 SPC %f5 SPC %f6 SPC %f7);
	if(!strLen(%fileName))
	{
		messageClient(%client, '', "\c6Invalid name.");
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
		messageClient(%client, '', "\c6Bad file name, try again.");
		return;
	}

	if(isFile(%filePath))
	{
		fileDelete(%filePath);
	}

	exportEnvironmentZones(%filePath);
	messageClient(%client, '', "\c6Zones saved.");
}

function serverCmdLoadEnvZones(%client, %f0, %f1, %f2, %f3, %f4, %f5, %f6, %f7)
{
	if(!%client.isAdmin)
	{
		messageClient(%client, '', "\c6This command is admin-only.");
		return;
	}

	if(!$ShowEnvironmentZones)
		showEnvironmentZones(1);

	%fileName = trim(%f0 SPC %f1 SPC %f2 SPC %f3 SPC %f4 SPC %f5 SPC %f6 SPC %f7);
	if(!strLen(%fileName))
	{
		messageClient(%client, '', "\c6Invalid name.");
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
		messageClient(%client, '', "\c6Bad file name, try again.");
		return;
	}

	if(!isFile(%filePath))
	{
		messageClient(%client, '', "\c6File does not exist.");
		return;
	}

	loadEnvironmentZones(%filePath);
	messageClient(%client, '', "\c6Zones loaded.");
}

function serverCmdDeleteEnvZoneFile(%client, %f0, %f1, %f2, %f3, %f4, %f5, %f6, %f7)
{
	if(!%client.isAdmin)
	{
		messageClient(%client, '', "\c6This command is admin-only.");
		return;
	}

	%fileName = trim(%f0 SPC %f1 SPC %f2 SPC %f3 SPC %f4 SPC %f5 SPC %f6 SPC %f7);
	if(!strLen(%fileName))
	{
		messageClient(%client, '', "\c6Invalid name.");
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
		messageClient(%client, '', "\c6Bad file name, try again.");
		return;
	}

	if(!isFile(%filePath))
	{
		messageClient(%client, '', "\c6File does not exist.");
		return;
	}

	fileDelete(%filePath);
	messageClient(%client, '', "\c6File deleted.");
}

function serverCmdListEnvZoneFiles(%client)
{
	if(!%client.isAdmin)
	{
		messageClient(%client, '', "\c6This command is admin-only.");
		return;
	}

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
