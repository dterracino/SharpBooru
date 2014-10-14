<?php

require_once("booru.php");

$posts = $booru->search("");
$index = mt_rand(0, count($posts) - 1);

$id = $posts[$index];

header("Location: post.php?id=" . $id);
http_response_code(307);

?>
