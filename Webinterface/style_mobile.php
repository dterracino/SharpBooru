<?php
header("Content-Type: text/css");

require_once("helper.php");
require_once("config.php");

cache_headers(12 * 3600);

$current_date = date("d.m.", time());

if (in_array($current_date, $special_days))
	$background = "linear-gradient(to right, red, orange, yellow, green, blue, indigo, violet)";
else $background = "black";

$search_width = $mobile_width - 270;
if ($search_width > 300)
	$search_width = 300;

?>

div.main_mobile {
	width: <?php echo $mobile_width; ?>px;
}

div.header_mobile {
	width: <?php echo $mobile_width; ?>px;
	height: 52px;
	background: <?php echo $background; ?>;
}

div.search_wrapper {
	margin-right: 12px;
	margin-top: 12px;
	float: right;
}

input[type=text].search_mobile {
	width: <?php echo $search_width; ?>px;
}

div.body_mobile {
	margin: 5px;
}

div.page_chooser_mobile {
	text-align: center;
	margin-left: auto;
	margin-right: auto;
	width: 80%;
	margin-bottom: 19px;
	line-height: 25px;
}

img.mimg_mobile {
	display: block;
	width: <?php echo $mobile_width - 10; ?>px;
	height: auto;
}

div.login_mobile {
	background: black;
	text-align: right;
	margin-top: 5px;
	padding: 7px;
}
