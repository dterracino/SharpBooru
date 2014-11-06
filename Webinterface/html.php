<?php
require_once("session.php");

require_once("config.php");
require_once("session.php");

function _html_header_common($title)
{
	echo "<!DOCTYPE html>";
	echo "<html><head><title>" . $title . "</title>";
	echo '<link rel="stylesheet" type="text/css" href="style_static.css">';
	echo '<link rel="stylesheet" type="text/css" href="style_dynamic.php">';
	echo '<script type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/jquery/2.1.1/jquery.min.js"></script>';
	echo '<script type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/jqueryui/1.11.1/jquery-ui.min.js"></script>';
	echo '<script type="text/javascript" src="script.js"></script>';
	echo '<link rel="icon" type="image/icon" href="favicon.ico">';
	echo '<meta charset="utf-8">';
}

function html_header($title)
{
	global $booru_name, $motd, $header_links, $header_links_loggedin, $logo_link;

	_html_header_common($title);
	echo "</head><body>";
	echo '<div class="main">';
	echo '<div class="header">';
	echo '<a href="' . $logo_link . '">';
	echo '<img class="title" alt="' . $booru_name . '" src="images/title.svg">';
	echo "</a>";

	$header_links = array(
		"house.svg" => "index.php",
		"tiles.svg" => "search.php",
		"new.svg" => "specialpost.php?type=newest",
		"dice.svg" => "specialpost.php?type=random",
		"github.svg" => "https://github.com/teamalpha5441",
		"mobile.svg" => "search_m.php"
	);
	$header_links_loggedin = array(
		"upload.svg" => "upload.php"
	);
	if (session_loggedin())
		$header_links = array_merge($header_links, $header_links_loggedin);
	$link_count = count($header_links);
	foreach ($header_links as $key => $value)
	{
		echo '<div class="link"><a href="' . $value . '">';
		echo '<img alt="' . $value . '" src="images/' . $key . '">';
		echo "</a></div>";
	}

	if (isset($motd))
		echo '<div class="motd"><b>Notice:</b> ' . $motd . "</div>";

	echo '<div class="login">';
	session_printform();
	
	echo '</div></div><div class="body">';
}

function html_header_mobile($title, $search_value)
{
	global $booru_name, $header_links, $header_links_loggedin, $logo_link, $mobile_width;

	_html_header_common($title);
	echo '<meta name="viewport" content="width=' . $mobile_width . '" >';
	echo '<link rel="stylesheet" type="text/css" href="style_mobile.php">';
	echo "</head><body>";
	echo '<div class="main_mobile">';
	echo '<div class="header_mobile">';
//	echo '<a href="' . $logo_link . '">';
	echo '<img class="title" alt="' . $booru_name . '" src="images/title.svg">';
//	echo "</a>";

	$header_links = array(
//		"house.svg" => "index.php",
		"tiles.svg" => "search_m.php",
		"new.svg" => "specialpost.php?target=mobile&amp;type=newest",
		"dice.svg" => "specialpost.php?target=mobile&amp;type=random"
//		"github.svg" => "https://github.com/teamalpha5441",
	);
	$header_links_loggedin = array(
//		"upload.svg" => "upload.php"
	);
	if (session_loggedin())
		$header_links = array_merge($header_links, $header_links_loggedin);
	$link_count = count($header_links);
	foreach ($header_links as $key => $value)
	{
		echo '<div class="link"><a href="' . $value . '">';
		echo '<img alt="' . $value . '" src="images/' . $key . '">';
		echo "</a></div>";
	}

	echo '<div class="search_wrapper"><form action="search_m.php" method="GET">';
	echo '<input class="search_mobile tagbox" type="text" name="tags" value="';
	echo $search_value . '"></form></div></div><div class="body_mobile">';
}

function html_footer()
{
	echo "</div></div>";
	echo "</body></html>";
}

function table_header($class)
{
	if (isset($class))
		echo '<table class="' . $class . '">';
	else echo "<table>";
	echo '<tr><td class="nav">';
}

function table_middle() { echo '</td><td style="padding-left:14px;">'; }
function table_footer() { echo '</td></tr></table>'; }

function nav_searchbox($value)
{
	echo '<form action="search.php" method="GET">';
	echo '<input class="search tagbox" type="text" name="tags" value="';
	echo $value . '"></form>';
}

function subsection_header($name) { echo '<div class="sub"><h2>' . $name . '</h2><p>'; }
function subsection_footer() { echo "</p></div>"; }

function subsection($name, $content)
{
	subsection_header($name);
	echo $content;
	subsection_footer();
}

?>
