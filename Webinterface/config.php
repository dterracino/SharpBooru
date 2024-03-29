<?php

$socket_path = "/srv/booru/socket.sock";

$booru_name = "Eggy's Booru";
// $motd = "Put your MOTD here";

$logo_link = "index.php";

$special_days = array(
	"01.05.",
	"31.10.",
	"24.12.",
	"25.12.",
	"31.12.",
	"01.01."
);

$upload_default_tags = "todo_tags todo_source";

$thumbs_per_page = 60;
$thumb_size = 120;

$mobile_thumbs_per_row = 3;
$mobile_width = (12 + $thumb_size) * $mobile_thumbs_per_row + 10;

if (isset($_SERVER['HTTPS']))
	$server_base_url = "https://";
else $server_base_url = "http://";

$server_base_url .= $_SERVER['SERVER_NAME'];

?>
