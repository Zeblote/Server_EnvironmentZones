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
	}

	function serverCmdEnvGui_SetVar(%client, %varName, %value)
	{
		if(%client.isAdmin)
			%client.currentEnvironment.setCurrent();

		parent::serverCmdEnvGui_SetVar(%client, %varName, %value);
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
			commandToClient(%client, 'setVignette', $EnvGuiServer::VignetteMultiply, $EnvGuiServer::VignetteColor);
		else
			commandToClient(%client, 'setVignette', %client.currentEnvironment.var_VignetteMultiply, %client.currentEnvironment.var_VignetteColor);
	}
};

activatePackage(EnvironmentZones);
