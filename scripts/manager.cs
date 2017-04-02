function exportEnvironmentZones(%filename)
{
	// Save all environment zones to a file
	%file = new FileObject();
	if(!%file.openForWrite(%filename))
		return false;

	%file.writeLine(EnvironmentZoneGroup.getCount());
	for(%i = 0; %i < EnvironmentZoneGroup.getCount(); %i++)
	{
		%zone = EnvironmentZoneGroup.getObject(%i);
		%env = %zone.environment;

		// Write zone data
		%file.writeLine("ZONE: " @ %zone.zoneName);
		%file.writeLine(!!%zone.persistent);
		%file.writeLine(%zone.point1 TAB %zone.point2);

		// Write simple environment data
		%file.writeLine("File_Ground" TAB $EnvGuiServer::Ground[%env.var_GroundIdx]);
		%file.writeLine("File_Sky" TAB $EnvGuiServer::Sky[%env.var_SkyIdx]);
		%file.writeLine("File_Water" TAB $EnvGuiServer::Water[%env.var_WaterIdx]);

		%file.writeLine("SimpleMode" TAB %env.var_SimpleMode);

		if(!%env.var_SimpleMode)
		{
			// Write advanced environment data
			%file.writeLine("File_DayCycle" TAB $EnvGuiServer::DayCycle[%env.var_DayCycleIdx]);
			%file.writeLine("File_SunFlareBottom" TAB $EnvGuiServer::SunFlare[%env.var_SunFlareBottomIdx]);
			%file.writeLine("File_SunFlareTop" TAB $EnvGuiServer::SunFlare[%env.var_SunFlareTopIdx]);
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
		}

		%file.writeLine("");
	}

	%file.delete();
	return true;
}

function loadEnvironmentZones(%filename)
{
	// Read all environment zones from a file
	deleteAllEnvZones();

	%file = new FileObject();
	if(!%file.openForRead(%filename))
		return false;

	// If we get a bad number here, it's likely not a zone file
	%num = %file.readLine() * 1;
	if(!%num || %num > 1000)
	{
		messageAll('', "\c0ERROR: Bad number of zones in file.");
		return false;
	}

	messageAll('', "\c6Loading " @ %num @ " environment zones...");
	
	while(!%file.isEOF())
	{
		// Each zone block should start with "ZONE: name"
		%line = %file.readLine();
		if(firstWord(%line) !$= "ZONE:")
		{
			messageAll('', "\c0ERROR: Missing zone start marker.");
			deleteAllEnvZones();
			return false;
		}
		%name = getSubStr(%line, 6, 999);
		if(!strLen(%name))
		{
			messageAll('', "\c0ERROR: Missing zone name.");
			deleteAllEnvZones();
			return false;
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
		%skipping = false;

		while(!%file.isEOF() && strLen(%line = %file.readLine()))
		{
			if(%skipping)
				continue;

			%var = getField(%line, 0);
			%value = getField(%line, 1);

			if(%var $= "SimpleMode" && %value == 1)
			{
				// Skip all the advanced settings that follow.
				// Support for broken environments from v1.0
				%skipping = true;
			}

			// Resource ids change between restarts, so find the correct ones
			switch$(%var)
			{
				case "File_Ground":
					%var = "GroundIdx";
					%value = getEnvironmentFileIdx("Ground", %value);
				case "File_Sky":
					%var = "SkyIdx";
					%value = getEnvironmentFileIdx("Sky", %value);
				case "File_Water":
					%var = "WaterIdx";
					%value = getEnvironmentFileIdx("Water", %value);
				case "File_DayCycle":
					%var = "DayCycleIdx";
					%value = getEnvironmentFileIdx("DayCycle", %value);
				case "File_SunFlareBottom":
					%var = "SunFlareBottomIdx";
					%value = getEnvironmentFileIdx("SunFlare", %value);
				case "File_SunFlareTop":
					%var = "SunFlareTopIdx";
					%value = getEnvironmentFileIdx("SunFlare", %value);
			}

			setEnvVariable(%var, %value);
		}

		%zone.environment.postEditCheck();
	}

	%file.delete();
	return true;
}

function getEnvironmentFileIdx(%type, %search)
{
	%count = $EnvGuiServer["::" @ %type @ "Count"];

	for(%i = 0; %i < %count; %i++)
	{
		%file = $EnvGuiServer["::" @ %type @ %i];
		if(%file $= %search)
			return %i;
	}

	return 0;
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
	// Mark clients as using the global environment when they spawn
	function GameConnection::onClientEnterGame(%this)
	{
		parent::onClientEnterGame(%this);

		setupDefaultEnvironment();
		%this.popAllEnvironments();
		%this.setEnvironment($DefaultEnvironment);
	}

	// Mark clients as using the global environment when they spawn
	function GameConnection::spawnPlayer(%this)
	{
		setupDefaultEnvironment();
		%this.popAllEnvironments();

		return parent::spawnPlayer(%this);
	}

	// Make sure we don't leave edit boxes in limbo if the client leaves
	function GameConnection::onClientLeaveGame(%this)
	{
		if(isObject(%this.envEditZone))
			%this.envEditZone.stopEdit();

		parent::onClientLeaveGame(%this);
	}

	// EnvGui should receive variables from the environment the client is using
	function serverCmdEnvGui_RequestCurrent(%client)
	{
		if(%client.isAdmin)
			%client.currentEnvironment.setCurrent();

		parent::serverCmdEnvGui_RequestCurrent(%client);
	}

	// EnvGui should receive variables from the environment the client is using
	function serverCmdEnvGui_RequestLists(%client)
	{
		if(%client.isAdmin)
			%client.currentEnvironment.setCurrent();

		parent::serverCmdEnvGui_RequestLists(%client);
	}

	// EnvGui should receive variables from the environment the client is using
	function serverCmdEnvGui_RequestCurrentVars(%client)
	{
		if(%client.isAdmin)
			%client.currentEnvironment.setCurrent();

		parent::serverCmdEnvGui_RequestCurrentVars(%client);
	}

	// EnvGui should apply changes to the environment the client is using
	function serverCmdEnvGui_ClickDefaults(%client)
	{
		if(%client.isAdmin)
			%client.currentEnvironment.setCurrent();

		parent::serverCmdEnvGui_ClickDefaults(%client);

		if(%client.isAdmin)
			%client.currentEnvironment.postEditCheck();
	}

	// EnvGui should apply changes to the environment the client is using
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

	// Clients should see the vignette of the environment they are using
	function EnvGuiServer::SendVignette(%client)
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
