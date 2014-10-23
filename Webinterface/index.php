<?php

require_once("html.php");
require_once("booru.php");
require_once("config.php");

html_header("Booru");
flush();

$counter = (string)$booru->postCount();
$counter_len = strlen($counter);

$div_width = 68 * ($counter_len + 1);
echo '<div style="width: ' . $div_width . 'px; margin-left: auto; margin-right: auto; margin-top: 180px; text-align: center;">';

for ($i = 0; $i < $counter_len; $i++)
	echo '<img alt="' . $counter[$i] . '" src="images/counter/' . $counter[$i] . '.png">';

echo "<br><br>";

echo '<form action="search.php" method="GET">';
echo '<input style="width: 100%;" class="tagbox" type="text" name="tags">';
echo "</form>";

echo '<span style="font-size: 10px;">';
echo "Running SharpBooru by Eggy";
echo "</span>";

echo "</div>";
html_footer();

?>
