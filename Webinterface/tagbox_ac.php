<?php

require_once("helper.php");
require_once("booru.php");

cache_headers(3600);

echo "[";
if (isset($_GET["term"]))
{
	$term = $_GET["term"];
	//TODO Do search in database, output as list
}
echo "]";

?>
