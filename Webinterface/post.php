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

		$contains_esoa = false;
		foreach ($post->tags as $tag)
			if ($tag->tag == "esoa")
			{
				$contains_esoa = true;
				break;
			}
		if ($contains_esoa)
		{
			echo '<div style="margin:26px auto 10px auto;width:100px;">';
			echo '<img alt="APPROVED" src="images/seal.png" width="100px" height="100px"></div>';
		}
		echo '<ul class="tags">';
		foreach ($post->tags as $tag)
		{
			echo '<li><span style="color:' . $tag->color . '">';
			echo '<a href="search.php?tags=' . urlencode($tag->tag) . '">';
			echo htmlspecialchars($tag->tag) . "</a></span></li>";
		}
		echo "</ul>";

		subsection_footer();
		subsection_header("User");
		echo '<a href="search.php?tags=user%3D' . $post->user . '">';
		echo htmlentities($post->user);
		if ($post->private)
			echo "</a> <i>(private)</i>";
		else echo "</a> <i>(public)</i>";
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
		subsection_header("IQDB");
		echo '<a href="http://iqdb.org/?url=';
		echo urlencode($server_base_url . "/image.php?id=" . $id);
		echo '">Search with thumbnail</a>';
		echo '<br><a href="http://iqdb.org/?url=';
		echo urlencode($server_base_url . "/image.php?type=image&id=" . $id);
		echo '">Search with image</a>';
		subsection_footer();
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
