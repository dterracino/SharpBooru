<?php

require_once("html.php");
require_once("booru.php");
require_once("config.php");

html_header("Booru");
flush();

echo '<div style="width: 400px; margin-left: auto; margin-right: auto; margin-top: 180px; text-align: center;">';

$count = (string)count($booru->search(""));
for ($i = 0; $i < strlen($count); $i++)
	echo '<img alt="' . $count[$i] . '" src="counter/' . $count[$i] . '.png">';

echo "<br><br>";

echo '<form action="search.php" method="GET">';
echo '<input style="width: 100%;" class="tagbox" type="text" name="tags">';
echo "</form>";

echo "</div>";
html_footer();

?>
