<?php

require_once("helper.php");
require_once("booru.php");

cache_headers(3600);

echo "[";
if (isset($_GET["term"]))
{
	$boorutags = $booru->SearchTags($_GET["term"]);
	$tags = array();
	foreach ($boorutags as $boorutag)
		array_push($tags, $boorutag->tag);
	echo implode(", ", $tags);
}
echo "]";

?>
