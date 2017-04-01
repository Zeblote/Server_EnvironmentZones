function GameConnection::setEnvironment(%this, %env)
{
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

package EnvironmentZones
{
	function GameConnection::onClientEnterGame(%this)
	{
		parent::onClientEnterGame(%this);

		setupDefaultEnvironment();
		%this.setEnvironment($DefaultEnvironment);
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
	if(!%client.isAdmin)
		return;

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

	messageClient(%client, '', "\c7--------------------------------------------------------------------------------");
	messageClient(%client, '', "\c6You might have to use \c3PageUp\c6/\c3PageDown\c6 to see all of them.");
	messageClient(%client, '', " ");
}

function serverCmdShowEnvZones(%client)
{
	if(!%client.isAdmin)
		return;

	messageClient(%client, '', "\c6Environment zones are now visible.");
	showEnvironmentZones(1);
}

function serverCmdHideEnvZones(%client)
{
	if(!%client.isAdmin)
		return;

	messageClient(%client, '', "\c6Environment zones are now hidden.");
	showEnvironmentZones(0);
}

function serverCmdCreateEnvZone(%client, %n1, %n2, %n3, %n4, %n5)
{
	if(!%client.isAdmin)
		return;

	if(!$ShowEnvironmentZones)
	{
		messageClient(%client, '', "\c6Zone visualization must be enabled before creating new zones.");
		return;
	}

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
		return;

	if(!$ShowEnvironmentZones)
	{
		messageClient(%client, '', "\c6Zone visualization must be enabled before deleting zones.");
		return;
	}

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
		return;

	if(!EnvironmentZoneGroup.getCount())
	{
		messageClient(%client, '', "\c6There are no zones to delete.");
		return;
	}

	while(EnvironmentZoneGroup.getCount())
	{
		EnvironmentZoneGroup.getObject(0).delete();
	}
	
	messageClient(%client, '', "\c6All zones deleted.");
}

function serverCmdEditEnvZone(%client, %n1, %n2, %n3, %n4, %n5)
{
	if(!%client.isAdmin)
		return;

	if(!$ShowEnvironmentZones)
	{
		messageClient(%client, '', "\c6Zone visualization must be enabled before editing zones.");
		return;
	}

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
		return;

	if(!$ShowEnvironmentZones)
	{
		messageClient(%client, '', "\c6Zone visualization must be enabled before editing zones.");
		return;
	}

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
		return;

	if(!$ShowEnvironmentZones)
	{
		messageClient(%client, '', "\c6Zone visualization must be enabled before editing zones.");
		return;
	}

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

function findEnvZoneByName(%name)
{
	for(%i = 0; %i < EnvironmentZoneGroup.getCount(); %i++)
	{
		%zone = EnvironmentZoneGroup.getObject(%i);
		if(%zone.zoneName $= %name)
			return %zone.getId();
	}
}
