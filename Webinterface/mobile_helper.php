<?php

require_once("Mobile_Detect.php");

$__is_mobile = false;

function is_mobile()
{
	global $__is_mobile;
	if ($__is_mobile == NULL)
	{
		$detect = new Mobile_Detect;
		$__is_mobile = $detect->isMobile() && !$detect->isTables();
	}
	return $__is_mobile;
}

?>
