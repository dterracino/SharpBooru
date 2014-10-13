<?php

require_once("helper.php");
require_once("booru.php");

cache_headers(3600);

echo "[ ";
if (isset($_GET["term"]))
{
	$parts = explode(" ", $_GET["term"]);
	$term = $parts[count($parts) - 1];
	if (count($parts) > 1)
		$alw = implode(" ", array_slice($parts, 0, count($parts) - 1)) . " ";
	else $alw = "";
	$boorutags = $booru->SearchTags($term);
	$tags = array();
	foreach ($boorutags as $boorutag)
		array_push($tags, '"' . $alw . $boorutag->tag . '"');
	echo implode(", ", $tags);
}
echo " ]";

?>
