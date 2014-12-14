<?php

require_once("html.php");
require_once("booru.php");

$mobile = false;
$search_page = "search.php";
if (isset($_GET["target"]))
	if ($_GET["target"] == "mobile")
	{
		$mobile = false;
		$search_page = "search_m.php";
	}

if ($mobile)
	html_header_mobile_ex("Booru - Tags", "", false, false, false); //TODO Search value
else html_header_ex("Booru - Tags", false, false, false);

flush();

$tags = $booru->searchTags("", 0);

usort($tags, function($a, $b) { return strcmp($a->tag, $b->tag); });
usort($tags, function($a, $b) { return strcmp($a->color, $b->color); });

foreach ($tags as $tag)
{
	echo '<a href="' . $search_page . '?tags=' . urlencode($tag->tag);
	echo '"><span style="color: ' . $tag->color;
	echo ';">' . htmlspecialchars($tag->tag) . "</span></a><br>";
}

html_footer();

?>
