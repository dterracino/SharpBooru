<?php

require_once("html.php");
require_once("booru.php");
require_once("config.php");

if (isset($_GET["tags"]))
	$tag_search = $_GET["tags"];
else $tag_search = "";

$id = $_GET["id"];
if (!isset($id))
	$id_err = "No ID given";
else if (!is_numeric($id))
	$id_err = "ID is not numeric";

html_header("Booru - Post " . $id);
flush();

table_header(NULL);
nav_searchbox($tag_search);
if (!isset($id_err))
{
	flush();
	try
	{
		$post = $booru->getPost($id);
		echo "<br>";
		subsection_header("Tags");
		echo '<ul class="tags">';
		foreach($post->tags as $tag)
		{
			echo '<li><span style="color:' . $tag->color . '">';
			$tag_encoded = htmlspecialchars($tag->tag);
			echo '<a href="search.php?tags=' . $tag_encoded . '">';
			echo $tag_encoded . "</a></span></li>";
		}
		echo "</ul>";
		subsection_footer();
		subsection_header("User");
		echo htmlentities($post->user);
		if ($post->private)
			echo " <i>(private)</i>";
		else echo " <i>(public)</i>";
		subsection_footer();
		$source = htmlentities($post->source);
		if (filter_var($post->source, FILTER_VALIDATE_URL))
			$source = '<a href="' . $source . '">' . $source . "</a>";
		subsection("Source", $source);
		if (!empty($post->description))
		{
			$description = htmlentities($post->description);
			subsection("Description", $post->description);
		}
		subsection("Rating", $post->rating);
		subsection("Size", $post->width . "x" . $post->height);
		$cdate = date("d.m.Y H:i", $post->date);
		subsection("Date", $cdate);
	}
	catch (Exception $ex) { $id_err = "Not found or no permission"; }
}
table_middle();
if (!isset($id_err))
	echo '<img id="mimg" class="mimg" alt="Image" src="image.php?type=image&amp;id=' . $id . '">';
else echo $id_err;
table_footer();

html_footer();

?>
