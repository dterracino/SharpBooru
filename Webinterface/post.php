<?php

require_once("html.php");
require_once("booru.php");
require_once("config.php");

html_header("Booru");

if (isset($_GET["tags"]))
	$tag_search = $_GET["tags"];
else $tag_search = "";

$id = $_GET["id"];
if (!isset($id))
	$id_err = "No ID given";
else if (!is_numeric($id))
	$id_err = "ID is not numeric";

table_header("vtable");
nav_searchbox("index.php", $tag_search);
if (!isset($id_err))
{
	try
	{
		$post = $booru->getPost($id);        
		echo "<br>";
		subsection_header("Tags");
		foreach($post->tags as $tag)
		{
			echo '<span style="color:' . $tag->color . '">';
			$tag_encoded = htmlspecialchars($tag->tag);
			echo '<a href="index.php?tags=' . $tag_encoded . '">';
			echo $tag_encoded . "</a></span><br>";
		}
		subsection_footer();
		subsection("User", htmlentities($post->user));
		$source = htmlentities($post->source);
		if (filter_var($post->source, FILTER_VALIDATE_URL))
			$source = '<a href="' . $source . '">' . $source . "</a>";
		subsection("Source", $source);
		subsection("Rating", $post->rating);
		subsection("Size", $post->width . "x" . $post->height);
		$cdate = date("d.m.Y H:i", $post->date);
		subsection("Date", $cdate);
	}
	catch (Exception $ex) { $id_err = "No permission"; }
}
table_middle();
if (!isset($id_err))
	echo '<img id="mimg" class="mimg" alt="Image" src="image.php?type=image&amp;id=' . $id . '">';
else echo $id_err;
table_footer();

html_footer();

?>
