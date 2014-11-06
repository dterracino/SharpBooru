<?php
require_once("session.php");

require_once("helper.php");
require_once("html.php");
require_once("booru.php");
require_once("config.php");

function isImage($buffer)
{
	$mimeTypes = array("image/jpeg", "image/png", "image/gif");
	return in_array(get_mime($buffer), $mimeTypes);
}

$mobile = false;
if (isset($_GET["target"]))
	if ($_GET["target"] == "mobile")
		$mobile = true;

if ($mobile)
{
	if (isset($_GET["tags"]))
		$tag_search = $_GET["tags"];
	else $tag_search = "";

	html_header_mobile("Booru - Upload", $tag_search);
}
else html_header("Booru - Upload");

if (session_loggedin())
{
	if (isset($_FILES["file"]))
	{
		$error = $_FILES["file"]["error"];
		if ($error > 0)
		{
			if ($error == 1 || $error == 2)
				echo "File too big";
			else if ($error == 4)
				echo "No file uploaded";
			else echo "Upload failed";
		}
		else
		{
			$file = file_get_contents($_FILES["file"]["tmp_name"]);
			if (isImage($file))
			{
				$user = session_username();
				$tags = $_POST["tags"];
				$source = $_POST["source"];
				$description = $_POST["description"];
				$rating = $_POST["rating"];

				if (isset($_POST["private"]))
				{
					$private = $_POST["private"];
					if ($private == "off" || $private == false)
						$private = false;
					else if($private == "on" || $private == true)
						$private = true;
				}
				else $private = false;

				if (empty($source))
					echo "Please provide a source";
				else if (empty($rating))
					echo "Please provida a rating";
				else if (empty($tags))
					echo "Please provide at least one tag";
				else
				{
					$booru->addPost($user, $tags, $source, $description, $rating, $private, $file);
					echo "Upload finished";
				}
			}
			else echo "Invalid format";
		}
		echo "<br><br>";
	}

	if ($mobile)
		$input_width = 400;
	else $input_width = 600;

	echo '<form method="POST" enctype="multipart/form-data">';
	echo '<input type="hidden" name="MAX_FILE_SIZE" value="67108864">';
	echo "<table>";
	echo '<tr><td>File</td><td><input type="file" name="file" accept="image/*"></td></tr>';

	if ($mobile)
		echo '<tr><td>Tags</td><td><textarea name="tags" style="width: ' . $input_width . 'px; resize: none;" rows="7" class="tagbox">' . $upload_default_tags . '</textarea></td></tr>';
	else echo '<tr><td>Tags</td><td><textarea name="tags" style="width: ' . $input_width . 'px;" rows="4" class="tagbox">' . $upload_default_tags . '</textarea></td></tr>';

	echo '<tr><td>Source</td><td><input style="width: ' . $input_width . 'px;" type="text" name="source" value="Online Upload"></td></tr>';
	echo '<tr><td>Description</td><td><input style="width: ' . $input_width . 'px;" type="text" name="description"></td></tr>';
	echo '<tr><td>Rating</td><td><input style="width: 40px;" type="number" name="rating" value="7"></td></tr>';
	echo '<tr><td>Private</td><td><input type="checkbox" name="private"></td></tr>'; // checked="checked"
	echo '<tr><td></td><td><input type="submit" value="Upload"></td></tr>';
	echo "</table></form>";
}
else echo "Please login first";

html_footer();

?>
