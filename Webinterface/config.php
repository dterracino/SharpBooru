<?php

$socket_path = "/srv/booru/socket.sock";

$booru_name = "Eggy's Booru";
// $motd = "Rewriting complete layout";

$logo_link = "index.php";

$header_links = array(
	"house.svg" => "index.php",
	"tiles.svg" => "search.php",
	"dice.svg" => "random.php",
	"github.svg" => "random.php",
	"github.svg" => "https://github.com/teamalpha5441"
);

$header_links_loggedin = array(
	"upload.svg" => "upload.php"
);

$special_days = array(
	"01.05.",
	"31.10.",
	"24.12.",
	"25.12.",
	"31.12.",
	"01.01."
);

$thumbs_per_page = 60;
$thumb_size = 120;

?>
