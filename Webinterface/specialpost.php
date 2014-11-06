<?php

require_once("booru.php");

$posts = $booru->search("");

$mobile = false;
if (isset($_GET["target"]))
	if ($_GET["target"] == "mobile")
		$mobile = true;

$index = mt_rand(0, count($posts) - 1);
if (isset($_GET["type"]))
	if ($_GET["type"] == "newest")
		$index = 0;

$id = $posts[$index];

if ($mobile)
	$url = "post_m.php";
else $url = "post.php";

header("Location: " . $url . "?id=" . $id);
http_response_code(307);

?>
