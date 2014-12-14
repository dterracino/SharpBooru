<?php

require_once("html.php");
require_once("booru.php");

html_header_ex("Booru - Tags", false, false, false);
flush();

$tags = $booru->searchTags("", 0);
foreach ($tags as $tag)
{
	echo '<span style="color: ' . $tag->color;
	echo ';">' . $tag->tag . "</span><br>";
}

html_footer();

?>
