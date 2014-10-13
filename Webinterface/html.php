<?php
require_once("session.php");

require_once("config.php");
require_once("session.php");

function html_header($title)
{
	global $header_name, $header_motd, $header_links, $header_links_loggedin;

	echo "<!DOCTYPE html>";
	echo "<html><head><title>";
	echo $title . "</title>";
	echo '<link rel="stylesheet" type="text/css" href="style_static.css">';
	echo '<link rel="stylesheet" type="text/css" href="style_dynamic.php">';
	echo '<script type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/jquery/2.1.1/jquery.min.js"></script>';
	echo '<script type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/jqueryui/1.11.1/jquery-ui.min.js"></script>';
	echo '<script type="text/javascript" src="script.js"></script>';
	echo '<link rel="icon" type="image/icon" href="favicon.ico">';
	echo '<meta charset="utf-8">';
	echo '</head><body><div class="main">';
	echo '<div class="header"><div class="innerheader">';
	echo '<span class="hl"><a href="index.php">' . $header_name . '</a></span><br>';

	//Links
	if (session_loggedin())
		$header_links = array_merge($header_links, $header_links_loggedin);
	$link_count = count($header_links);
	$i = 0;
	foreach ($header_links as $key => $value)
	{
		echo '<a href="' . $value . '">' . $key . "</a>";
		if (!(++$i === $link_count))
			echo " | ";
	}

	if (isset($header_motd))
	{
		echo '</div><div class="motd"><b>Notice:</b> ';
		echo $header_motd;
	}

	echo '</div><div class="login">';
	session_printform();
	
	echo '</div></div><div class="body">';
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

function nav_searchbox($action, $value)
{
	echo '<form action="' . $action . '" method="GET">';
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
