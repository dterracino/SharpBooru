<?php

require_once("booru.php");

if (isset($_GET["tags"]))
	$tag_search = $_GET["tags"];
else $tag_search = "";

$posts = $booru->search($tag_search);

$mobile = false;

$index = mt_rand(0, count($posts) - 1);
if (isset($_GET["type"]))
	if ($_GET["type"] == "newest")
		$index = 0;

$id = $posts[$index];

if (isset($_GET["target"]))
	if ($_GET["target"] == "mobile")
		$mobile = true;

if ($mobile)
	$url = "post_m.php";
else $url = "post.php";

header("Location: " . $url . "?id=" . $id);
http_response_code(307);

?>
