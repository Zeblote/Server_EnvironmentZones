function Environment()
{
	%this = new ScriptObject(Environment);
	return %this;
}

function Environment::onRemove(%this)
{
	%this.deleteObjects();

	for(%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%client = ClientGroup.getObject(%i);
		if(%client.currentEnvironment == %this)
			%client.setEnvironment($DefaultEnvironment);
	}
}

function Environment::deleteObjects(%this)
{
	if(isObject(%this.sky))
		%this.sky.delete();

	if(isObject(%this.sun))
		%this.sun.delete();

	if(isObject(%this.sunLight))
		%this.sunLight.delete();

	if(isObject(%this.dayCycle))
		%this.dayCycle.delete();

	if(isObject(%this.groundPlane))
		%this.groundPlane.delete();

	if(isObject(%this.waterPlane))
		%this.waterPlane.delete();

	if(isObject(%this.waterZone))
		%this.waterZone.delete();

	if(isObject(%this.rain))
		%this.rain.delete();
}

function Environment::copyFrom(%this, %other)
{
	if(!isObject(%other))
		return;

	%this.deleteObjects();

	if(isObject(%other.sky))
	{
		%name = %other.sky.getName();
		%other.sky.setName("Template");
		%this.sky = new Sky(Copy : Template).getId();
		%this.sky.setName("");
		%other.sky.setName(%name);
	}

	if(isObject(%other.sun))
	{
		%name = %other.sun.getName();
		%other.sun.setName("Template");
		%this.sun = new Sun(Copy : Template).getId();
		%this.sun.setName("");
		%other.sun.setName(%name);
	}

	if(isObject(%other.sunLight))
	{
		%name = %other.sunLight.getName();
		%other.sunLight.setName("Template");
		%this.sunLight = new FxSunLight(Copy : Template).getId();
		%this.sunLight.setName("");
		%other.sunLight.setName(%name);
	}

	if(isObject(%other.dayCycle))
	{
		%name = %other.dayCycle.getName();
		%other.dayCycle.setName("Template");
		%this.dayCycle = new FxDayCycle(Copy : Template).getId();
		%this.dayCycle.setName("");
		%other.dayCycle.setName(%name);
	}

	if(isObject(%other.groundPlane))
	{
		%name = %other.groundPlane.getName();
		%other.groundPlane.setName("Template");
		%this.groundPlane = new FxPlane(Copy : Template).getId();
		%this.groundPlane.setName("");
		%other.groundPlane.setName(%name);
	}

	if(isObject(%other.waterPlane))
	{
		%name = %other.waterPlane.getName();
		%other.waterPlane.setName("Template");
		%this.waterPlane = new FxPlane(Copy : Template).getId();
		%this.waterPlane.setName("");
		%other.waterPlane.setName(%name);
	}

	if(isObject(%other.waterZone))
	{
		%name = %other.waterZone.getName();
		%other.waterZone.setName("Template");
		%this.waterZone = new PhysicalZone(Copy : Template).getId();
		%this.waterZone.setName("");
		%other.waterZone.setName(%name);
	}

	if(isObject(%other.rain))
	{
		%name = %other.rain.getName();
		%other.rain.setName("Template");
		%this.rain = new Precipitation(Copy : Template).getId();
		%this.rain.setName("");
		%other.rain.setName(%name);
	}

	%this.setupNetFlags();

	if(%other.getId() != $CurrentEnvironment)
	{
		%this.var_AmbientLightColor = %other.var_AmbientLightColor;
		%this.var_DayCycleEnabled = %other.var_DayCycleEnabled;
		%this.var_DayCycleFile = %other.var_DayCycleFile;
		%this.var_DayCycleIdx = %other.var_DayCycleIdx;
		%this.var_DayLength = %other.var_DayLength;
		%this.var_DirectLightColor = %other.var_DirectLightColor;
		%this.var_FogColor = %other.var_FogColor;
		%this.var_FogDistance = %other.var_FogDistance;
		%this.var_FogHeight = %other.var_FogHeight;
		%this.var_GroundColor = %other.var_GroundColor;
		%this.var_GroundFile = %other.var_GroundFile;
		%this.var_GroundIdx = %other.var_GroundIdx;
		%this.var_GroundScrollX = %other.var_GroundScrollX;
		%this.var_GroundScrollY = %other.var_GroundScrollY;
		%this.var_HasSetAdvancedOnce = %other.var_HasSetAdvancedOnce;
		%this.var_ShadowColor = %other.var_ShadowColor;
		%this.var_SimpleMode = %other.var_SimpleMode;
		%this.var_SkyColor = %other.var_SkyColor;
		%this.var_SkyFile = %other.var_SkyFile;
		%this.var_SkyIdx = %other.var_SkyIdx;
		%this.var_SunAzimuth = %other.var_SunAzimuth;
		%this.var_SunElevation = %other.var_SunElevation;
		%this.var_SunFlareBottomIdx = %other.var_SunFlareBottomIdx;
		%this.var_SunFlareColor = %other.var_SunFlareColor;
		%this.var_SunFlareSize = %other.var_SunFlareSize;
		%this.var_SunFlareTopIdx = %other.var_SunFlareTopIdx;
		%this.var_UnderWaterColor = %other.var_UnderWaterColor;
		%this.var_VignetteColor = %other.var_VignetteColor;
		%this.var_VignetteMultiply = %other.var_VignetteMultiply;
		%this.var_VisibleDistance = %other.var_VisibleDistance;
		%this.var_WaterColor = %other.var_WaterColor;
		%this.var_WaterFile = %other.var_WaterFile;
		%this.var_WaterHeight = %other.var_WaterHeight;
		%this.var_WaterIdx = %other.var_WaterIdx;
		%this.var_WaterScrollX = %other.var_WaterScrollX;
		%this.var_WaterScrollY = %other.var_WaterScrollY;
		%this.var_WindEffectPrecipitation = %other.var_WindEffectPrecipitation;
		%this.var_WindVelocity = %other.var_WindVelocity;

		%this.simple_VignetteColor = %other.simple_VignetteColor;
		%this.simple_VignetteMultiply = %other.simple_VignetteMultiply;
	}
	else
		%this.copyVariables();

	%this.setCurrent();

	if(!$EnvGuiServer::SimpleMode)
	{
		if(!$EnvGuiServer::HasSetAdvancedOnce)
			EnvGuiServer::readAdvancedVarsFromSimple();

		EnvGuiServer::setAdvancedMode();
	}
	else
		EnvGuiServer::setSimpleMode();

}

function Environment::startEdit(%this)
{
	%this.nameObjects();
	%this.applyVariables();
}

function Environment::stopEdit(%this)
{
	//in case objects were created by the gui
	%this.sky = nameToId("Sky");
	%this.sun = nameToId("Sun");
	%this.sunLight = nameToId("SunLight");
	%this.dayCycle = nameToId("DayCycle");
	%this.groundPlane = nameToId("GroundPlane");
	%this.waterPlane = nameToId("WaterPlane");
	%this.waterZone = nameToId("WaterZone");
	%this.rain = nameToId("Rain");

	%this.unNameObjects();
	%this.copyVariables();
}

function Environment::copyVariables(%this)
{
	%this.var_AmbientLightColor = $EnvGuiServer::AmbientLightColor;
	%this.var_DayCycleEnabled = $EnvGuiServer::DayCycleEnabled;
	%this.var_DayCycleFile = $EnvGuiServer::DayCycleFile;
	%this.var_DayCycleIdx = $EnvGuiServer::DayCycleIdx;
	%this.var_DayLength = $EnvGuiServer::DayLength;
	%this.var_DirectLightColor = $EnvGuiServer::DirectLightColor;
	%this.var_FogColor = $EnvGuiServer::FogColor;
	%this.var_FogDistance = $EnvGuiServer::FogDistance;
	%this.var_FogHeight = $EnvGuiServer::FogHeight;
	%this.var_GroundColor = $EnvGuiServer::GroundColor;
	%this.var_GroundFile = $EnvGuiServer::GroundFile;
	%this.var_GroundIdx = $EnvGuiServer::GroundIdx;
	%this.var_GroundScrollX = $EnvGuiServer::GroundScrollX;
	%this.var_GroundScrollY = $EnvGuiServer::GroundScrollY;
	%this.var_HasSetAdvancedOnce = $EnvGuiServer::HasSetAdvancedOnce;
	%this.var_ShadowColor = $EnvGuiServer::ShadowColor;
	%this.var_SimpleMode = $EnvGuiServer::SimpleMode;
	%this.var_SkyColor = $EnvGuiServer::SkyColor;
	%this.var_SkyFile = $EnvGuiServer::SkyFile;
	%this.var_SkyIdx = $EnvGuiServer::SkyIdx;
	%this.var_SunAzimuth = $EnvGuiServer::SunAzimuth;
	%this.var_SunElevation = $EnvGuiServer::SunElevation;
	%this.var_SunFlareBottomIdx = $EnvGuiServer::SunFlareBottomIdx;
	%this.var_SunFlareColor = $EnvGuiServer::SunFlareColor;
	%this.var_SunFlareSize = $EnvGuiServer::SunFlareSize;
	%this.var_SunFlareTopIdx = $EnvGuiServer::SunFlareTopIdx;
	%this.var_UnderWaterColor = $EnvGuiServer::UnderWaterColor;
	%this.var_VignetteColor = $EnvGuiServer::VignetteColor;
	%this.var_VignetteMultiply = $EnvGuiServer::VignetteMultiply;
	%this.var_VisibleDistance = $EnvGuiServer::VisibleDistance;
	%this.var_WaterColor = $EnvGuiServer::WaterColor;
	%this.var_WaterFile = $EnvGuiServer::WaterFile;
	%this.var_WaterHeight = $EnvGuiServer::WaterHeight;
	%this.var_WaterIdx = $EnvGuiServer::WaterIdx;
	%this.var_WaterScrollX = $EnvGuiServer::WaterScrollX;
	%this.var_WaterScrollY = $EnvGuiServer::WaterScrollY;
	%this.var_WindEffectPrecipitation = $EnvGuiServer::WindEffectPrecipitation;
	%this.var_WindVelocity = $EnvGuiServer::WindVelocity;

	%this.simple_VignetteColor = $Sky::VignetteColor;
	%this.simple_VignetteMultiply = $Sky::VignetteMultiply;
}

function Environment::applyVariables(%this)
{
	$EnvGuiServer::AmbientLightColor = %this.var_AmbientLightColor;
	$EnvGuiServer::DayCycleEnabled = %this.var_DayCycleEnabled;
	$EnvGuiServer::DayCycleFile = %this.var_DayCycleFile;
	$EnvGuiServer::DayCycleIdx = %this.var_DayCycleIdx;
	$EnvGuiServer::DayLength = %this.var_DayLength;
	$EnvGuiServer::DirectLightColor = %this.var_DirectLightColor;
	$EnvGuiServer::FogColor = %this.var_FogColor;
	$EnvGuiServer::FogDistance = %this.var_FogDistance;
	$EnvGuiServer::FogHeight = %this.var_FogHeight;
	$EnvGuiServer::GroundColor = %this.var_GroundColor;
	$EnvGuiServer::GroundFile = %this.var_GroundFile;
	$EnvGuiServer::GroundIdx = %this.var_GroundIdx;
	$EnvGuiServer::GroundScrollX = %this.var_GroundScrollX;
	$EnvGuiServer::GroundScrollY = %this.var_GroundScrollY;
	$EnvGuiServer::HasSetAdvancedOnce = %this.var_HasSetAdvancedOnce;
	$EnvGuiServer::ShadowColor = %this.var_ShadowColor;
	$EnvGuiServer::SimpleMode = %this.var_SimpleMode;
	$EnvGuiServer::SkyColor = %this.var_SkyColor;
	$EnvGuiServer::SkyFile = %this.var_SkyFile;
	$EnvGuiServer::SkyIdx = %this.var_SkyIdx;
	$EnvGuiServer::SunAzimuth = %this.var_SunAzimuth;
	$EnvGuiServer::SunElevation = %this.var_SunElevation;
	$EnvGuiServer::SunFlareBottomIdx = %this.var_SunFlareBottomIdx;
	$EnvGuiServer::SunFlareColor = %this.var_SunFlareColor;
	$EnvGuiServer::SunFlareSize = %this.var_SunFlareSize;
	$EnvGuiServer::SunFlareTopIdx = %this.var_SunFlareTopIdx;
	$EnvGuiServer::UnderWaterColor = %this.var_UnderWaterColor;
	$EnvGuiServer::VignetteColor = %this.var_VignetteColor;
	$EnvGuiServer::VignetteMultiply = %this.var_VignetteMultiply;
	$EnvGuiServer::VisibleDistance = %this.var_VisibleDistance;
	$EnvGuiServer::WaterColor = %this.var_WaterColor;
	$EnvGuiServer::WaterFile = %this.var_WaterFile;
	$EnvGuiServer::WaterHeight = %this.var_WaterHeight;
	$EnvGuiServer::WaterIdx = %this.var_WaterIdx;
	$EnvGuiServer::WaterScrollX = %this.var_WaterScrollX;
	$EnvGuiServer::WaterScrollY = %this.var_WaterScrollY;
	$EnvGuiServer::WindEffectPrecipitation = %this.var_WindEffectPrecipitation;
	$EnvGuiServer::WindVelocity = %this.var_WindVelocity;

	$Sky::VignetteColor = %this.simple_VignetteColor;
	$Sky::VignetteMultiply = %this.simple_VignetteMultiply;
}

function Environment::nameObjects(%this)
{
	if(isObject(%this.sky))
		%this.sky.setName("Sky");

	if(isObject(%this.sun))
		%this.sun.setName("Sun");

	if(isObject(%this.sunLight))
		%this.sunLight.setName("SunLight");

	if(isObject(%this.dayCycle))
		%this.dayCycle.setName("DayCycle");

	if(isObject(%this.groundPlane))
		%this.groundPlane.setName("GroundPlane");

	if(isObject(%this.waterPlane))
		%this.waterPlane.setName("WaterPlane");

	if(isObject(%this.waterZone))
		%this.waterZone.setName("WaterZone");

	if(isObject(%this.rain))
		%this.rain.setName("Rain");
}

function Environment::unNameObjects(%this)
{
	if(isObject(%this.sky))
		%this.sky.setName("");
		
	if(isObject(%this.sun))
		%this.sun.setName("");

	if(isObject(%this.sunLight))
		%this.sunLight.setName("");

	if(isObject(%this.dayCycle))
		%this.dayCycle.setName("");

	if(isObject(%this.groundPlane))
		%this.groundPlane.setName("");

	if(isObject(%this.waterPlane))
		%this.waterPlane.setName("");

	if(isObject(%this.waterZone))
		%this.waterZone.setName("");

	if(isObject(%this.rain))
		%this.rain.setName("");
}

function Environment::setupNetFlags(%this)
{
	if(isObject(%this.sky))
	{
		if(GhostAlwaysSet.isMember(%this.sky))
			%this.sky.clearScopeAlways();

		%this.sky.setNetFlag(6, true);
	}

	if(isObject(%this.sun))
	{
		if(GhostAlwaysSet.isMember(%this.sun))
			%this.sun.clearScopeAlways();

		%this.sun.setNetFlag(6, true);
	}

	if(isObject(%this.sunLight))
	{
		if(GhostAlwaysSet.isMember(%this.sunLight))
			%this.sunLight.clearScopeAlways();

		%this.sunLight.setNetFlag(6, true);
	}

	if(isObject(%this.dayCycle))
	{
		if(GhostAlwaysSet.isMember(%this.dayCycle))
			%this.dayCycle.clearScopeAlways();

		%this.dayCycle.setNetFlag(6, true);
	}

	if(isObject(%this.groundPlane))
	{
		if(GhostAlwaysSet.isMember(%this.groundPlane))
			%this.groundPlane.clearScopeAlways();

		%this.groundPlane.setNetFlag(6, true);
	}

	if(isObject(%this.waterPlane))
	{
		if(GhostAlwaysSet.isMember(%this.waterPlane))
			%this.waterPlane.clearScopeAlways();

		%this.waterPlane.setNetFlag(6, true);
	}

	if(isObject(%this.waterZone))
	{
		if(GhostAlwaysSet.isMember(%this.waterZone))
			%this.waterZone.clearScopeAlways();

		%this.waterZone.setNetFlag(6, true);
	}

	if(isObject(%this.rain))
	{
		if(GhostAlwaysSet.isMember(%this.rain))
			%this.rain.clearScopeAlways();

		%this.rain.setNetFlag(6, true);
	}
}

function Environment::scopeToClient(%this, %client)
{
	if(isObject(%this.sky))
		%this.sky.scopeToClient(%client);

	if(isObject(%this.sun))
		%this.sun.scopeToClient(%client);

	if(isObject(%this.sunLight))
		%this.sunLight.scopeToClient(%client);

	if(isObject(%this.dayCycle))
		%this.dayCycle.scopeToClient(%client);

	if(isObject(%this.groundPlane))
		%this.groundPlane.scopeToClient(%client);

	if(isObject(%this.waterPlane))
		%this.waterPlane.scopeToClient(%client);

	if(isObject(%this.waterZone))
		%this.waterZone.scopeToClient(%client);

	if(isObject(%this.rain))
		%this.rain.scopeToClient(%client);
}

function Environment::clearScopeToClient(%this, %client)
{
	if(isObject(%this.sky))
		%this.sky.clearScopeToClient(%client);

	if(isObject(%this.sun))
		%this.sun.clearScopeToClient(%client);

	if(isObject(%this.sunLight))
		%this.sunLight.clearScopeToClient(%client);

	if(isObject(%this.dayCycle))
		%this.dayCycle.clearScopeToClient(%client);

	if(isObject(%this.groundPlane))
		%this.groundPlane.clearScopeToClient(%client);

	if(isObject(%this.waterPlane))
		%this.waterPlane.clearScopeToClient(%client);

	if(isObject(%this.waterZone))
		%this.waterZone.clearScopeToClient(%client);

	if(isObject(%this.rain))
		%this.rain.clearScopeToClient(%client);
}

function Environment::setCurrent(%this)
{
	if($CurrentEnvironment == %this.getId())
		return;

	if(isObject($CurrentEnvironment))
		$CurrentEnvironment.stopEdit();

	$CurrentEnvironment = %this.getId();
	%this.startEdit();
}

function Environment::postEditCheck(%this)
{
	%this.stopEdit();
	%this.setupNetFlags();
	%this.startEdit();

	for(%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%client = ClientGroup.getObject(%i);
		if(%client.currentEnvironment == %this)
			%this.scopeToClient(%client);
	}
}
