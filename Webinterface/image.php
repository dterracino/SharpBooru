<?php

require_once("helper.php");
require_once("config.php");
require_once("booru.php");

$req_code = 1; //Get_Thumb
if (isset($_GET["type"]))
	if ($_GET["type"] == "image")
		$req_code = 2; //Get_Image

if (isset($_GET["id"]))
{
	//TODO 64Bit ID support
	$id = "\x00\x00\x00\x00" . pack("N", $_GET["id"]);

	try
	{
		//TODO Don't skip length uint32
		$data = substr($booru->request($req_code, $id), 4);

		header("Content-Type: " . get_mime($data));

		cache_headers(12 * 3600);
		$etag = etag_buffer($data);
		if (!etag_check($etag))
			echo $data;
	}
	catch (Exception $ex)
	{
		echo $ex->getMessage();
	}
}
else echo "ID not set";

?>
