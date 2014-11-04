<?php

require_once("booru.php");

$posts = $booru->search("");
$index = 0;
$id = $posts[$index];

header("Location: post.php?id=" . $id);
http_response_code(307);

?>
